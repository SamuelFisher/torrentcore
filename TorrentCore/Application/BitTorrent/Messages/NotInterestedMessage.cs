// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentCore.Application.BitTorrent.Messages
{
    /// <summary>
    /// A message used to indicate that a peer is not interested in pieces another peer has to offer.
    /// </summary>
    class NotInterestedMessage : CommonPeerMessage
    {
        public const byte MessageID = 3;

        /// <summary>
        /// Gets the ID of the message.
        /// </summary>
        public override byte ID
        {
            get { return MessageID; }
        }
    }
}
