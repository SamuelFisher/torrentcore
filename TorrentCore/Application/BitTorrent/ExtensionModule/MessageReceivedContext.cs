// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TorrentCore.Data;
using TorrentCore.Modularity;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent.ExtensionModule
{
    class MessageReceivedContext : PeerContext, IMessageReceivedContext
    {
        public MessageReceivedContext(
            PeerConnection peer,
            ITorrentContext torrentContext,
            int messageId,
            int messageLength,
            BinaryReader reader,
            Dictionary<string, object> customValues,
            Action<byte> registerMessageHandler)
            : base(peer, customValues, torrentContext, registerMessageHandler)
        {
            Reader = reader;
            MessageId = messageId;
            MessageLength = messageLength;
        }

        public int MessageId { get; }

        public int MessageLength { get; }

        public BinaryReader Reader { get; }
    }
}
