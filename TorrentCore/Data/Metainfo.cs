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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentCore.Data
{
    /// <summary>
    /// Describes a set of files.
    /// </summary>
    public class Metainfo
    {
        private readonly List<Piece> pieces;

        /// <summary>
        /// Creates a new Metainfo with the specified data.
        /// </summary>
        /// <param name="name">The name of the torrent.</param>
        /// <param name="infoHash">SHA-1 hash of the metadata.</param>
        /// <param name="files">List of files to include.</param>
        /// <param name="pieces">List of pieces to include.</param>
        /// <param name="trackers">URLs of the trackers.</param>
        public Metainfo(string name,
                        Sha1Hash infoHash,
                        IEnumerable<ContainedFile> files,
                        IEnumerable<Piece> pieces,
                        IEnumerable<IEnumerable<Uri>> trackers)
        {
            Name = name;
            this.pieces = new List<Piece>();
            this.pieces.AddRange(pieces);
            InfoHash = infoHash;
            Files = new List<ContainedFile>();
            Files.AddRange(files);
            TotalSize = Files.Any() ? Files.Sum(f => f.Size) : 0;
            Trackers = trackers.Select(x => (IReadOnlyList<Uri>)new ReadOnlyCollection<Uri>(x.ToList())).ToList().AsReadOnly();
            PieceSize = this.pieces.First().Size;
        }

        /// <summary>
        /// Gets the name of the torrent.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a hash of the data for this set of files.
        /// </summary>
        public Sha1Hash InfoHash { get; }

        /// <summary>
        /// Gets the list of files contained within this collection.
        /// </summary>
        public List<ContainedFile> Files { get; }

        /// <summary>
        /// Gets the total size in bytes of this set of files.
        /// </summary>
        public long TotalSize { get; }

        /// <summary>
        /// Gets the uris of the trackers managing downloads for this set of files.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<Uri>> Trackers { get; }

        /// <summary>
        /// Gets a value indicating the number of bytes in each piece.
        /// </summary>
        public int PieceSize { get; }

        /// <summary>
        /// Gets a read-only list of pieces for this set of files.
        /// </summary>
        public IReadOnlyList<Piece> Pieces => pieces.AsReadOnly();

        /// <summary>
        /// Returns the offset in bytes to the start of the specified piece.
        /// </summary>
        /// <param name="piece">The piece to calculate the offset for.</param>
        /// <returns>Offset in bytes.</returns>
        public long PieceOffset(Piece piece)
        {
            return piece.Index * PieceSize;
        }

        /// <summary>
        /// Returns the index of the first file containing the data at the specified offset.
        /// </summary>
        /// <param name="offset">Offset into the file data, in bytes.</param>
        /// <returns>Index of file for the specified offset.</returns>
        public int FileIndex(long offset)
        {
            long remainder;
            return FileIndex(offset, out remainder);
        }

        /// <summary>
        /// Returns the index of the first file containing the data at the specified offset.
        /// </summary>
        /// <param name="offset">Offset into the file data, in bytes.</param>
        /// <param name="remainder">Offset into file at which data begins.</param>
        /// <returns>Index of file for the specified offset.</returns>
        public int FileIndex(long offset, out long remainder)
        {
            if (offset < 0)
                throw new IndexOutOfRangeException();

            if (Files.Count == 0)
                throw new IndexOutOfRangeException();

            int fileIndex = 0;
            while (offset > Files[fileIndex].Size)
            {
                ContainedFile result = Files[fileIndex];
                offset -= result.Size;
                fileIndex++;

                if (fileIndex > Files.Count)
                    throw new IndexOutOfRangeException();
            }

            remainder = offset;
            return fileIndex;
        }

        /// <summary>
        /// Returns the offset in the file block data at which the data for the specified file begins.
        /// </summary>
        /// <param name="fileIndex">Index of file to find offset for.</param>
        /// <returns>Offset to file in bytes.</returns>
        public long FileOffset(int fileIndex)
        {
            long offset = 0;
            for (int i = 0; i < fileIndex; i++)
                offset += Files[i].Size;
            return offset;
        }
    }
}
