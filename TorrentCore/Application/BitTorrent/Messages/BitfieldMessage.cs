// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentCore.Application.BitTorrent.Messages
{
    /// <summary>
    /// A message used to indicate which pieces the peer has available.
    /// </summary>
    class BitfieldMessage : CommonPeerMessage
    {
        public const byte MessageId = 5;

        private readonly int bitfieldLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="BitfieldMessage"/> class.
        /// The bitfield length is used later when receiving the message.
        /// </summary>
        /// <param name="bitfieldLength">Number of pieces in the bitfield.</param>
        public BitfieldMessage(int bitfieldLength)
        {
            this.bitfieldLength = bitfieldLength;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BitfieldMessage"/> class.
        /// Creates a new bitfield message with the specified bitfield.
        /// </summary>
        /// <param name="bitfield">The bitfield to use.</param>
        public BitfieldMessage(Bitfield bitfield)
        {
            Bitfield = bitfield;
        }

        /// <summary>
        /// Gets the ID of the message.
        /// </summary>
        public override byte ID => MessageId;

        /// <summary>
        /// Gets the bitfield associated with this message.
        /// </summary>
        public Bitfield Bitfield { get; private set; }

        /// <summary>
        /// Sends the message by writing it to the specified BinaryWriter.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public override void Send(BinaryWriter writer)
        {
            // Message ID
            writer.Write(ID);

            // Bitfield
            writer.Write(Bitfield.RawData);

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
            // Bitfield
            byte[] bitfieldData = reader.ReadBytes(length);
            Bitfield = new Bitfield(bitfieldData, bitfieldLength);
        }
    }
}
