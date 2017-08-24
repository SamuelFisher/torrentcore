// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

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
