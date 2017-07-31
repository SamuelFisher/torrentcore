// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2017 Sam Fisher.
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
using System.Text;
using System.Threading.Tasks;

namespace TorrentCore.Tracker
{
    /// <summary>
    /// Manages the communication with multiple trackers, while appearing as a single tracker.
    /// </summary>
    class AggregatedTracker : ITracker
    {
        private readonly ITrackerClientFactory trackerClientFactory;
        private readonly IReadOnlyList<IReadOnlyList<Uri>> trackers;

        private readonly Dictionary<ITracker, TrackerStatistics> activeTrackers;

        public AggregatedTracker(ITrackerClientFactory trackerClientFactory, IReadOnlyList<IReadOnlyList<Uri>> trackers)
        {
            this.trackerClientFactory = trackerClientFactory;
            this.trackers = trackers;
            activeTrackers = new Dictionary<ITracker, TrackerStatistics>();
        }

        public string Type => throw new NotSupportedException();

        public IReadOnlyCollection<ITrackerDetails> Trackers => activeTrackers.Values;

        public async Task<AnnounceResult> Announce(AnnounceRequest request)
        {
            if (!activeTrackers.Any())
                CreateCandidateTrackers();

            var announceTime = DateTime.Now;
            var announceTasks = activeTrackers.Select(tracker =>
            {
                tracker.Value.LastAnnounce = announceTime;
                return new
                {
                    AnnounceTask = tracker.Key.Announce(request),
                    Statistics = tracker.Value
                };
            }).ToArray();
            await Task.WhenAll(announceTasks.Select(x => x.AnnounceTask));

            foreach (var tracker in announceTasks)
                tracker.Statistics.Peers += tracker.AnnounceTask.Result.Peers.Count;

            var peers = announceTasks.SelectMany(x => x.AnnounceTask.Result.Peers);
            return new AnnounceResult(peers);
        }

        private void CreateCandidateTrackers()
        {
            // TODO: currently just take the first tracker.
            var trackerUri = trackers.First().First();
            var tracker = trackerClientFactory.CreateTrackerClient(trackerUri);
            activeTrackers.Add(tracker, new TrackerStatistics
            {
                Uri = trackerUri,
                Type = tracker.Type
            });
        }

        private class TrackerStatistics : ITrackerDetails
        {
            public Uri Uri { get; set; }
            public int Peers { get; set; }
            public DateTime? LastAnnounce { get; set; }
            public string Type { get; set; }
        }
    }
}
