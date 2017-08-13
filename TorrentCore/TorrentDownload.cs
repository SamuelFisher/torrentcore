// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2016 Sam Fisher.
// 
// TorrentCore is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, version 3.
// 
// TorrentCore is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with TorrentCore.  If not, see <http://www.gnu.org/licenses/>.

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
        private readonly TorrentDownloadManager download;

        internal TorrentDownload(TorrentDownloadManager download)
        {
            this.download = download;
        }

        internal TorrentDownloadManager Manager => download;

        public Metainfo Description => Manager.Description;

        public DownloadState State => Manager.State;

        public double Progress => Manager.DownloadProgress;

        public IReadOnlyCollection<ITrackerDetails> Trackers => (Manager.Tracker as AggregatedTracker)?.Trackers;

        public void Start()
        {
            download.Start();
        }

        public void Stop()
        {
            download.Stop();
        }

        public Task WaitForDownloadCompletionAsync(TimeSpan? timeout = null)
        {
            return Task.Run(() =>
            {
                bool completed = false;
                var completionEvent = new ManualResetEvent(false);
                download.ApplicationProtocol.DownloadCompleted += (sender, args) =>
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
