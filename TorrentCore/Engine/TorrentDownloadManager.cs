// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using TorrentCore.Application;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Data;
using TorrentCore.Engine;
using TorrentCore.Stage;
using TorrentCore.Tracker;

namespace TorrentCore
{
    /// <summary>
    /// Manages the download of a torrent by executing the pipeline and keeping track of downloaded data.
    /// </summary>
    class TorrentDownloadManager : ITorrentDownloadManager
    {
        private static readonly ILogger Log = LogManager.GetLogger<TorrentDownloadManager>();

        private readonly PeerId _localPeerId;
        private readonly IMainLoop _mainLoop;
        private readonly Pipeline _pipeline;
        private readonly StageInterrupt _stageInterrupt;
        private readonly Progress<StatusUpdate> _progress;

        private volatile int _recentlyDownloaded;
        private volatile int _recentlyUploaded;

        private bool _isRunning;

        internal TorrentDownloadManager(PeerId localPeerId,
                                        IMainLoop mainLoop,
                                        IApplicationProtocol<PeerConnection> applicationProtocol,
                                        ITracker tracker,
                                        Metainfo description)
        {
            _localPeerId = localPeerId;
            _mainLoop = mainLoop;
            ApplicationProtocol = applicationProtocol;
            Description = description;
            Tracker = tracker;
            State = DownloadState.Pending;
            Downloaded = 0;
            DownloadRateMeasurer = new RateMeasurer();
            UploadRateMeasurer = new RateMeasurer();
            _progress = new Progress<StatusUpdate>();
            _progress.ProgressChanged += ProgressChanged;

            _pipeline = new PipelineBuilder()
                .AddStage<VerifyDownloadedPiecesStage>()
                .AddStage<DownloadPiecesStage>()
                .Build();

            _stageInterrupt = new StageInterrupt();
        }

        internal IApplicationProtocol<PeerConnection> ApplicationProtocol { get; }

        internal ITracker Tracker { get; }

        /// <summary>
        /// Gets the metainfo describing the collection of files.
        /// </summary>
        public Metainfo Description { get; }

        /// <summary>
        /// Gets or sets the number of bytes downloaded so far.
        /// </summary>
        public long Downloaded { get; set; }

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

                using (var container = new Container())
                {
                    container.RegisterSingleton(ApplicationProtocol);
                    container.RegisterSingleton(_mainLoop);
                    container.RegisterSingleton<IPiecePicker>(new PiecePicker());

                    _pipeline.Run(container, _stageInterrupt, _progress);
                }
            });
        }

        private async Task ContactTracker()
        {
            Log.LogInformation("Contacting tracker");

            try
            {
                var request = new AnnounceRequest(
                    _localPeerId,
                    Remaining,
                    Description.InfoHash);

                var result = await Tracker.Announce(request);

                Log.LogInformation($"{result.Peers.Count} peers available");

                ApplicationProtocol.PeersAvailable(result.Peers);
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                Log.LogError(default(EventId), ex, "Unable to contact tracker");

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

        private void ProgressChanged(object sender, StatusUpdate e)
        {
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
}
