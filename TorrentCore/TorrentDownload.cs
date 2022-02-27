// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Data;
using TorrentCore.Tracker;
using TorrentCore.Utils;

namespace TorrentCore;

public sealed class TorrentDownload
{
    private readonly PipelineRunner _download;

    internal TorrentDownload(PipelineRunner download)
    {
        _download = download;
    }

    internal PipelineRunner Manager => _download;

    public Metainfo Description => Manager.Description;

    public DownloadState State => Manager.State;

    public double Progress => Manager.DownloadProgress;

    public IReadOnlyCollection<ITrackerDetails> Trackers => ((AggregatedTracker)Manager.Tracker).Trackers;

    public void Start()
    {
        _download.Start();
    }

    public void Stop()
    {
        _download.Stop();
    }

    public Task WaitForDownloadCompletionAsync(TimeSpan? timeout = default, CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            bool completed = false;
            var completionEvent = new ManualResetEvent(false);
            _download.ApplicationProtocol.DownloadCompleted += (sender, args) =>
            {
                completed = true;
                completionEvent.Set();
            };

            if (timeout == null)
                await completionEvent.WaitOneAsync().WaitAsync(cancellationToken);
            else
                await completionEvent.WaitOneAsync().WaitAsync(timeout.Value, cancellationToken);

            if (!completed)
                throw new TaskCanceledException("Download was cancelled before completion.");
        });
    }

    /// <summary>
    /// Gets the average download rate in bytes per second.
    /// </summary>
    public long DownloadRate() => _download.DownloadRateMeasurer.AverageRate();

    /// <summary>
    /// Gets the average upload rate in bytes per second.
    /// </summary>
    public long UploadRate() => _download.UploadRateMeasurer.AverageRate();
}
