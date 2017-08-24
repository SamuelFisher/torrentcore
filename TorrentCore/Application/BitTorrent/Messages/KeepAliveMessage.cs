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

namespace TorrentCore.Application.BitTorrent.Messages
{
    /// <summary>
    /// Message used to maintain connection with a peer.
    /// </summary>
    class KeepAliveMessage : CommonPeerMessage
    {
        /// <summary>
        /// Gets the ID of the message.
        /// </summary>
        /// <remarks>The keep-alive message has no ID.</remarks>
        public override byte ID { get; } = 0;

        /// <summary>
        /// Sends the message by writing it to the specified BinaryWriter.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public override void Send(BinaryWriter writer)
        {
            // Do nothing
        }
    }
}
