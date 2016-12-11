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

namespace TorrentCore.Application.BitTorrent.Messages
{
    /// <summary>
    /// A message used to request a block from a peer.
    /// </summary>
    class RequestMessage : CommonPeerMessage
    {
        public const byte MessageID = 6;

        /// <summary>
        /// The ID of the message.
        /// </summary>
        public override byte ID
        {
            get { return MessageID; }
        }

        /// <summary>
        /// Gets or sets the block associated with this request.
        /// </summary>
        public BlockRequest Block { get; set; }

        /// <summary>
        /// Creates a new, empty RequestMessage.
        /// </summary>
        public RequestMessage()
        {
        }

        /// <summary>
        /// Creates a new RequestMessage to request the specified block.
        /// </summary>
        /// <param name="block">The block to request.</param>
        public RequestMessage(BlockRequest block)
        {
            this.Block = block;
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
            writer.Write(Block.PieceIndex);
            // Offset of block
            writer.Write(Block.Offset);
            // Length of block
            writer.Write(Block.Length);
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
            // Offset of block
            int blockOffset = reader.ReadInt32();
            // Length of block
            int blockLength = reader.ReadInt32();

            // Create block object
            Block = new BlockRequest(pieceIndex, blockOffset, blockLength);
        }
    }
}
