// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Tracker;

/// <summary>
/// Manages the communication with multiple trackers, while appearing as a single tracker.
/// </summary>
class AggregatedTracker : ITracker
{
    private readonly ITrackerClientFactory _trackerClientFactory;
    private readonly IReadOnlyList<IReadOnlyList<Uri>> _trackers;

    private readonly Dictionary<ITracker, TrackerStatistics> _activeTrackers;

    public AggregatedTracker(ITrackerClientFactory trackerClientFactory, IReadOnlyList<IReadOnlyList<Uri>> trackers)
    {
        _trackerClientFactory = trackerClientFactory;
        _trackers = trackers;
        _activeTrackers = new Dictionary<ITracker, TrackerStatistics>();
    }

    public string Type => throw new NotSupportedException();

    public IReadOnlyCollection<ITrackerDetails> Trackers => _activeTrackers.Values;

    public async Task<AnnounceResult> Announce(AnnounceRequest request)
    {
        if (!_activeTrackers.Any())
            CreateCandidateTrackers();

        var announceTime = DateTime.Now;
        var announceTasks = _activeTrackers.Select(tracker =>
        {
            tracker.Value.LastAnnounce = announceTime;
            return new
            {
                AnnounceTask = tracker.Key.Announce(request),
                Statistics = tracker.Value,
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
        var trackerUri = _trackers.First().First();
        var tracker = _trackerClientFactory.CreateTrackerClient(trackerUri);

        if (tracker == null)
        {
            return;
        }

        _activeTrackers.Add(tracker, new TrackerStatistics
        {
            Uri = trackerUri,
            Type = tracker.Type,
        });
    }

    #nullable disable
    private record TrackerStatistics : ITrackerDetails
    {
        public Uri Uri { get; set; }

        public int Peers { get; set; }

        public DateTime? LastAnnounce { get; set; }

        public string Type { get; set; }
    }
    #nullable enable
}
