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
        /// <returns></returns>
        IEnumerable<BlockRequest> BlocksToRequest(IReadOnlyList<Piece> incompletePieces,
                                                  Bitfield availability,
                                                  IReadOnlyCollection<PeerConnection> peers,
                                                  IBlockRequests blockRequests);
    }
}
