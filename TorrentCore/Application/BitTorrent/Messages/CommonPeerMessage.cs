// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Application.BitTorrent.Messages;

/// <summary>
/// Represents a message with an ID and length sent to or received from a remote peer.
/// </summary>
public abstract class CommonPeerMessage : IPeerMessage
{
    /// <summary>
    /// Gets the ID of the message.
    /// </summary>
    public abstract byte ID { get; }

    /// <summary>
    /// Sends the message by writing it to the specified BinaryWriter.
    /// </summary>
    /// <param name="writer">The writer to use.</param>
    public virtual void Send(BinaryWriter writer)
    {
        // Message ID
        writer.Write(ID);

        // Store to send
        writer.Flush();
    }

    /// <summary>
    /// Receives a message by reading it from the specified reader.
    /// <remarks>The length and ID of the message have already been read.</remarks>
    /// </summary>
    /// <param name="reader">The reader to use.</param>
    /// <param name="length">The length of the message, in bytes.</param>
    public virtual void Receive(BinaryReader reader, int length)
    {
        // Do nothing
    }
}
