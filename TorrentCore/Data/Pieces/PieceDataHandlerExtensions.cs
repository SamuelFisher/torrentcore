// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Data.Pieces;

public static class PieceDataHandlerExtensions
{
    public static IEnumerable<Piece> IncompletePieces(this IPieceDataHandler pieceDataHandler)
    {
        return pieceDataHandler.Metainfo.Pieces.Except(pieceDataHandler.CompletedPieces);
    }
}
