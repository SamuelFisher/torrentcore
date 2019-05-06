// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Tracker;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Test.Functional
{
    class MockTracker : ITrackerClientFactory
    {
        private readonly List<IPEndPoint> _peers = new List<IPEndPoint>();

        public ITracker CreateTrackerClient(Uri trackerUri)
        {
            return new TrackerClient(_peers);
        }

        public void RegisterPeer(int port)
        {
            _peers.Add(new IPEndPoint(IPAddress.Loopback, port));
        }

        private class TrackerClient : ITracker
        {
            private readonly IList<IPEndPoint> _peers;

            public TrackerClient(IList<IPEndPoint> peers)
            {
                this._peers = peers;
            }

            public string Type { get; }

            public Task<AnnounceResult> Announce(AnnounceRequest request)
            {
                var result = new AnnounceResult(_peers.Select(x => new TcpTransportStream(IPAddress.Loopback, x.Address, x.Port)));
                return Task.FromResult(result);
            }
        }
    }
}
