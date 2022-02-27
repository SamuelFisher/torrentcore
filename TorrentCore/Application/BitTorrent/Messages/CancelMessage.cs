// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Application.BitTorrent.Messages;

/// <summary>
/// A message used to cancel the request of a block of data.
/// </summary>
class CancelMessage : CommonPeerMessage
{
    public const byte MessageID = 8;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelMessage"/> class that is empty.
    /// </summary>
    #nullable enable
    public CancelMessage()
    {
    }
    #nullable disable

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelMessage"/> class with the specified block.
    /// </summary>
    /// <param name="block">The block request to be cancelled by the cancel message.</param>
    public CancelMessage(BlockRequest block)
    {
        Block = block;
    }

    /// <summary>
    /// Gets the ID of the message.
    /// </summary>
    public override byte ID => MessageID;

    /// <summary>
    /// Gets the block request to be cancelled by this message.
    /// </summary>
    public BlockRequest Block { get; private set; }

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

        // Block length
        writer.Write(Block.Length);

        // Flush to send
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

        // Block length
        int blockLength = reader.ReadInt32();

        // Set block
        Block = new BlockRequest(pieceIndex, blockOffset, blockLength);
    }
}
