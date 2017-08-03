// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2017 Samuel Fisher.
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
using System.Reflection;
using System.Text;
using TorrentCore.ExtensionModule;

namespace TorrentCore.Application.BitTorrent.ExtensionModule
{
    class MessageReceivedContext : PeerContext, IMessageReceivedContext
    {
        private readonly BinaryReader reader;

        public MessageReceivedContext(PeerConnection peer,
                                      int messageId,
                                      int messageLength,
                                      BinaryReader reader,
                                      Dictionary<string, object> customValues)
            : base(peer, customValues)
        {
            this.reader = reader;
            Peer = peer;
            MessageId = messageId;
            MessageLength = messageLength;
        }

        public PeerConnection Peer { get; }

        public int MessageId { get; }

        public int MessageLength { get; }

        public bool IsHandled { get; private set; }

        public BinaryReader Handle()
        {
            IsHandled = true;
            return reader;
        }
    }
}
