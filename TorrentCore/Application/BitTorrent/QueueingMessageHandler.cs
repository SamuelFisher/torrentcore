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
        private readonly IMainLoop mainLoop;
        private readonly IPeerMessageHandler underlying;

        public QueueingMessageHandler(IMainLoop mainLoop, IPeerMessageHandler underlying)
        {
            this.mainLoop = mainLoop;
            this.underlying = underlying;
        }

        public void MessageReceived(PeerConnection peer, byte[] data)
        {
            mainLoop.AddTask(() => underlying.MessageReceived(peer, data));
        }

        public void PeerDisconnected(PeerConnection peer)
        {
            mainLoop.AddTask(() => underlying.PeerDisconnected(peer));
        }
    }
}
