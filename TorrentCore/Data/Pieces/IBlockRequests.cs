// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Application.BitTorrent;

namespace TorrentCore.Data.Pieces;

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
