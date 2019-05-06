// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TorrentCore.Data;
using TorrentCore.Tracker;

namespace TorrentCore
{
    public sealed class TorrentDownload
    {
        private readonly TorrentDownloadManager _download;

        internal TorrentDownload(TorrentDownloadManager download)
        {
            _download = download;
        }

        internal TorrentDownloadManager Manager => _download;

        public Metainfo Description => Manager.Description;

        public DownloadState State => Manager.State;

        public double Progress => Manager.DownloadProgress;

        public IReadOnlyCollection<ITrackerDetails> Trackers => (Manager.Tracker as AggregatedTracker)?.Trackers;

        public void Start()
        {
            _download.Start();
        }

        public void Stop()
        {
            _download.Stop();
        }

        public Task WaitForDownloadCompletionAsync(TimeSpan? timeout = null)
        {
            return Task.Run(() =>
            {
                bool completed = false;
                var completionEvent = new ManualResetEvent(false);
                _download.ApplicationProtocol.DownloadCompleted += (sender, args) =>
                {
                    completed = true;
                    completionEvent.Set();
                };

                if (timeout == null)
                    completionEvent.WaitOne();
                else
                    completionEvent.WaitOne(timeout.Value);

                if (!completed)
                    throw new TimeoutException("Download did not complete within the specified timeout.");
            });
        }
    }
}
