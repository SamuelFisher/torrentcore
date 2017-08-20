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
using TorrentCore.Data.Pieces;

namespace TorrentCore.Application.BitTorrent
{
    /// <summary>
    /// Provides methods to determine which blocks to request.
    /// </summary>
    class PiecePicker : IPiecePicker
    {
        public const int MaxOutstandingRequests = 100;
        private const int BlockRequestSize = 16384; // 16 kB

        public IEnumerable<BlockRequest> BlocksToRequest(IReadOnlyList<Piece> incompletePieces,
                                                         Bitfield availability,
                                                         IReadOnlyCollection<PeerConnection> peers,
                                                         IBlockRequests blockRequests)
        {
            var toRequest = new List<BlockRequest>();
            int maxToRequest = MaxOutstandingRequests - blockRequests.RequestedBlocks.Count;

            foreach (var piece in incompletePieces)
            {
                if (toRequest.Count >= maxToRequest)
                    break;

                if (!availability.IsPieceAvailable(piece.Index))
                    continue;

                var block = NextBlockForPiece(piece, blockRequests);
                if (block != null)
                {
                    toRequest.Add(block);
                }
            }

            return toRequest;
        }

        private BlockRequest NextBlockForPiece(Piece piece, IBlockRequests blockRequests)
        {
            for (int blockOffset = 0; blockOffset < piece.Size; blockOffset += BlockRequestSize)
            {
                int blockSize = blockOffset < piece.Size - BlockRequestSize ? BlockRequestSize : piece.Size - blockOffset;
                var blockToRequest = new BlockRequest(piece.Index, blockOffset, blockSize);
                if (!blockRequests.RequestedBlocks.Contains(blockToRequest) &&
                    !blockRequests.DownloadedBlocks.Contains(blockToRequest))
                {
                    return blockToRequest;
                }
            }

            // No blocks to request for specified piece
            return null;
        }
    }
}
