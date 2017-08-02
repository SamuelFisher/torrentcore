// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2017 Samuel Fisher.
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
                                            IPAddress.Loopback,
                                            _ => { });
        }
    }
}
