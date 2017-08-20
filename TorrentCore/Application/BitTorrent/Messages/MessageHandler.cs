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
        public static CommonPeerMessage ReadMessage(Metainfo meta, BinaryReader reader, int messageLength, byte messageId)
        {
            Func<Metainfo, CommonPeerMessage> creationDelegate;
            if (MessageDelegates.TryGetValue(messageId, out creationDelegate))
            {
                CommonPeerMessage message = creationDelegate(meta);
                message.Receive(reader, messageLength);
                return message;
            }

            return null;
        }
    }
}
