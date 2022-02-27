// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Data;

namespace TorrentCore.Application.BitTorrent.Messages;

/// <summary>
/// A message used to inform a peer that it has a specific piece available.
/// </summary>
public class HaveMessage : CommonPeerMessage
{
    public const byte MessageId = 4;

    /// <summary>
    /// Initializes a new instance of the <see cref="HaveMessage"/> class.
    /// </summary>
    #nullable disable
    public HaveMessage()
    {
    }
    #nullable enable

    /// <summary>
    /// Initializes a new instance of the <see cref="HaveMessage"/> class indicating the specified piece is available.
    /// </summary>
    /// <param name="piece">The piece that is available.</param>
    public HaveMessage(Piece piece)
    {
        Piece = piece;
    }

    /// <summary>
    /// Gets the ID of the message.
    /// </summary>
    public override byte ID => MessageId;

    /// <summary>
    /// Gets the piece that is available.
    /// </summary>
    public Piece Piece { get; private set; }

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
        Piece = new Piece(pieceIndex);
    }
}
