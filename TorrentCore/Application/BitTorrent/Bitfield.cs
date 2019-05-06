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
using TorrentCore.Data;

namespace TorrentCore.Application.BitTorrent
{
    /// <summary>
    /// Represents the pieces a client has available.
    /// </summary>
    public class Bitfield
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bitfield"/> class,
        /// with the specified length and all bits set to zero.
        /// </summary>
        /// <param name="length">Number of pieces within the bitfield.</param>
        public Bitfield(int length)
        {
            Length = length;
            RawData = new byte[(int)Math.Ceiling((double)length / 8)];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitfield"/> class with the specified data and length.
        /// </summary>
        /// <param name="rawData">The raw data to use.</param>
        /// <param name="length">The length of the bitfield.</param>
        public Bitfield(byte[] rawData, int length)
        {
            RawData = rawData;
            Length = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitfield"/> class,
        /// marking the specified pieces as available.
        /// </summary>
        /// <param name="length">The length of the bitfield.</param>
        /// <param name="availablePieces">Set of available pieces.</param>
        public Bitfield(int length, IReadOnlyCollection<Piece> availablePieces)
        {
            Length = length;
            RawData = new byte[(int)Math.Ceiling((double)length / 8)];

            foreach (var piece in availablePieces)
                SetPieceAvailable(piece.Index, true);
        }

        /// <summary>
        /// Gets the raw data for this bitfield.
        /// </summary>
        public byte[] RawData { get; }

        /// <summary>
        /// Gets the number of pieces contained within the bitfield.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Returns the number of available pieces.
        /// </summary>
        /// <returns>Number of available pieces.</returns>
        public int GetAvailablePiecesCount()
        {
            // Use quick check where possible
            List<int> quickChecked = new List<int>();
            for (int i = 0; i < RawData.Length - 1; i++)
            {
                if (RawData[i] == 255)
                    quickChecked.Add(i);
            }

            int available = quickChecked.Count * 8;
            for (int i = 0; i < Length; i++)
            {
                if (quickChecked.Contains((int)Math.Floor((double)i / 8)))
                    continue;
                else if (IsPieceAvailable(i))
                    available++;
            }

            return available;
        }

        /// <summary>
        /// Determines whether the specified piece is available.
        /// </summary>
        /// <param name="pieceIndex">Index of the piece to check.</param>
        /// <returns>True if the piece is available, false otherwise.</returns>
        public bool IsPieceAvailable(int pieceIndex)
        {
            if (pieceIndex > Length)
                throw new ArgumentOutOfRangeException(nameof(pieceIndex), "Piece index is beyond range of bitfield.");

            return (RawData[(int)Math.Floor((double)pieceIndex / 8)] & (0x80 >> pieceIndex % 8)) != 0;
        }

        /// <summary>
        /// Sets the availablility of the specified piece.
        /// </summary>
        /// <param name="pieceIndex">Index of the piece to set.</param>
        /// <param name="available">True if the piece is available, false otherwise.</param>
        public void SetPieceAvailable(int pieceIndex, bool available)
        {
            if (pieceIndex > Length)
                throw new ArgumentOutOfRangeException(nameof(pieceIndex), "Piece index is beyond range of bitfield.");

            int dataIndex = (int)Math.Floor((double)pieceIndex / 8);
            byte current = RawData[dataIndex];
            if (available)
                RawData[dataIndex] = (byte)(current | (0x80 >> (pieceIndex % 8)));
            else
                RawData[dataIndex] = (byte)(current & (current ^ (0x80 >> (pieceIndex % 8))));
        }

        /// <summary>
        /// Calculates the number of pieces which have not yet been downloaded.
        /// </summary>
        /// <returns>Number of pieces not yet downloaded.</returns>
        public int RemainingPiecesCount()
        {
            // Use quick check where possible
            List<int> quickChecked = new List<int>();
            for (int i = 0; i < RawData.Length - 1; i++)
            {
                if (RawData[i] == 255)
                    quickChecked.Add(i);
            }

            int unavailable = Length - quickChecked.Count * 8;
            for (int i = 0; i < Length; i++)
            {
                if (quickChecked.Contains((int)Math.Floor((double)i / 8)))
                    continue;
                else if (IsPieceAvailable(i))
                    unavailable--;
            }

            return unavailable;
        }

        public void Union(Bitfield bitfield)
        {
            for (int i = 0; i < RawData.Length; i++)
                RawData[i] |= bitfield.RawData[i];
        }

        public override string ToString()
        {
            return string.Format("Bitfield: {0}%", Math.Round((double)GetAvailablePiecesCount() / (double)Length, 2) * 100d);
        }

        /// <summary>
        /// Determines whether Bitfield a has pieces not available in Bitfield b.
        /// </summary>
        /// <param name="a">Bitfield to check for more pieces in.</param>
        /// <param name="b">Bitfield to compare against.</param>
        /// <returns>True if a has more pieces.</returns>
        public static bool NotSubset(Bitfield a, Bitfield b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a.IsPieceAvailable(i) && !b.IsPieceAvailable(i))
                    return true;
            }

            return false;
        }
    }
}
