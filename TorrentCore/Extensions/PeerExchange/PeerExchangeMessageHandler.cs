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
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using TorrentCore.Extensions.ExtensionProtocol;
using TorrentCore.Transport;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Extensions.PeerExchange
{
    public class PeerExchangeMessageHandler : IExtensionProtocolMessageHandler
    {
        private static readonly ILogger Log = LogManager.GetLogger<PeerExchangeMessageHandler>();

        private readonly IPAddress adapterAddress;

        public IReadOnlyDictionary<string, Func<IExtensionProtocolMessage>> SupportedMessageTypes { get; } = new Dictionary<string, Func<IExtensionProtocolMessage>>
        {
            [PeerExchangeMessage.MessageType] = () => new PeerExchangeMessage()
        };

        public PeerExchangeMessageHandler(IPAddress adapterAddress)
        {
            this.adapterAddress = adapterAddress;
        }

        public void PeerConnected(IExtensionProtocolPeerContext context)
        {
        }

        public void MessageReceived(IExtensionProtocolMessageReceivedContext context)
        {
            var message = context.Message as PeerExchangeMessage;
            if (message == null)
                throw new InvalidOperationException($"Expected a {nameof(PeerExchangeMessage)} but received a {context.Message.GetType().Name}");
            
            Log.LogDebug($"{message.Added.Count} peers received from PEX message");

            context.PeersAvailable(message.Added.Select(CreateTransportStream));
        }

        private ITransportStream CreateTransportStream(IPEndPoint endPoint)
        {
            return new TcpTransportStream(adapterAddress, endPoint.Address, endPoint.Port);
        }
    }
}
