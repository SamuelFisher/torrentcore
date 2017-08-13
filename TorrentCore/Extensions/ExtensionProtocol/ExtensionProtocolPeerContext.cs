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
using System.Text;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Data;
using TorrentCore.Data.Pieces;
using TorrentCore.Modularity;
using TorrentCore.Transport;

namespace TorrentCore.Extensions.ExtensionProtocol
{
    class ExtensionProtocolPeerContext : IExtensionProtocolPeerContext
    {
        private readonly IPeerContext peerContext;
        private readonly Action<IExtensionProtocolMessage> sendMessage;

        public ExtensionProtocolPeerContext(IPeerContext peerContext,
                                            Action<IExtensionProtocolMessage> sendMessage)
        {
            this.peerContext = peerContext;
            this.sendMessage = sendMessage;
        }

        public IReadOnlyCollection<string> SupportedMessageTypes =>
            peerContext.GetValue<Dictionary<string, byte>>(ExtensionProtocolModule.ExtensionProtocolMessageIds).Keys;

        public void SendMessage(IExtensionProtocolMessage message)
        {
            sendMessage(message);
        }

        #region IPeerContext Members

        public IBlockRequests BlockRequests => peerContext.BlockRequests;

        public PeerConnection Peer => peerContext.Peer;

        public Metainfo Metainfo => peerContext.Metainfo;

        public IReadOnlyCollection<PeerConnection> Peers => peerContext.Peers;

        public IPieceDataHandler DataHandler => peerContext.DataHandler;

        public void PeersAvailable(IEnumerable<ITransportStream> peers)
        {
            peerContext.PeersAvailable(peers);
        }
        
        public T GetValue<T>(string key)
        {
            return peerContext.GetValue<T>(key);
        }

        public void SetValue<T>(string key, T value)
        {
            peerContext.SetValue(key, value);
        }

        public void RegisterMessageHandler(byte messageId)
        {
            peerContext.RegisterMessageHandler(messageId);
        }

        public void SendMessage(byte messageId, byte[] data)
        {
            peerContext.SendMessage(messageId, data);
        }

        #endregion
    }
}
