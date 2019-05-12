// This file is part of TorrentCore.
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
                                                         IReadOnlyCollection<IPeer> peers,
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
