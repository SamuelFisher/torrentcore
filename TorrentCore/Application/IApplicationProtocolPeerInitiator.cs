// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Transport;

namespace TorrentCore.Application;

/// <summary>
/// Handshakes <see cref="ITransportStream"/> connections with new peers and determines which <see cref="IApplicationProtocol"/>
/// new connections should be given to.
/// </summary>
public interface IApplicationProtocolPeerInitiator
{
    void OnApplicationProtocolAdded(IApplicationProtocol instance);

    void OnApplicationProtocolRemoved(IApplicationProtocol instance);

    void AcceptIncomingConnection(AcceptConnectionEventArgs e);

    IPeer InitiateOutgoingConnection(ITransportStream transportStream, IApplicationProtocol applicationProtocol);
}
