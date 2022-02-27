// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Application.BitTorrent.Messages;

/// <summary>
/// A message used to request a block from a peer.
/// </summary>
class RequestMessage : CommonPeerMessage
{
    public const byte MessageID = 6;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestMessage"/> class that is empty.
    /// </summary>
    #nullable enable
    public RequestMessage()
    {
    }
    #nullable disable

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestMessage"/> class to request the specified block.
    /// </summary>
    /// <param name="block">The block to request.</param>
    public RequestMessage(BlockRequest block)
    {
        Block = block;
    }

    /// <summary>
    /// Gets the ID of the message.
    /// </summary>
    public override byte ID => MessageID;

    /// <summary>
    /// Gets or sets the block associated with this request.
    /// </summary>
    public BlockRequest Block { get; set; }

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
