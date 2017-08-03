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

namespace TorrentCore.Transport.Tcp
{
    public class LocalTcpConnectionDetails
    {
        public LocalTcpConnectionDetails(int port, IPAddress publicAddress, IPAddress bindAddress)
        {
            Port = port;
            PublicAddress = publicAddress;
            BindAddress = bindAddress;
        }

        /// <summary>
        /// Gets the port on which incoming connections can be made.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Gets the public address that is used to listen for incoming connections.
        /// </summary>
        public IPAddress PublicAddress { get; }

        /// <summary>
        /// Gets the address of the local adapter used for connections.
        /// </summary>
        public IPAddress BindAddress { get; }
    }
}
