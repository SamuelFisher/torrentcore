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

namespace TorrentCore.Application.BitTorrent
{
    /// <summary>
    /// Represents a region of a piece with a specified offset and length, without data.
    /// </summary>
    public class BlockRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockRequest"/> class, using the specified piece index and offset.
        /// </summary>
        /// <param name="pieceIndex">Index of the piece the block belongs to.</param>
        /// <param name="offset">Offset into the piece at which the data starts.</param>
        /// <param name="length">Length of the block.</param>
        internal BlockRequest(int pieceIndex, int offset, int length)
        {
            PieceIndex = pieceIndex;
            Offset = offset;
            Length = length;
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
        /// Gets a value indicating the length of the data contained within the block.
        /// </summary>
        public int Length { get; }

        public static bool operator ==(BlockRequest x, BlockRequest y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }
            else if (((object)x == null) || ((object)y == null))
            {
                return false;
            }
            else
            {
                return x.PieceIndex == y.PieceIndex
                       && x.Offset == y.Offset
                       && x.Length == y.Length;
            }
        }

        public static bool operator !=(BlockRequest x, BlockRequest y)
        {
            return !(x == y);
        }

        public override bool Equals(object obj)
        {
            if (obj is BlockRequest)
            {
                BlockRequest other = (BlockRequest)obj;
                return this == other;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + PieceIndex.GetHashCode();
            hash = (hash * 7) + Offset.GetHashCode();
            hash = (hash * 7) + Length.GetHashCode();
            return hash;
        }
    }
}
