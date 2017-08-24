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
