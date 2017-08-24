// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using TorrentCore.Transport;

namespace TorrentCore.Application
{
    /// <summary>
    /// Handshakes <see cref="ITransportStream"/> connections with new peers and determines which <see cref="IApplicationProtocol"/>
    /// new connections should be given to.
    /// </summary>
    /// <typeparam name="TPeerConnection">The type that represents connections to peers.</typeparam>
    /// <typeparam name="TPreparationContext">The context supplied when accepting an incoming connection.</typeparam>
    /// <typeparam name="TConnectionArgs">The connection args supplied when a new connection is accepted or initiated.</typeparam>
    public interface IApplicationProtocolPeerInitiator<TPeerConnection, TPreparationContext, in TConnectionArgs>
    {
        IApplicationProtocol<TPeerConnection> PrepareAcceptIncomingConnection(ITransportStream transportStream,
                                                                              out TPreparationContext context);

        TPeerConnection AcceptIncomingConnection(ITransportStream transportStream,
                                                 TPreparationContext context,
                                                 TConnectionArgs c);

        TPeerConnection InitiateOutgoingConnection(ITransportStream transportStream, TConnectionArgs c);
    }
}
