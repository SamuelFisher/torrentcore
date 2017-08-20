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
using Microsoft.Extensions.Logging;

namespace TorrentCore.Data
{
    /// <summary>
    /// Provides an intermediate data store using the decorator pattern.
    /// Writes to the backing store when an entire piece successfully downloads and
    /// keeps track which pieces have completed.
    /// </summary>
    internal class PieceCheckerHandler : IPieceDataHandler
    {
        private static readonly ILogger Log = LogManager.GetLogger<PieceCheckerHandler>();

        private readonly IBlockDataHandler baseHandler;
        private readonly Dictionary<Piece, SortedSet<Block>> pendingBlocks;
        private readonly HashSet<Piece> completedPieces;

        /// <summary>
        /// Initializes a new instance of the <see cref="PieceCheckerHandler"/> class,
        /// using the provided file handler as the backing store.
        /// </summary>
        /// <param name="baseHandler">File handler to use as backing store.</param>
        public PieceCheckerHandler(IBlockDataHandler baseHandler)
        {
            this.baseHandler = baseHandler;
            pendingBlocks = new Dictionary<Piece, SortedSet<Block>>(Metainfo.Pieces.Count);
            completedPieces = new HashSet<Piece>();
        }

        /// <summary>
        /// Called when a piece successfully completes downloading.
        /// </summary>
        public event Action<Piece> PieceCompleted;

        /// <summary>
        /// Called when a piece finishes downloading but contains corrupted data.
        /// </summary>
        public event Action<Piece> PieceCorrupted;

        /// <summary>
        /// Gets the metainfo describing the layout of the collection of files.
        /// </summary>
        public Metainfo Metainfo => baseHandler.Metainfo;
        
        /// <summary>
        /// Gets the pieces that have already completed downloading.
        /// </summary>
        public IReadOnlyCollection<Piece> CompletedPieces => completedPieces;

        public void MarkPieceAsCompleted(Piece piece)
        {
            completedPieces.Add(piece);
        }

        /// <summary>
        /// Returns a copy of the contiguous file data starting at the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <param name="length">Number of bytes to read.</param>
        /// <returns>Block data from specified region.</returns>
        public byte[] ReadBlockData(long offset, long length)
        {
            return baseHandler.ReadBlockData(offset, length);
        }

        /// <summary>
        /// Returns a copy of the contiguous file data starting at the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <param name="length">Number of bytes to read.</param>
        /// <param name="data">Returned data.</param>
        /// <returns>Block data from specified region.</returns>
        public bool TryReadBlockData(long offset, long length, out byte[] data)
        {
            return baseHandler.TryReadBlockData(offset, length, out data);
        }

        /// <summary>
        /// Writes the specified contiguous data from the given offset position.
        /// </summary>
        /// <param name="offset">Offset at which to start writing.</param>
        /// <param name="data">Block data to write.</param>
        public void WriteBlockData(long offset, byte[] data)
        {
            int pieceIndex = (int)(offset / Metainfo.PieceSize);
            int pieceOffset = (int)(offset % Metainfo.PieceSize);
            Block block = new Block(pieceIndex, pieceOffset, data);
            if (!pendingBlocks.ContainsKey(Metainfo.Pieces[pieceIndex]))
                pendingBlocks.Add(Metainfo.Pieces[pieceIndex], new SortedSet<Block>(new BlockComparer()));
            pendingBlocks[Metainfo.Pieces[pieceIndex]].Add(block);

            WriteCompletedPieces();
        }

        /// <summary>
        /// Writes pending blocks that make up a full piece.
        /// </summary>
        void WriteCompletedPieces()
        {
            var completed = GetCompletedPieces(pendingBlocks);

            foreach (var piece in completed)
            {
                byte[] data;
                if (VerifyPiece(piece.Key, piece.Value, out data))
                {
                    Log.LogDebug($"Writing piece #{piece.Key.Index}");

                    // Write piece
                    long pieceOffset = Metainfo.PieceOffset(piece.Key);
                    baseHandler.WriteBlockData(pieceOffset, data);

                    completedPieces.Add(piece.Key);

                    // Notify of completed piece
                    PieceCompleted?.Invoke(piece.Key);
                }
                else
                {
                    Log.LogInformation($"Piece #{piece.Key.Index} is corrupted");

                    // Notify of corrupted piece
                    PieceCorrupted?.Invoke(piece.Key);
                }
            }
        }

        /// <summary>
        /// Finds pending blocks that make up a full piece.
        /// </summary>
        /// <param name="pendingBlocks">Blocks to check.</param>
        internal static Dictionary<Piece, SortedSet<Block>> GetCompletedPieces(Dictionary<Piece, SortedSet<Block>> pendingBlocks)
        {
            Dictionary<Piece, SortedSet<Block>> foundPieces = new Dictionary<Piece, SortedSet<Block>>();

            foreach (Piece piece in pendingBlocks.Keys)
            {
                int offset = 0;
                foreach (Block block in pendingBlocks[piece])
                {
                    if (block.Offset == offset)
                        offset += block.Length;
                    else
                        break;
                }

                if (offset == piece.Size)
                    foundPieces.Add(piece, pendingBlocks[piece]);
            }

            foreach (Piece piece in foundPieces.Keys)
                pendingBlocks.Remove(piece);

            return foundPieces;
        }

        private bool VerifyPiece(Piece piece, SortedSet<Block> blocks, out byte[] data)
        {
            using (var dataStream = new MemoryStream())
            {
                // Concatenate blocks
                foreach (var block in blocks)
                    dataStream.Write(block.Data, 0, block.Data.Length);

                data = dataStream.ToArray();

                using (var sha1 = SHA1.Create())
                {
                    byte[] hash = sha1.ComputeHash(data);
                    return Enumerable.SequenceEqual(hash, piece.Hash.Value);
                }
            }
        }
    }
}
