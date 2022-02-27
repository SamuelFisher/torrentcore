// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Extensions.Logging;
using TorrentCore.Extensions.ExtensionProtocol;
using TorrentCore.Transport;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Extensions.PeerExchange;

public class PeerExchangeMessageHandler : IExtensionProtocolMessageHandler
{
    private readonly ILogger<PeerExchangeMessageHandler> _logger;
    private readonly ITcpTransportProtocol _tcpTransportProtocol;

    public PeerExchangeMessageHandler(ILogger<PeerExchangeMessageHandler> logger, ITcpTransportProtocol tcpTransportProtocol)
    {
        _logger = logger;
        _tcpTransportProtocol = tcpTransportProtocol;
    }

    public IReadOnlyDictionary<string, Func<IExtensionProtocolMessage>> SupportedMessageTypes { get; } = new Dictionary<string, Func<IExtensionProtocolMessage>>
    {
        [PeerExchangeMessage.MessageType] = () => new PeerExchangeMessage(),
    };

    public void PrepareExtensionProtocolHandshake(IPrepareExtensionProtocolHandshakeContext context)
    {
    }

    public void PeerConnected(IExtensionProtocolPeerContext context)
    {
    }

    public void MessageReceived(IExtensionProtocolMessageReceivedContext context)
    {
        var message = context.Message as PeerExchangeMessage;
        if (message == null)
            throw new InvalidOperationException($"Expected a {nameof(PeerExchangeMessage)} but received a {context.Message.GetType().Name}");

        _logger.LogDebug($"{message.Added.Count} peers received from PEX message");
        context.PeersAvailable(message.Added.Select(CreateTransportStream));

        var metadata = context.GetValue<PeerExchangeMetadata>(PeerExchangeMetadata.Key) ?? new PeerExchangeMetadata();
        if (DateTime.UtcNow.Subtract(metadata.LastMessageDate) >= TimeSpan.FromMinutes(1))
        {
            var currentConnectedPeers = context.Peers.Select(p => p.Address);

            var addedPeers = currentConnectedPeers.Where(p => !metadata.ConnectedPeersSnapshot.Contains(p));
            var droppedPeers = metadata.ConnectedPeersSnapshot.Where(p => !currentConnectedPeers.Contains(p));

            if (!droppedPeers.Any() && !addedPeers.Any())
                return;

            message = new PeerExchangeMessage
            {
                Added = ParseEndpoints(addedPeers),
                Dropped = ParseEndpoints(droppedPeers),
            };
            context.SendMessage(message);

            metadata.LastMessageDate = DateTime.UtcNow;
            metadata.ConnectedPeersSnapshot = currentConnectedPeers;
            context.SetValue(PeerExchangeMetadata.Key, metadata);
        }
    }

    private ITransportStream CreateTransportStream(IPEndPoint endPoint)
    {
        return _tcpTransportProtocol.CreateTransportStream(endPoint.Address, endPoint.Port);
    }

    private IList<IPEndPoint> ParseEndpoints(IEnumerable<string> addresses)
    {
        var result = new List<IPEndPoint>();
        foreach (var address in addresses)
        {
            if (TryParseEndpoint(address, out var endPoint))
            {
                result.Add(endPoint);
            }
        }

        return result;
    }

    private bool TryParseEndpoint(string value, [NotNullWhen(true)] out IPEndPoint? result)
    {
        // TODO: IPV6 support
        var addressChunks = value.Split(':');
        if (addressChunks.Length == 2)
        {
            var address = IPAddress.Parse(addressChunks[0]);
            var port = int.Parse(addressChunks[1]);

            result = new IPEndPoint(address, port);
            return true;
        }

        result = null;
        return false;
    }
}
