// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Application.BitTorrent;
using TorrentCore.Data;
using TorrentCore.Data.Pieces;
using TorrentCore.Modularity;
using TorrentCore.Transport;

namespace TorrentCore.Extensions.ExtensionProtocol;

internal partial class ExtensionProtocolPeerContext : IExtensionProtocolPeerContext
{
    private readonly IPeerContext _peerContext;
    private readonly Action<IExtensionProtocolMessage> _sendMessage;

    public ExtensionProtocolPeerContext(
        IPeerContext peerContext,
        Action<IExtensionProtocolMessage> sendMessage)
    {
        _peerContext = peerContext;
        _sendMessage = sendMessage;
    }

    public IReadOnlyCollection<string> SupportedMessageTypes =>
        _peerContext.GetRequiredValue<Dictionary<string, byte>>(ExtensionProtocolModule.ExtensionProtocolMessageIds).Keys;

    public void SendMessage(IExtensionProtocolMessage message)
    {
        _sendMessage(message);
    }
}

internal partial class ExtensionProtocolPeerContext : IPeerContext
{
    public IBlockRequests BlockRequests => _peerContext.BlockRequests;

    public BitTorrentPeer Peer => _peerContext.Peer;

    public Metainfo Metainfo => _peerContext.Metainfo;

    public IReadOnlyCollection<BitTorrentPeer> Peers => _peerContext.Peers;

    public IPieceDataHandler DataHandler => _peerContext.DataHandler;

    public void PeersAvailable(IEnumerable<ITransportStream> peers)
    {
        _peerContext.PeersAvailable(peers);
    }

    public T? GetValue<T>(string key)
    {
        return _peerContext.GetValue<T>(key);
    }

    public void SetValue<T>(string key, T value)
    {
        _peerContext.SetValue(key, value);
    }

    public void RegisterMessageHandler(byte messageId)
    {
        _peerContext.RegisterMessageHandler(messageId);
    }

    public void SendMessage(byte messageId, byte[] data)
    {
        _peerContext.SendMessage(messageId, data);
    }
}
