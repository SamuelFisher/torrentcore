// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TorrentCore.Transport
{
    /// <summary>
    /// Represents a connection request at the transport protocol level.
    /// </summary>
    class TransportConnectionEventArgs
    {
        public TransportConnectionEventArgs(TcpClient client)
        {
            Client = client;
        }

        /// <summary>
        /// Gets the TcpClient associated with this connection request.
        /// </summary>
        public TcpClient Client { get; }
    }
}
