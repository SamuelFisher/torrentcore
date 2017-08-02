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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Data;
using TorrentCore.Tracker;

namespace TorrentCore.Transport
{
    interface ITransportProtocol
    {
        /// <summary>
        /// Gets a collection of the active transport streams.
        /// </summary>
        IEnumerable<ITransportStream> Streams { get; }

        /// <summary>
        /// Handles new incoming connection requests.
        /// </summary>
        /// <param name="e">Event args for handling the request.</param>
        void AcceptConnection(TransportConnectionEventArgs e);

        /// <summary>
        /// Starts the transport protocol.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the transport protocol.
        /// </summary>
        void Stop();
    }
}
