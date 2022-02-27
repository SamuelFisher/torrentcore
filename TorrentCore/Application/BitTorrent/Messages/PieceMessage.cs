// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Data;

namespace TorrentCore.Application.BitTorrent.Messages;

/// <summary>
/// A message used to transmit blocks of data.
/// </summary>
class PieceMessage : CommonPeerMessage
{
    public const byte MessageId = 7;

    /// <summary>
    /// Initializes a new instance of the <see cref="PieceMessage"/> class.
    /// </summary>
    #nullable disable
    public PieceMessage()
    {
    }
    #nullable enable

    /// <summary>
    /// Initializes a new instance of the <see cref="PieceMessage"/> class with the specified block.
    /// </summary>
    /// <param name="block">The block to be represented by the piece message.</param>
    public PieceMessage(Block block)
    {
        Block = block;
    }

    /// <summary>
    /// Gets the ID of the message.
    /// </summary>
    public override byte ID => MessageId;

    /// <summary>
    /// Gets the block this piece message represents.
    /// </summary>
    public Block Block { get; private set; }

    /// <summary>
    /// Sends the message by writing it to the specified BinaryWriter.
    /// </summary>
    /// <param name="writer">The writer to use.</param>
    public override void Send(BinaryWriter writer)
    {
        // Message ID
        writer.Write(ID);

        // Piece index of block
        writer.Write(Block.PieceIndex);

        // Block offset within piece
        writer.Write(Block.Offset);

        // Block data
        writer.Write(Block.Data);

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
        // Piece index of block
        int pieceIndex = reader.ReadInt32();

        // Block offset within piece
        int blockOffset = reader.ReadInt32();

        // Block data
        byte[] blockData = reader.ReadBytes(length - 8);

        // Create block
        Block = new Block(pieceIndex, blockOffset, blockData);
    }
}
