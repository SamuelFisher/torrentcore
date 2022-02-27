// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TorrentCore.Application;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Application.Pipelines;
using TorrentCore.Data;
using TorrentCore.Engine;
using TorrentCore.Tracker;

namespace TorrentCore;

/// <summary>
/// Runs a Torrent pipeline.
/// </summary>
class PipelineRunner : ITorrentPipelineRunner
{
    private readonly ILogger<PipelineRunner> _logger;
    private readonly PeerId _localPeerId;
    private readonly IMainLoop _mainLoop;
    private readonly IServiceProvider _parentContainer;
    private readonly IPipelineFactory _pipelineFactory;
    private readonly StageInterrupt _stageInterrupt;
    private readonly Progress<StatusUpdate> _progress;

    private IPipeline? _pipeline;
    private volatile int _recentlyDownloaded;
    private volatile int _recentlyUploaded;

    private bool _isRunning;

    public PipelineRunner(
        ILogger<PipelineRunner> logger,
        PeerId localPeerId,
        IMainLoop mainLoop,
        IApplicationProtocol applicationProtocol,
        ITracker tracker,
        IServiceProvider parentContainer,
        IPipelineFactory pipelineFactory)
    {
        _logger = logger;
        _localPeerId = localPeerId;
        _mainLoop = mainLoop;
        ApplicationProtocol = applicationProtocol;
        Description = applicationProtocol.Metainfo;
        _parentContainer = parentContainer;
        _pipelineFactory = pipelineFactory;
        Tracker = tracker;
        State = DownloadState.Pending;
        DownloadRateMeasurer = new RateMeasurer();
        UploadRateMeasurer = new RateMeasurer();
        _progress = new Progress<StatusUpdate>();
        _progress.ProgressChanged += ProgressChanged;

        _stageInterrupt = new StageInterrupt();
    }

    public IApplicationProtocol ApplicationProtocol { get; }

    internal ITracker Tracker { get; }

    /// <summary>
    /// Gets the metainfo describing the collection of files.
    /// </summary>
    public Metainfo Description { get; }

    /// <summary>
    /// Gets the number of bytes downloaded so far.
    /// </summary>
    public long Downloaded => ApplicationProtocol.DataHandler.CompletedPieces.Sum(x => x.Size);

    /// <summary>
    /// Gets the number of bytes uploaded.
    /// </summary>
    public long Uploaded => ApplicationProtocol.Uploaded;

    /// <summary>
    /// Gets the number of bytes still to be downloaded.
    /// </summary>
    public long Remaining => Description.TotalSize - Downloaded;

    /// <summary>
    /// Gets a value indicating the percentage of data that has been downloaded.
    /// </summary>
    public double DownloadProgress => (double)Downloaded / Description.TotalSize;

    /// <summary>
    /// Gets the current state of the download.
    /// </summary>
    public DownloadState State { get; private set; }

    /// <summary>
    /// Gets the RateMeasurer used to measure the download rate.
    /// </summary>
    public RateMeasurer DownloadRateMeasurer { get; }

    /// <summary>
    /// Gets the RateMeasurer used to measure the upload rate.
    /// </summary>
    public RateMeasurer UploadRateMeasurer { get; }

    public void Start()
    {
        if (_isRunning)
            throw new InvalidOperationException("Already started.");
        _isRunning = true;

        _stageInterrupt.Reset();

        Task.Run(async () =>
        {
            await ContactTracker();

            using (var pipelineScope = _parentContainer.CreateScope())
            {
                _pipeline = _pipelineFactory.CreatePipeline(pipelineScope.ServiceProvider, ApplicationProtocol);
                _pipeline.Run(_stageInterrupt, _progress);
            }
        });
    }

    private async Task ContactTracker()
    {
        _logger.LogInformation("Contacting tracker");

        try
        {
            var request = new AnnounceRequest(
                _localPeerId,
                Remaining,
                Downloaded,
                Uploaded,
                Description.InfoHash);

            var result = await Tracker.Announce(request);

            _logger.LogInformation($"{result.Peers.Count} peers available");

            ApplicationProtocol.PeersAvailable(result.Peers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to contact tracker");

            // Cannot connect to tracker
            State = DownloadState.Error;
        }
    }

    public void Pause()
    {
        _isRunning = false;
        _stageInterrupt.Pause();
    }

    public void Stop()
    {
        _isRunning = false;
        _stageInterrupt.Stop();
    }

    public void Dispose()
    {
        if (_isRunning)
            Stop();
    }

    private void ProgressChanged(object? sender, StatusUpdate e)
    {
        _logger.LogTrace($"Progress: {e}");
        State = e.State;
    }

    internal void UpdateStatistics()
    {
        DownloadRateMeasurer.AddMeasure(_recentlyDownloaded);
        _recentlyDownloaded = 0;

        UploadRateMeasurer.AddMeasure(_recentlyUploaded);
        _recentlyUploaded = 0;
    }
}
