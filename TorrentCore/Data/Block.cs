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

namespace TorrentCore.Data
{
    /// <summary>
    /// Represents a region of a piece with a specified offset and length.
    /// </summary>
    public class Block
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Block"/> class,
        /// with the specified piece index, offset and data.
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
