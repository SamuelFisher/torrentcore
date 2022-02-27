// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Extensions.ExtensionProtocol;

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
