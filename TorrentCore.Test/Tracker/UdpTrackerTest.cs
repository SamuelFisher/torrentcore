// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using BencodeNET.Objects;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Data;
using TorrentCore.Tracker;
using TorrentCore.Tracker.Http;
using TorrentCore.Tracker.Udp;
using TorrentCore.Transport;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Test.Tracker
{
    [TestFixture]
    public class UdpTrackerTest
    {
        private readonly AnnounceRequest _request = new AnnounceRequest(PeerId.CreateNew(),
                                                                       0,
                                                                       0,
                                                                       1000,
                                                                       Sha1Hash.Empty);

        [Test]
        public void Announce()
        {
            var fakeTracker = new FakeUdpTracker(5001);
            fakeTracker.Start();

            var trackerClient = new UdpTracker(new LocalTcpConnectionOptions
            {
                Port = 5000,
                BindAddress = IPAddress.Loopback,
                PublicAddress = IPAddress.Loopback,
            },
            new Uri("udp://localhost:5001"));

            var response = trackerClient.Announce(_request).Result;
            var peers = response.Peers.Cast<TcpTransportStream>().ToArray();

            Assert.That(peers, Has.Length.EqualTo(2));

            var peer1 = peers.Single(x => x.RemoteEndPoint.Port == 5001);
            Assert.That(peer1.RemoteEndPoint.Address, Is.EqualTo(IPAddress.Parse("192.168.0.1")));

            var peer2 = peers.Single(x => x.RemoteEndPoint.Port == 5002);
            Assert.That(peer2.RemoteEndPoint.Address, Is.EqualTo(IPAddress.Parse("192.168.0.2")));

            fakeTracker.Stop();
        }

        private class FakeUdpTracker
        {
            private UdpClient _client;
            IPEndPoint _sender;

            public FakeUdpTracker(int port)
            {
                _client = new UdpClient(port);
                _sender = new IPEndPoint(IPAddress.Any, 0);
            }

            public void Start()
            {
                Task.Run(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            var message = _client.Receive(ref _sender);
                            await HandleMessage(message);
                        }
                    }
                    catch
                    {
                        // Socket closed
                    }
                });
            }

            private async Task HandleMessage(byte[] message)
            {
                using (var ms = new MemoryStream(message))
                {
                    var reader = new BigEndianBinaryReader(ms);
                    var connectionId = reader.ReadInt64();
                    var action = reader.ReadInt32();
                    var transactionId = reader.ReadInt32();

                    if (action == (int)MessageAction.Connect)
                    {
                        await SendConnectResponse(transactionId, connectionId);
                    }
                    else
                    {
                        await SendAnnounceResponse(transactionId);
                    }
                }
            }

            public void Stop()
            {
                _client.Dispose();
            }

            private async Task SendConnectResponse(int transactionId, long connectionId)
            {
                using (var ms = new MemoryStream())
                {
                    int action = (int)MessageAction.Connect;

                    var writer = new BigEndianBinaryWriter(ms);
                    writer.Write(action);
                    writer.Write(transactionId);
                    writer.Write(connectionId);

                    var data = ms.ToArray();

                    await _client.SendAsync(data, data.Length, _sender);
                }
            }

            private async Task SendAnnounceResponse(int transactionId)
            {
                using (var ms = new MemoryStream())
                {
                    int action = (int)MessageAction.Announce;
                    int interval = 1;
                    int leechers = 0;
                    int seeders = 0;
                    List<UdpPeer> peers = new List<UdpPeer>
                    {
                        new UdpPeer(new byte[] { 192, 168, 0, 1 }, 5001),
                        new UdpPeer(new byte[] { 192, 168, 0, 2 }, 5002),
                    };

                    var writer = new BigEndianBinaryWriter(ms);
                    writer.Write(action);
                    writer.Write(transactionId);
                    writer.Write(interval);
                    writer.Write(leechers);
                    writer.Write(seeders);
                    foreach (var peer in peers)
                    {
                        writer.Write(peer.IpAddress);
                        writer.Write(peer.Port);
                    }

                    var data = ms.ToArray();
                    await _client.SendAsync(data, data.Length, _sender);
                }
            }

            private class UdpPeer
            {
                public UdpPeer(byte[] ipAddress, ushort port)
                {
                    IpAddress = ipAddress;
                    Port = port;
                }

                public byte[] IpAddress { get; }

                public ushort Port { get; }
            }
        }
    }
}
