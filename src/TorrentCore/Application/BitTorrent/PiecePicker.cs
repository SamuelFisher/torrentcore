// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2016 Sam Fisher.
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
using TorrentCore.Data;

namespace TorrentCore.Application.BitTorrent
{
    /// <summary>
    /// Provides methods to determine which blocks to request.
    /// </summary>
    class PiecePicker : IPiecePicker
    {
        public const int MaxOutstandingRequests = 100;
        private const int BlockRequestSize = 16384; // 16 kB

        private readonly HashSet<BlockRequest> requestedBlocks = new HashSet<BlockRequest>();
        private readonly HashSet<BlockRequest> downloadedBlocks = new HashSet<BlockRequest>();

        public IEnumerable<BlockRequest> BlocksToRequest(IEnumerable<Piece> incompletePieces, Bitfield availability)
        {
            var toRequest = new List<BlockRequest>();
            int maxToRequest = MaxOutstandingRequests - requestedBlocks.Count;

            foreach (var piece in incompletePieces)
            {
                if (toRequest.Count >= maxToRequest)
                    break;

                if (!availability.IsPieceAvailable(piece.Index))
                    continue;

                var block = NextBlockForPiece(piece);
                if (block != null)
                {
                    toRequest.Add(block);
                }
            }

            return toRequest;
        }

        public void BlockRequested(BlockRequest block)
        {
            requestedBlocks.Add(block);
        }

        public void BlockReceived(Block block)
        {
            downloadedBlocks.Add(block.AsRequest());
            requestedBlocks.Remove(block.AsRequest());
        }

        public void PieceCompleted(Piece piece)
        {
            downloadedBlocks.RemoveWhere(block => block.PieceIndex == piece.Index);
        }

        private BlockRequest NextBlockForPiece(Piece piece)
        {
            for (int blockOffset = 0; blockOffset < piece.Size; blockOffset += BlockRequestSize)
            {
                int blockSize = (blockOffset < piece.Size - BlockRequestSize ? BlockRequestSize : piece.Size - blockOffset);
                var blockToRequest = new BlockRequest(piece.Index, blockOffset, blockSize);
                if (!requestedBlocks.Contains(blockToRequest) && !downloadedBlocks.Contains(blockToRequest))
                {
                    return blockToRequest;
                }
            }

            // No blocks to request for specified piece
            return null;
        }
    }
}
