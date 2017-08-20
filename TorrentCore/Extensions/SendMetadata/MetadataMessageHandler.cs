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
using System.Threading;
using System.Threading.Tasks;
using BencodeNET.Objects;
using TorrentCore.Data;
using TorrentCore.Extensions.ExtensionProtocol;
using TorrentCore.Modularity;
using TorrentCore.Modularity.MetainfoProvider;

namespace TorrentCore.Extensions.SendMetadata
{
    public class MetadataMessageHandler : IExtensionProtocolMessageHandler, IMetainfoProvider
    {
        private const string MetadataSize = "metadata_size";

        public IReadOnlyDictionary<string, Func<IExtensionProtocolMessage>> SupportedMessageTypes { get; } = new Dictionary<string, Func<IExtensionProtocolMessage>>
        {
            [MetadataMessage.MessageType] = () => new MetadataMessage()
        };

        public void PrepareExtensionProtocolHandshake(IPrepareExtensionProtocolHandshakeContext context)
        {
            if (context.Metainfo.Metadata != null)
                context.HandshakeContent[MetadataSize] = new BNumber(context.Metainfo.Metadata.Count);
        }

        public void PeerConnected(IExtensionProtocolPeerContext context)
        {
            if (!context.SupportedMessageTypes.Contains(MetadataMessage.MessageType))
                return;

            context.SendMessage(new MetadataMessage
            {
                RequestType = MetadataMessage.Type.Request,
                PieceIndex = 0
            });
        }

        public void MessageReceived(IExtensionProtocolMessageReceivedContext context)
        {
            var message = context.Message as MetadataMessage;
            if (message == null)
                throw new InvalidOperationException($"Expected a {nameof(MetadataMessage)} but received a {context.Message.GetType().Name}");
        }

        public Task<Metainfo> GetMetainfo(ITorrentContext context, CancellationToken ct)
        {
            // Try to download metadata from a peer supporting the metadata message
            throw new NotImplementedException();
        }
    }
}
