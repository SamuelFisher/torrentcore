// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Data;
using TorrentCore.Engine;
using TorrentCore.Modularity;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent.Connection;

/// <summary>
/// Initiates new peer connections using the <see cref="BitTorrentApplicationProtocol"/>.
/// </summary>
class BitTorrentPeerInitiator : IApplicationProtocolPeerInitiator
{
    private const string BitTorrentProtocol = "BitTorrent protocol";
    private const int BitTorrentProtocolReservedBytes = 8;

    private readonly Dictionary<Sha1Hash, IApplicationProtocol> _applicationProtocolLookup;
    private readonly PeerId _localPeerId;
    private readonly IMainLoop _mainLoop;
    private readonly IReadOnlyCollection<IModule> _modules;

    public BitTorrentPeerInitiator(PeerId localPeerId, IMainLoop mainLoop, IEnumerable<IModule> modules)
    {
        _applicationProtocolLookup = new Dictionary<Sha1Hash, IApplicationProtocol>();
        _localPeerId = localPeerId;
        _mainLoop = mainLoop;
        _modules = modules.ToList().AsReadOnly();
    }

    public void AcceptIncomingConnection(AcceptConnectionEventArgs e)
    {
        var reader = new BigEndianBinaryReader(e.TransportStream.Stream);
        var header = ReadConnectionHeader(reader);

        // TODO: check if exists
        var applicationProtocol = _applicationProtocolLookup[header.InfoHash];

        applicationProtocol.AcceptConnection(new AcceptPeerConnectionEventArgs(e.TransportStream, () =>
        {
            e.Accept();

            var writer = new BigEndianBinaryWriter(e.TransportStream.Stream);
            WriteConnectionHeader(writer, applicationProtocol.Metainfo.InfoHash, _localPeerId);

            return new BitTorrentPeer(
                applicationProtocol.Metainfo,
                header.PeerId,
                header.ReservedBytes,
                header.SupportedExtensions,
                new QueueingMessageHandler(_mainLoop, (BitTorrentApplicationProtocol)applicationProtocol),
                e.TransportStream);
        }));
    }

    public IPeer InitiateOutgoingConnection(ITransportStream transportStream, IApplicationProtocol applicationProtocol)
    {
        var writer = new BigEndianBinaryWriter(transportStream.Stream);
        var reader = new BigEndianBinaryReader(transportStream.Stream);
        WriteConnectionHeader(writer, applicationProtocol.Metainfo.InfoHash, _localPeerId);
        var header = ReadConnectionHeader(reader);

        if (header.InfoHash != applicationProtocol.Metainfo.InfoHash)
        {
            // Infohash mismatch
            throw new NotImplementedException();
        }

        return new BitTorrentPeer(
            applicationProtocol.Metainfo,
            header.PeerId,
            header.ReservedBytes,
            header.SupportedExtensions,
            new QueueingMessageHandler(_mainLoop, (BitTorrentApplicationProtocol)applicationProtocol),
            transportStream);
    }

    private void WriteConnectionHeader(
        BinaryWriter writer,
        Sha1Hash infoHash,
        PeerId localPeerId)
    {
        // Length of protocol string
        writer.Write((byte)BitTorrentProtocol.Length);

        // Protocol
        writer.Write(BitTorrentProtocol.ToCharArray());

        // Reserved bytes
        var reservedBytes = new byte[BitTorrentProtocolReservedBytes];
        var prepareHandshakeContext = new PrepareHandshakeContext(reservedBytes);
        foreach (var module in _modules)
            module.OnPrepareHandshake(prepareHandshakeContext);
        writer.Write(prepareHandshakeContext.ReservedBytes);

        // Info hash
        writer.Write(infoHash.Value);

        // Peer ID
        writer.Write(localPeerId.Value.ToArray());

        writer.Flush();
    }

    private ConnectionHeader ReadConnectionHeader(BinaryReader reader)
    {
        // Length of protocol string
        byte protocolStringLength = reader.ReadByte();

        // Protocol
        string protocol = new string(reader.ReadChars(protocolStringLength));

        // Reserved bytes
        var reservedBytes = reader.ReadBytes(8);
        var supportedExtensions = ProtocolExtensions.DetermineSupportedProcotolExtensions(reservedBytes);

        // Info hash
        var infoHash = new Sha1Hash(reader.ReadBytes(20));

        // Peer ID
        var peerId = new PeerId(reader.ReadBytes(20));

        return new ConnectionHeader(infoHash, peerId, supportedExtensions, reservedBytes);
    }

    public void OnApplicationProtocolAdded(IApplicationProtocol instance)
    {
        _applicationProtocolLookup.Add(instance.Metainfo.InfoHash, instance);
    }

    public void OnApplicationProtocolRemoved(IApplicationProtocol instance)
    {
        _applicationProtocolLookup.Remove(instance.Metainfo.InfoHash);
    }

    private record ConnectionHeader(Sha1Hash InfoHash, PeerId PeerId, ProtocolExtension SupportedExtensions, byte[] ReservedBytes);

    private class PrepareHandshakeContext : IPrepareHandshakeContext
    {
        public PrepareHandshakeContext(byte[] reservedBytes)
        {
            ReservedBytes = reservedBytes;
        }

        public byte[] ReservedBytes { get; }
    }
}
