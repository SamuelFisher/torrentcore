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
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Data;

namespace TorrentCore.Transport
{
    /// <summary>
    /// Represents a communication channel between two peers.
    /// </summary>
    interface ITransportStream : IDisposable
    {
        /// <summary>
        /// Gets the ID for this peer.
        /// </summary>
        PeerId PeerId { get; }

        /// <summary>
        /// Gets a value indicating whether this connection is active.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets an address that uniquely identifies the peer this transport stream.
        /// </summary>
        string Address { get; }

        /// <summary>
        /// Gets the info hash the stream can be used for.
        /// </summary>
        Sha1Hash InfoHash { get; }

        /// <summary>
        /// Attempts to initiate this connection.
        /// </summary>
        /// <returns>Task which completes when the connection is made.</returns>
        Task Connect();

        /// <summary>
        /// Sends the specified block of data.
        /// </summary>
        /// <param name="data">Data to send.</param>
        void SendData(byte[] data);
    }
}
