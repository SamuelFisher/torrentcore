// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Tracker;

namespace CustomTransportProtocol;

class SingletonTracker : ITrackerClientFactory
{
    private readonly ITracker _instance;

    public SingletonTracker(ITracker instance)
    {
        _instance = instance;
    }

    public ITracker CreateTrackerClient(Uri trackerUri)
    {
        return _instance;
    }
}
