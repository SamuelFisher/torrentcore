// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Moq;
using NUnit.Framework;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Test.Transport
{
    [TestFixture]
    public class ListenPortTest
    {
        [Test]
        public void FindNextAvailablePort()
        {
            var client1 = CreateTransportProtocol();
            client1.Start();
            Assert.That(client1.LocalConection.Port, Is.EqualTo(6881));

            var client2 = CreateTransportProtocol();
            client2.Start();
            Assert.That(client2.LocalConection.Port, Is.EqualTo(6882));
        }

        private static TcpTransportProtocol CreateTransportProtocol()
        {
            return new TcpTransportProtocol(6881,
                                            true,
                                            IPAddress.Loopback);
        }
    }
}
