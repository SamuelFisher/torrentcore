// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Data;

namespace TorrentCore.Tracker
{
    /// <summary>
    /// Represents an announce request made to a tracker.
    /// </summary>
    public class AnnounceRequest
    {
        public AnnounceRequest(PeerId peerId,
                               long remaining,
                               Sha1Hash infoHash)
        {
            PeerId = peerId;
            Remaining = remaining;
            InfoHash = infoHash;
        }

        /// <summary>
        /// Gets the ID of the peer making the announce request.
        /// </summary>
        public PeerId PeerId { get; }

        /// <summary>
        /// Gets the number of bytes left to download.
        /// </summary>
        public long Remaining { get; }

        /// <summary>
        /// Gets the infohash of the torrent being downloaded.
        /// </summary>
        public Sha1Hash InfoHash { get; }
    }
}
