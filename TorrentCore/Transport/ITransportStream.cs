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
using System.Text;
using System.Threading.Tasks;

namespace TorrentCore.Transport
{
    /// <summary>
    /// Represents a communication channel between two peers.
    /// </summary>
    public interface ITransportStream
    {
        /// <summary>
        /// Gets a value indicating whether this connection is active.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets an address that uniquely identifies the peer this transport stream.
        /// </summary>
        string DisplayAddress { get; }

        /// <summary>
        /// Gets an address that uniquely identifies the peer this transport stream.
        /// <remarks>This may be compared to other addresses to determine whether streams refer to the same peer.</remarks>
        /// </summary>
        object Address { get; }

        /// <summary>
        /// Gets the stream used for communication.
        /// </summary>
        Stream Stream { get; }

        /// <summary>
        /// Attempts to initiate this connection.
        /// </summary>
        /// <returns>Task which completes when the connection is made.</returns>
        Task Connect();

        /// <summary>
        /// Closes the connection.
        /// </summary>
        void Disconnect();
    }
}
