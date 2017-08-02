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
    public interface IApplicationProtocolPeerInitiator<TPeerConnection, in TConnectionArgs>
    {
        IApplicationProtocol<TPeerConnection> PrepareAcceptIncomingConnection(ITransportStream transportStream);
        TPeerConnection InitiateOutgoingConnection(ITransportStream transportStream, TConnectionArgs c);
    }
}
