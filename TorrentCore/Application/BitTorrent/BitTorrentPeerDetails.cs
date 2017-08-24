// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent
{
    public sealed class BitTorrentPeerDetails
    {
        public BitTorrentPeerDetails(string address, PeerId peerId)
        {
            Address = address;
            PeerId = peerId;
        }

        public string Address { get; }

        public PeerId PeerId { get; }
    }
}
