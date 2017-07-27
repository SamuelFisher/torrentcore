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

namespace TorrentCore.Data
{
    /// <summary>
    /// Represents a region of a piece with a specified offset and length.
    /// </summary>
    internal class Block
    {
        /// <summary>
        /// Creates a new block with the specified piece index, offset and data.
        /// </summary>
        /// <param name="pieceIndex">Index of the piece the block belongs to.</param>
        /// <param name="offset">Offset into the piece at which the data starts.</param>
        /// <param name="data">Data for the block.</param>
        public Block(int pieceIndex, int offset, byte[] data)
        {
            PieceIndex = pieceIndex;
            Offset = offset;
            Data = data;
        }

        /// <summary>
        /// Gets a value indicating the index of the piece to which the block belongs.
        /// </summary>
        public int PieceIndex { get; }

        /// <summary>
        /// Gets a value indicating the offset into the piece the data contained within the block represents.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Gets the data contained within the block.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Gets a value indicating the number of bytes contained within the block.
        /// </summary>
        public int Length => Data.Length;
    }
}
