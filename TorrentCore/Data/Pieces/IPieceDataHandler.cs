// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2017 Samuel Fisher.
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
using System.Text;

namespace TorrentCore.Data
{
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
        void MarkPieceAsCompleted(Piece piece);
    }
}
