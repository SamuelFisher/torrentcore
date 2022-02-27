// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Modularity;

namespace TorrentCore.Extensions.ExtensionProtocol;

public interface IExtensionProtocolPeerContext : IPeerContext
{
    /// <summary>
    /// Gets the message types this peer has indicated support for.
    /// </summary>
    IReadOnlyCollection<string> SupportedMessageTypes { get; }

    /// <summary>
    /// Sends an extension protocol message to this peer.
    /// </summary>
    /// <param name="message">The message to send.</param>
    void SendMessage(IExtensionProtocolMessage message);
}
