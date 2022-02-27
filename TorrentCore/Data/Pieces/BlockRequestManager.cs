// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Application.BitTorrent;

namespace TorrentCore.Data.Pieces;

/// <summary>
/// Keeps track of the blocks that have been requested.
/// </summary>
class BlockRequestManager : IBlockRequests
{
    private readonly HashSet<BlockRequest> _requestedBlocks = new HashSet<BlockRequest>();
    private readonly HashSet<BlockRequest> _pendingBlocks = new HashSet<BlockRequest>();

    public IReadOnlyCollection<BlockRequest> RequestedBlocks => _requestedBlocks;

    public IReadOnlyCollection<BlockRequest> DownloadedBlocks => _pendingBlocks;

    public void BlockRequested(BlockRequest block)
    {
        _requestedBlocks.Add(block);
    }

    public void BlockReceived(Block block)
    {
        _pendingBlocks.Add(block.AsRequest());
        _requestedBlocks.Remove(block.AsRequest());
    }

    public void ClearBlocksForPiece(Piece piece)
    {
        _pendingBlocks.RemoveWhere(block => block.PieceIndex == piece.Index);
    }
}
