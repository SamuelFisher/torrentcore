// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Net;
using BencodeNET.Objects;
using HttpMock.Net;
using Microsoft.Extensions.Logging.Abstractions;
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
        private readonly AnnounceRequest _request = new AnnounceRequest(PeerId.CreateNew(),
                                                                       1000,
                                                                       0,
                                                                       0,
                                                                       Sha1Hash.Empty);

        [TestCase(false, Description = "Announce with normal response")]
        [TestCase(true, Description = "Announce with compact response")]
        public void Announce(bool compact)
        {
            var fakeHttpTracker = new FakeHttpTracker(compact, 5001);
            fakeHttpTracker.Start();

            var trackerClient = new HttpTracker(
                new NullLogger<HttpTracker>(),
                new LocalTcpConnectionOptions
                {
                    Port = 5000,
                    BindAddress = IPAddress.Loopback,
                    PublicAddress = IPAddress.Loopback,
                },
                new Uri("http://localhost:5001/announce"));

            var response = trackerClient.Announce(_request).Result;
            var peers = response.Peers.Cast<TcpTransportStream>().ToArray();

            Assert.That(peers, Has.Length.EqualTo(2));

            var peer1 = peers.Single(x => x.RemoteEndPoint.Port == 5001);
            Assert.That(peer1.RemoteEndPoint.Address, Is.EqualTo(IPAddress.Parse("192.168.0.1")));

            var peer2 = peers.Single(x => x.RemoteEndPoint.Port == 5002);
            Assert.That(peer2.RemoteEndPoint.Address, Is.EqualTo(IPAddress.Parse("192.168.0.2")));

            fakeHttpTracker.Stop();
        }

        private class FakeHttpTracker
        {
            private readonly bool _compact;
            private readonly int _port;

            private HttpHandlerBuilder _server;

            public FakeHttpTracker(bool compact, int port)
            {
                _compact = compact;
                _port = port;
            }

            public void Start()
            {
                _server = Server.Start(_port);

                _server
                    .When(c => c.Request.Method.Equals("GET") && c.Request.Path.Value.StartsWith("/announce"))
                    .Do(c =>
                    {
                        var response = BuildResponse();
                        c.Response.Body.Write(response, 0, response.Length);
                    });
            }

            public void Stop()
            {
                _server.Clear();
                _server.Dispose();
            }

            private byte[] BuildResponse()
            {
                BDictionary response;

                if (_compact)
                {
                    var ms = new MemoryStream();
                    var writer = new BigEndianBinaryWriter(ms);

                    // Peer 1
                    writer.Write(new byte[] { 192, 168, 0, 1 });
                    writer.Write((ushort)5001);

                    // Peer 2
                    writer.Write(new byte[] { 192, 168, 0, 2 });
                    writer.Write((ushort)5002);

                    writer.Flush();

                    response = new BDictionary
                    {
                        ["peers"] = new BString(ms.ToArray()),
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
                                ["port"] = new BNumber(5001),
                            },
                            new BDictionary
                            {
                                ["ip"] = new BString("192.168.0.2"),
                                ["port"] = new BNumber(5002),
                            },
                        },
                    };
                }

                return response.EncodeAsBytes();
            }
        }
    }
}
