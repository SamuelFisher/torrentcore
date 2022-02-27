// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Data;

public interface IPieceDataHandler : IBlockDataHandler
{
    /// <summary>
    /// Called when a piece successfully completes downloading.
    /// </summary>
    event Action<Piece> PieceCorrupted;

    /// <summary>
    /// Called when a piece finishes downloading but contains corrupted data.
    /// </summary>
    event Action<Piece> PieceCompleted;

    /// <summary>
    /// Gets the set of completed pieces.
    /// </summary>
    IReadOnlyCollection<Piece> CompletedPieces { get; }

    /// <summary>
    /// Marks the specified piece as completed. This should only be used as an override mechanism.
    /// </summary>
    /// <param name="piece">The piece to mark as completed.</param>
    void MarkPieceAsCompleted(Piece piece);
}
