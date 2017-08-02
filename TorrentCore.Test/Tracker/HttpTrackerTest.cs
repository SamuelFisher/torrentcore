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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Objects;
using NUnit.Framework;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Data;
using TorrentCore.Tracker;
using TorrentCore.Tracker.Http;
using TorrentCore.Transport;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Test.Tracker
{
    [TestFixture]
    public class HttpTrackerTest
    {
        private readonly AnnounceRequest request = new AnnounceRequest(PeerId.CreateNew(),
                                                                       1000,
                                                                       Sha1Hash.Empty);

        [TestCase(false, Description = "Announce with normal response")]
        [TestCase(true, Description = "Announce with compact response")]
        public void Announce(bool compact)
        {
            var tracker = new MockHttpTracker(new LocalTcpConnectionDetails(5000, IPAddress.Loopback, IPAddress.Loopback), compact, new Uri("http://example.com/announce"));
            
            var response = tracker.Announce(request).Result;
            var peers = response.Peers.Cast<TcpTransportStream>().ToArray();

            Assert.That(peers, Has.Length.EqualTo(2));

            var peer1 = peers.Single(x => x.RemoteEndPoint.Port == 5001);
            Assert.That(peer1.RemoteEndPoint.Address, Is.EqualTo(IPAddress.Parse("192.168.0.1")));

            var peer2 = peers.Single(x => x.RemoteEndPoint.Port == 5002);
            Assert.That(peer2.RemoteEndPoint.Address, Is.EqualTo(IPAddress.Parse("192.168.0.2")));
        }

        private class MockHttpTracker : HttpTracker
        {
            private readonly bool compact;

            public MockHttpTracker(LocalTcpConnectionDetails tcpConnectionDetails, bool compact, Uri baseUrl)
                : base(tcpConnectionDetails, baseUrl)
            {
                this.compact = compact;
            }

            // Mock out the web request
            protected override Task<Stream> HttpGet(string requestUri)
            {
                BDictionary response;

                if (compact)
                {
                    var ms = new MemoryStream();
                    var writer = new BigEndianBinaryWriter(ms);

                    // Peer 1
                    writer.Write(new byte[] { 192, 168, 0, 1});
                    writer.Write((ushort)5001);

                    // Peer 2
                    writer.Write(new byte[] { 192, 168, 0, 2 });
                    writer.Write((ushort)5002);

                    writer.Flush();

                    response = new BDictionary
                    {
                        ["peers"] = new BString(ms.ToArray())
                    };
                }
                else
                {
                    response = new BDictionary
                    {
                        ["peers"] = new BList
                        {
                            new BDictionary
                            {
                                ["ip"] = new BString("192.168.0.1"),
                                ["port"] = new BNumber(5001)
                            },
                            new BDictionary
                            {
                                ["ip"] = new BString("192.168.0.2"),
                                ["port"] = new BNumber(5002)
                            }
                        }
                    };
                }

                var resultStream = new MemoryStream(response.EncodeAsBytes());
                return Task.FromResult((Stream)resultStream);
            }
        }
    }
}
