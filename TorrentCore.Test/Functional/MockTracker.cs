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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Tracker;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Test.Functional
{
    class MockTracker : ITrackerClientFactory
    {
        private readonly List<IPEndPoint> peers = new List<IPEndPoint>();

        public ITracker CreateTrackerClient(Uri trackerUri)
        {
            return new TrackerClient(peers);
        }

        public void RegisterPeer(int port)
        {
            peers.Add(new IPEndPoint(IPAddress.Loopback, port));
        }

        private class TrackerClient : ITracker
        {
            private readonly IList<IPEndPoint> peers;

            public TrackerClient(IList<IPEndPoint> peers)
            {
                this.peers = peers;
            }

            public string Type { get; }

            public Task<AnnounceResult> Announce(AnnounceRequest request)
            {
                var result = new AnnounceResult(peers.Select(x => new TcpTransportStream(IPAddress.Loopback, x.Address, x.Port)));
                return Task.FromResult(result);
            }
        }
    }
}
