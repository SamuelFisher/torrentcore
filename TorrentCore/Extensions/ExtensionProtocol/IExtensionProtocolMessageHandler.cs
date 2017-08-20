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
using System.Text;

namespace TorrentCore.Extensions.ExtensionProtocol
{
    public interface IExtensionProtocolMessageHandler
    {
        IReadOnlyDictionary<string, Func<IExtensionProtocolMessage>> SupportedMessageTypes { get; }

        /// <summary>
        /// Invoked when an extension protocol handshake message is about to be sent.
        /// </summary>
        /// <param name="context">Contextual information about the handshake.</param>
        void PrepareExtensionProtocolHandshake(IPrepareExtensionProtocolHandshakeContext context);

        /// <summary>
        /// Invoked when a peer that has indicated support for one of the <see cref="SupportedMessageTypes"/>
        /// has connected.
        /// </summary>
        /// <param name="context">Contextual information about the peer.</param>
        void PeerConnected(IExtensionProtocolPeerContext context);

        /// <summary>
        /// Invoked when an extension protocol message is received.
        /// </summary>
        /// <param name="context">Contextual information about the message.</param>
        void MessageReceived(IExtensionProtocolMessageReceivedContext context);
    }
}
