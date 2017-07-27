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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TorrentCore.Data
{
    class PieceCalculator
    {
        public static IList<Piece> ComputePieces(IList<ContainedFile> files, int pieceSize, IFileHandler fileHandler)
        {
            var pieces = new List<Piece>();

            if (files.Count == 0)
                return pieces;

            long totalLength = files.Sum(f => f.Size);

            int currentFile = 0;
            Stream fileStream = fileHandler.GetFileStream(files[currentFile].Name);
            long offset = 0;
            int pieceCounter = 0;

            using (var sha1 = SHA1.Create())
            {
                // Full pieces
                while (offset <= totalLength - pieceSize && totalLength - pieceSize > 0)
                {
                    byte[] data = GetBlockData(fileHandler, files, pieceSize, ref currentFile, ref fileStream);

                    Sha1Hash pieceHash = new Sha1Hash(sha1.ComputeHash(data));
                    Piece piece = new Piece(pieceCounter, pieceSize, pieceHash);
                    pieces.Add(piece);
                    offset += pieceSize;
                    pieceCounter++;
                }

                // Remaining smaller piece
                long remaining = totalLength - offset;
                if (remaining > 0)
                {
                    byte[] remainingData = GetBlockData(fileHandler, files, remaining, ref currentFile, ref fileStream);

                    Sha1Hash pieceHash = new Sha1Hash(sha1.ComputeHash(remainingData));
                    Piece piece = new Piece(pieceCounter, (int)remaining, pieceHash);
                    pieces.Add(piece);
                }
            }

            return pieces;
        }

        private static byte[] GetBlockData(IFileHandler fileHandler, IList<ContainedFile> files, long length, ref int currentFile, ref Stream fileStream)
        {
            // Fill the current piece with file data
            int copied = 0;
            var data = new byte[length];
            while (copied < length)
            {
                // Move to next file if necessary
                if (fileStream.Position == files[currentFile].Size)
                {
                    currentFile++;
                    fileStream = fileHandler.GetFileStream(files[currentFile].Name);
                }

                // Copy to end of file, or end of piece
                int toRead = (int)Math.Min(length, files[currentFile].Size - fileStream.Position);

                // Check if going beyond end of file
                if (fileStream.Length - fileStream.Position < toRead)
                    throw new InvalidOperationException("Tried to read beyond the end of the file.");

                fileStream.Read(data, copied, toRead);
                copied += toRead;
            }

            return data;
        }
    }
}
