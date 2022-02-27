// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Data;

namespace TorrentCore.Application.BitTorrent.Messages;

/// <summary>
/// Provides methods to help with processing incoming messages.
/// </summary>
static class MessageHandler
{
    private static readonly Dictionary<byte, Func<Metainfo, CommonPeerMessage>> MessageDelegates = new Dictionary<byte, Func<Metainfo, CommonPeerMessage>>();

    static MessageHandler()
    {
        // Register message types
        MessageDelegates.Add(ChokeMessage.MessageID, meta => new ChokeMessage());
        MessageDelegates.Add(UnchokeMessage.MessageID, meta => new UnchokeMessage());
        MessageDelegates.Add(InterestedMessage.MessageID, meta => new InterestedMessage());
        MessageDelegates.Add(NotInterestedMessage.MessageID, meta => new NotInterestedMessage());
        MessageDelegates.Add(HaveMessage.MessageId, meta => new HaveMessage());
        MessageDelegates.Add(BitfieldMessage.MessageId, meta => new BitfieldMessage(meta.Pieces.Count));
        MessageDelegates.Add(RequestMessage.MessageID, meta => new RequestMessage());
        MessageDelegates.Add(PieceMessage.MessageId, meta => new PieceMessage());
        MessageDelegates.Add(CancelMessage.MessageID, meta => new CancelMessage());
    }

    /// <summary>
    /// Reads the incoming message from the specified reader.
    /// </summary>
    /// <param name="meta">Metainfo for the download.</param>
    /// <param name="reader">The data reader to read from.</param>
    /// <param name="messageLength">The length of the incoming message.</param>
    /// <param name="messageId">The message ID of the incoming message.</param>
    public static CommonPeerMessage? ReadMessage(Metainfo meta, BinaryReader reader, int messageLength, byte messageId)
    {
        if (MessageDelegates.TryGetValue(messageId, out var creationDelegate))
        {
            CommonPeerMessage message = creationDelegate(meta);
            message.Receive(reader, messageLength);
            return message;
        }

        return null;
    }
}
