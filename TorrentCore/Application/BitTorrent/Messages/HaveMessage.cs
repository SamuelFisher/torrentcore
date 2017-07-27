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
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Data;

namespace TorrentCore.Application.BitTorrent.Messages
{
    /// <summary>
    /// A message used to inform a peer that it has a specific piece available.
    /// </summary>
    public class HaveMessage : CommonPeerMessage
    {
        public const byte MessageId = 4;

        /// <summary>
        /// The ID of the message.
        /// </summary>
        public override byte ID
        {
            get { return MessageId; }
        }

        /// <summary>
        /// The piece that is available.
        /// </summary>
        public Piece Piece { get; private set; }

        /// <summary>
        /// Creates a new, empty have message.
        /// </summary>
        public HaveMessage()
        {
        }

        /// <summary>
        /// Creates a new have message with the specified piece.
        /// </summary>
        /// <param name="piece">The piece that is available.</param>
        public HaveMessage(Piece piece)
        {
            this.Piece = piece;
        }

        /// <summary>
        /// Sends the message by writing it to the specified BinaryWriter.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public override void Send(BinaryWriter writer)
        {
            // Message ID
            writer.Write(ID);
            // Index of piece
            writer.Write(Piece.Index);
            // Store to send
            writer.Flush();
        }

        /// <summary>
        /// Receives a message by reading it from the specified reader.
        /// <remarks>The length and ID of the message have already been read.</remarks>
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="length">The length of the message, in bytes.</param>
        public override void Receive(BinaryReader reader, int length)
        {
            // Index of piece
            int pieceIndex = reader.ReadInt32();

            // Create piece object
            this.Piece = new Piece(pieceIndex);
        }
    }
}
