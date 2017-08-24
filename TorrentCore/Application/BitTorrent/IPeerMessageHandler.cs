﻿// This file is part of TorrentCore.
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
using TorrentCore.Application.BitTorrent;

namespace TorrentCore.Application.BitTorrent
{
    public interface IPeerMessageHandler
    {
        /// <summary>
        /// Invoked when a message is received from a connected stream.
        /// </summary>
        /// <param name="peer">Peer the message was received from.</param>
        /// <param name="data">Received message data.</param>
        void MessageReceived(PeerConnection peer, byte[] data);

        /// <summary>
        /// Invoked when a peer disconnects.
        /// </summary>
        /// <param name="peer">The peer that disconnected.</param>
        void PeerDisconnected(PeerConnection peer);
    }
}
