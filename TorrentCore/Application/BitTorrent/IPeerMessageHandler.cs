// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2017 Samuel Fisher.
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
