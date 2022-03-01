// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Modularity;

namespace TorrentCore.Extensions.ExtensionProtocol;

class ExtensionProtocolMessageReceivedContext : ExtensionProtocolPeerContext, IExtensionProtocolMessageReceivedContext
{
    public ExtensionProtocolMessageReceivedContext(
        IExtensionProtocolMessage message,
        IPeerContext peerContext,
        Action<IExtensionProtocolMessage> sendMessage)
        : base(peerContext, sendMessage)
    {
        Message = message;
    }

    public IExtensionProtocolMessage Message { get; }
}
