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
using System.IO;
using System.Linq;
using System.Text;
using TorrentCore.Data;
using TorrentCore.ExtensionModule;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent.Connection
{
    class BitTorrentPeerInitiator : IApplicationProtocolPeerInitiator<PeerConnection, BitTorrentPeerInitiator.IContext, PeerConnectionArgs>
    {
        private const string BitTorrentProtocol = "BitTorrent protocol";
        private const int BitTorrentProtocolReservedBytes = 8;

        private readonly Func<Sha1Hash, BitTorrentApplicationProtocol<IContext>> applicationProtocolLookup;
        private readonly IModuleManager modules;

        public BitTorrentPeerInitiator(Func<Sha1Hash, BitTorrentApplicationProtocol<IContext>> applicationProtocolLookup,
                                       IModuleManager modules)
        {
            this.applicationProtocolLookup = applicationProtocolLookup;
            this.modules = modules;
        }

        public BitTorrentApplicationProtocol<IContext> PrepareAcceptIncomingConnection(ITransportStream transportStream, out IContext context)
        {
            var reader = new BigEndianBinaryReader(transportStream.Stream);
            var header = ReadConnectionHeader(reader);
            context = new PeerConnectionPreparationContext(header.PeerId, header.ReservedBytes, header.SupportedExtensions);
            return applicationProtocolLookup(header.InfoHash);
        }

        IApplicationProtocol<PeerConnection> IApplicationProtocolPeerInitiator<PeerConnection, IContext, PeerConnectionArgs>.PrepareAcceptIncomingConnection(
            ITransportStream transportStream,
            out IContext context)
        {
            return PrepareAcceptIncomingConnection(transportStream, out context);
        }

        public PeerConnection AcceptIncomingConnection(ITransportStream transportStream,
                                                       IContext context,
                                                       PeerConnectionArgs c)
        {
            var writer = new BigEndianBinaryWriter(transportStream.Stream);
            WriteConnectionHeader(writer, c.Metainfo.InfoHash, c.LocalPeerId);
            return new PeerConnection(c.Metainfo,
                                      context.PeerId,
                                      context.ReservedBytes,
                                      context.SupportedExtensions,
                                      c.MessageHandler,
                                      transportStream);
        }

        public PeerConnection InitiateOutgoingConnection(ITransportStream transportStream,
                                                         PeerConnectionArgs c)
        {
            var writer = new BigEndianBinaryWriter(transportStream.Stream);
            var reader = new BigEndianBinaryReader(transportStream.Stream);
            WriteConnectionHeader(writer, c.Metainfo.InfoHash, c.LocalPeerId);
            var header = ReadConnectionHeader(reader);

            if (header.InfoHash != c.Metainfo.InfoHash)
            {
                // Infohash mismatch
                throw new NotImplementedException();
            }

            return new PeerConnection(c.Metainfo,
                                      header.PeerId,
                                      header.ReservedBytes,
                                      header.SupportedExtensions,
                                      c.MessageHandler,
                                      transportStream);
        }

        private void WriteConnectionHeader(BinaryWriter writer,
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
            foreach(var module in modules.Modules)
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
            var result = new ConnectionHeader();

            // Length of protocol string
            byte protocolStringLength = reader.ReadByte();

            // Protocol
            string protocol = new string(reader.ReadChars(protocolStringLength));

            // Reserved bytes
            result.ReservedBytes = reader.ReadBytes(8);
            result.SupportedExtensions = ProtocolExtensions.DetermineSupportedProcotolExtensions(result.ReservedBytes);

            // Info hash
            result.InfoHash = new Sha1Hash(reader.ReadBytes(20));

            // Peer ID
            result.PeerId = new PeerId(reader.ReadBytes(20));
            
            return result;
        }

        private class ConnectionHeader
        {
            public Sha1Hash InfoHash { get; set; }
            public PeerId PeerId { get; set; }
            public ProtocolExtension SupportedExtensions { get; set; }
            public byte[] ReservedBytes { get; set; }
        }

        private class PeerConnectionPreparationContext : IContext
        {
            internal PeerConnectionPreparationContext(PeerId peerId,
                                                      byte[] reservedBytes,
                                                      ProtocolExtension supportExtensions)
            {
                PeerId = peerId;
                ReservedBytes = reservedBytes;
                SupportedExtensions = supportExtensions;
            }

            public PeerId PeerId { get; }

            public byte[] ReservedBytes { get; }

            public ProtocolExtension SupportedExtensions { get; }
        }

        public interface IContext
        {
            PeerId PeerId { get; }
            byte[] ReservedBytes { get; }
            ProtocolExtension SupportedExtensions { get; }
        }

        private class PrepareHandshakeContext : IPrepareHandshakeContext
        {
            public PrepareHandshakeContext(byte[] reservedBytes)
            {
                ReservedBytes = reservedBytes;
            }

            public byte[] ReservedBytes { get; }
        }
    }
}
