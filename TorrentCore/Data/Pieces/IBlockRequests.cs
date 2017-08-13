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
using System.Text;
using TorrentCore.Application.BitTorrent;

namespace TorrentCore.Data.Pieces
{
    public interface IBlockRequests
    {
        /// <summary>
        /// Gets the blocks that have been requested from peers.
        /// </summary>
        IReadOnlyCollection<BlockRequest> RequestedBlocks { get; }

        /// <summary>
        /// Gets the blocks that have been received but are part of incomplete pieces.
        /// </summary>
        IReadOnlyCollection<BlockRequest> DownloadedBlocks { get; }

        void BlockRequested(BlockRequest block);

        void BlockReceived(Block block);

        void ClearBlocksForPiece(Piece piece);
    }
}
