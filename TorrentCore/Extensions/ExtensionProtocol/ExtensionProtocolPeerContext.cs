// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

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
    internal partial class ExtensionProtocolPeerContext : IExtensionProtocolPeerContext
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
    }

    internal partial class ExtensionProtocolPeerContext : IPeerContext
    {
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
    }
}
