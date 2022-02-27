// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Net;
using BencodeNET.Objects;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Data;
using TorrentCore.Tracker;
using TorrentCore.Tracker.Http;
using TorrentCore.Transport;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Test.Tracker;

[TestFixture]
public class HttpTrackerTest
{
    private readonly AnnounceRequest _request = new AnnounceRequest(
        PeerId.CreateNew(),
        1000,
        0,
        0,
        Sha1Hash.Empty);

    [TestCase(false, Description = "Announce with normal response")]
    [TestCase(true, Description = "Announce with compact response")]
    public void Announce(bool compact)
    {
        var fakeHttpTracker = new FakeHttpTracker(compact);

        var trackerClient = new HttpTracker(
            new NullLogger<HttpTracker>(),
            new LocalTcpConnectionOptions
            {
                Port = 5000,
                BindAddress = IPAddress.Loopback,
                PublicAddress = IPAddress.Loopback,
            },
            new Uri("http://localhost:5001/announce"),
            fakeHttpTracker.CreateHttpClient());

        var response = trackerClient.Announce(_request).Result;
        var peers = response.Peers.Cast<TcpTransportStream>().ToArray();

        Assert.That(peers, Has.Length.EqualTo(2));

        var peer1 = peers.Single(x => x.RemoteEndPoint.Port == 5001);
        Assert.That(peer1.RemoteEndPoint.Address, Is.EqualTo(IPAddress.Parse("192.168.0.1")));

        var peer2 = peers.Single(x => x.RemoteEndPoint.Port == 5002);
        Assert.That(peer2.RemoteEndPoint.Address, Is.EqualTo(IPAddress.Parse("192.168.0.2")));
    }

    private class FakeHttpTracker
    {
        private readonly bool _compact;
        private readonly MockHttpMessageHandler _mockHttp;

        public FakeHttpTracker(bool compact)
        {
            _compact = compact;
            _mockHttp = new MockHttpMessageHandler();
            _mockHttp.When(HttpMethod.Get, "/announce")
                .Respond(HttpStatusCode.OK, new ByteArrayContent(BuildResponse()));
        }

        public HttpClient CreateHttpClient() => _mockHttp.ToHttpClient();

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
