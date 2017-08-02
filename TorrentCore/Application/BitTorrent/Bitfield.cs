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
using TorrentCore.Data;

namespace TorrentCore.Application.BitTorrent
{
    /// <summary>
    /// Represents the pieces a client has available.
    /// </summary>
    public class Bitfield
    {
        private byte[] data;

        /// <summary>
        /// Gets the raw data for this bitfield.
        /// </summary>
        public byte[] RawData
        {
            get { return data; }
        }

        /// <summary>
        /// Gets or sets a value indicating the number of pieces contained within the bitfield.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Returns the number of available pieces.
        /// </summary>
        /// <returns>Number of available pieces.</returns>
        public int GetAvailablePiecesCount()
        {
            // Use quick check where possible
            List<int> quickChecked = new List<int>();
            for (int i = 0; i < RawData.Length - 1; i++)
                if (RawData[i] == 255)
                    quickChecked.Add(i);

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
        /// Creates a new bitfield of the specified length, with all bits set to zero.
        /// </summary>
        /// <param name="length">Number of pieces within the bitfield.</param>
        public Bitfield(int length)
        {
            Length = length;
            data = new byte[(int)Math.Ceiling((double)length / 8)];
        }

        /// <summary>
        /// Creates a new bitfield using the specified data and length.
        /// </summary>
        /// <param name="rawData">The raw data to use.</param>
        /// <param name="length">The length of the bitfield.</param>
        public Bitfield(byte[] rawData, int length)
        {
            data = rawData;
            Length = length;
        }

        /// <summary>
        /// Creates a new bitfield, marking the specified pieces as available.
        /// </summary>
        /// <param name="length">The length of the bitfield.</param>
        /// <param name="availablePieces">Set of available pieces.</param>
        public Bitfield(int length, IReadOnlyCollection<Piece> availablePieces)
        {
            Length = length;
            data = new byte[(int)Math.Ceiling((double)length / 8)];

            foreach (var piece in availablePieces)
                SetPieceAvailable(piece.Index, true);
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

            return (data[(int)Math.Floor((double)pieceIndex / 8)] & (0x80 >> pieceIndex % 8)) != 0;
        }

        /// <summary>
        /// Sets the availablility of the specified piece.
        /// </summary>
        /// <param name="pieceIndex">Index of the piece to set.</param>
        /// <param name="available">True if the piece is available, false otherwise.</param>
        public void SetPieceAvailable(int pieceIndex, bool available)
        {
            if (pieceIndex > Length)
                throw new ArgumentOutOfRangeException("pieceIndex", "Piece index is beyond range of bitfield.");

            int dataIndex = (int)Math.Floor((double)pieceIndex / 8);
            byte current = data[dataIndex];
            if (available)
                data[dataIndex] = (byte)(current | (0x80 >> (pieceIndex % 8)));
            else
                data[dataIndex] = (byte)(current & (current ^ (0x80 >> (pieceIndex % 8))));
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
                if (RawData[i] == 255)
                    quickChecked.Add(i);

            int unavailable = this.Length - quickChecked.Count * 8;
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
            return String.Format("Bitfield: {0}%", Math.Round((double)GetAvailablePiecesCount() / (double)Length, 2) * 100d);
        }

        /// <summary>
        /// Determines whether Bitfield a has pieces not available in Bitfield b.
        /// </summary>
        /// <param name="a">Bitfield to check for more pieces in.</param>
        /// <param name="b">Bitfield to compare against.</param>
        /// <returns>True if a has more pieces </returns>
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
