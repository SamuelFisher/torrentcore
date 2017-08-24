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
    /// Represents a piece of data in a collection of files.
    /// </summary>
    public class Piece
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Piece"/> class with the specified index.
        /// </summary>
        /// <param name="index">Zero-based index of the piece.</param>
        public Piece(int index)
        {
            Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Piece"/> class
        /// with the specified index, size, files and hash.
        /// </summary>
        /// <param name="index">Zero-based index of the piece.</param>
        /// <param name="size">Size of the piece, in bytes.</param>
        /// <param name="hash">Hash for the piece.</param>
        public Piece(int index, int size, Sha1Hash hash)
        {
            Index = index;
            Size = size;
            Hash = hash;
        }

        /// <summary>
        /// Gets a value indicating the zero-based piece index.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the piece size, in bytes.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets the SHA-1 hash for this piece.
        /// </summary>
        public Sha1Hash Hash { get; }
    }
}
