// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using TorrentCore.Engine;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent
{
    class QueueingMessageHandler : IPeerMessageHandler
    {
        private readonly IMainLoop _mainLoop;
        private readonly IPeerMessageHandler _underlying;

        public QueueingMessageHandler(IMainLoop mainLoop, IPeerMessageHandler underlying)
        {
            _mainLoop = mainLoop ?? throw new ArgumentNullException(nameof(mainLoop));
            _underlying = underlying ?? throw new ArgumentNullException(nameof(underlying));
        }

        public void MessageReceived(BitTorrentPeer peer, byte[] data)
        {
            _mainLoop.AddTask(() => _underlying.MessageReceived(peer, data));
        }

        public void PeerDisconnected(BitTorrentPeer peer)
        {
            _mainLoop.AddTask(() => _underlying.PeerDisconnected(peer));
        }
    }
}
