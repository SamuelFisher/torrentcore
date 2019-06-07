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
                                                                       1000,
                                                                       Sha1Hash.Empty);

        [Test]
        public void Announce()
        {
            var tracker = new MockUdpTracker(
               new LocalTcpConnectionOptions
               {
                   Port = 5000,
                   BindAddress = IPAddress.Loopback,
                   PublicAddress = IPAddress.Loopback,
               },
               new Uri("udp://example.com/announce"));

            var response = tracker.Announce(_request).Result;
            var peers = response.Peers.Cast<TcpTransportStream>().ToArray();

            Assert.That(peers, Has.Length.EqualTo(2));

            var peer1 = peers.Single(x => x.RemoteEndPoint.Port == 5001);
            Assert.That(peer1.RemoteEndPoint.Address, Is.EqualTo(IPAddress.Parse("192.168.0.1")));

            var peer2 = peers.Single(x => x.RemoteEndPoint.Port == 5002);
            Assert.That(peer2.RemoteEndPoint.Address, Is.EqualTo(IPAddress.Parse("192.168.0.2")));
        }

        private class MockUdpTracker : UdpTracker
        {
            private bool _connected;

            public MockUdpTracker(LocalTcpConnectionOptions tcpConnectionDetails, Uri baseUrl)
                : base(tcpConnectionDetails, baseUrl)
            {
            }

            protected override Task<MemoryStream> Receive()
            {
                var ms = new MemoryStream();
                var writer = new BigEndianBinaryWriter(ms);
                if (_connected)
                {
                    // Announce response payload
                    int action = (int)MessageAction.Announce;
                    int transactionId = default;
                    int interval = 1;
                    int leechers = 0;
                    int seeders = 0;
                    List<UdpPeer> peers = new List<UdpPeer>
                    {
                        new UdpPeer(new byte[] { 192, 168, 0, 1 }, 5001),
                        new UdpPeer(new byte[] { 192, 168, 0, 2 }, 5002),
                    };

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
                }
                else
                {
                    // Connection response payload
                    int action = (int)MessageAction.Connect;
                    int transactionId = default;
                    long connectionId = 1;

                    writer.Write(action);
                    writer.Write(transactionId);
                    writer.Write(connectionId);
                    _connected = true;
                }

                writer.Flush();
                ms.Position = 0;

                return Task.FromResult(ms);
            }

            protected override Task Send(UdpTrackerRequestMessage message)
            {
                return Task.CompletedTask;
            }

            protected override int GenerateTransactionId()
            {
                return default;
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
