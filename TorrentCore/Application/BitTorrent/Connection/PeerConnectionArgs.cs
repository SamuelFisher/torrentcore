// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using TorrentCore.Data;
using TorrentCore.Engine;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent.Connection
{
    class PeerConnectionArgs
    {
        public PeerConnectionArgs(
            PeerId localPeerId,
            Metainfo metainfo,
            IPeerMessageHandler messageHandler)
        {
            LocalPeerId = localPeerId;
            Metainfo = metainfo;
            MessageHandler = messageHandler;
        }

        public PeerId LocalPeerId { get; }

        public Metainfo Metainfo { get; }

        public IPeerMessageHandler MessageHandler { get; }
    }
}
