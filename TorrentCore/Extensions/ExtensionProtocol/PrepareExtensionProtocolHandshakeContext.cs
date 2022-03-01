// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using BencodeNET.Objects;
using TorrentCore.Modularity;

namespace TorrentCore.Extensions.ExtensionProtocol;

class PrepareExtensionProtocolHandshakeContext : ExtensionProtocolPeerContext, IPrepareExtensionProtocolHandshakeContext
{
    public PrepareExtensionProtocolHandshakeContext(
        BDictionary handshakeContent,
        IPeerContext peerContext,
        Action<IExtensionProtocolMessage> sendMessage)
        : base(peerContext, sendMessage)
    {
        HandshakeContent = handshakeContent;
    }

    public BDictionary HandshakeContent { get; }
}
