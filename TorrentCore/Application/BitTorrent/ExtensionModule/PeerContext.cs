// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Data;
using TorrentCore.Data.Pieces;
using TorrentCore.Modularity;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent.ExtensionModule;

internal partial class PeerContext : IPeerContext
{
    private readonly ITorrentContext _torrentContext;
    private readonly Dictionary<string, object?> _customValues;
    private readonly Action<byte> _registerMessageHandler;

    public PeerContext(
        BitTorrentPeer peer,
        Dictionary<string, object?> customValues,
        ITorrentContext torrentContext,
        Action<byte> registerMessageHandler)
    {
        Peer = peer;
        _customValues = customValues;
        _registerMessageHandler = registerMessageHandler;
        _torrentContext = torrentContext;
    }

    public BitTorrentPeer Peer { get; }

    public IPieceDataHandler DataHandler => _torrentContext.DataHandler;

    public IBlockRequests BlockRequests => _torrentContext.BlockRequests;

    public T? GetValue<T>(string key)
    {
        if (!_customValues.TryGetValue(key, out var value))
            return default;

        return (T?)value;
    }

    public void SetValue<T>(string key, T value)
    {
        _customValues[key] = value;
    }

    public void RegisterMessageHandler(byte messageId)
    {
        _registerMessageHandler(messageId);
    }

    public void SendMessage(byte messageId, byte[] data)
    {
        using (var ms = new MemoryStream())
        {
            BinaryWriter writer = new BigEndianBinaryWriter(ms);
            writer.Write(messageId);
            writer.Write(data);
            writer.Flush();
            Peer.Send(ms.ToArray());
        }
    }
}

internal partial class PeerContext : ITorrentContext
{
    public Metainfo Metainfo => _torrentContext.Metainfo;

    public IReadOnlyCollection<BitTorrentPeer> Peers => _torrentContext.Peers;

    public void PeersAvailable(IEnumerable<ITransportStream> peers)
    {
        _torrentContext.PeersAvailable(peers);
    }
}
