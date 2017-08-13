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
    /// <summary>
    /// Keeps track of the blocks that have been requested.
    /// </summary>
    class BlockRequestManager : IBlockRequests
    {
        private readonly HashSet<BlockRequest> requestedBlocks = new HashSet<BlockRequest>();
        private readonly HashSet<BlockRequest> pendingBlocks = new HashSet<BlockRequest>();

        public IReadOnlyCollection<BlockRequest> RequestedBlocks => requestedBlocks;

        public IReadOnlyCollection<BlockRequest> DownloadedBlocks => pendingBlocks;

        public void BlockRequested(BlockRequest block)
        {
            requestedBlocks.Add(block);
        }

        public void BlockReceived(Block block)
        {
            pendingBlocks.Add(block.AsRequest());
            requestedBlocks.Remove(block.AsRequest());
        }

        public void ClearBlocksForPiece(Piece piece)
        {
            pendingBlocks.RemoveWhere(block => block.PieceIndex == piece.Index);
        }
    }
}
