// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Collections.Generic;
using TorrentCore.Data;
using TorrentCore.Data.Pieces;

namespace TorrentCore.Application.BitTorrent
{
    /// <summary>
    /// Provides an algorithm to determine which blocks should be requested from peers.
    /// </summary>
    public interface IPiecePicker
    {
        /// <summary>
        /// Determines which blocks should be requested next.
        /// </summary>
        /// <param name="incompletePieces">The pieces which have not yet been downloaded.</param>
        /// <param name="availability">Indicates which pieces are available.</param>
        /// <param name="peers">The peers that are currently connected.</param>
        /// <param name="blockRequests">Provides details of which blocks have been requested already.</param>
        IEnumerable<BlockRequest> BlocksToRequest(IReadOnlyList<Piece> incompletePieces,
                                                  Bitfield availability,
                                                  IReadOnlyCollection<PeerConnection> peers,
                                                  IBlockRequests blockRequests);
    }
}
