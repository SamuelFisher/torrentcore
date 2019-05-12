// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Application.BitTorrent.Messages;
using TorrentCore.Data;
using TorrentCore.Modularity;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent
{
    /// <summary>
    /// Represents a connection to a peer used by the <see cref="BitTorrentApplicationProtocol" />.
    /// </summary>
    public class BitTorrentPeer : IPeer
    {
        private readonly IPeerMessageHandler _messageHandler;
        private readonly BigEndianBinaryReader _reader;
        private readonly BigEndianBinaryWriter _writer;
        private readonly ITransportStream _transportStream;
        private readonly Dictionary<IModule, Dictionary<string, object>> _customValues;

        internal BitTorrentPeer(
            Metainfo meta,
            PeerId peerId,
            IReadOnlyList<byte> reservedBytes,
            ProtocolExtension supportedExtensions,
            IPeerMessageHandler messageHandler,
            ITransportStream transportStream)
        {
            _messageHandler = messageHandler;
            _transportStream = transportStream;
            _customValues = new Dictionary<IModule, Dictionary<string, object>>();
            PeerId = peerId;
            ReservedBytes = reservedBytes;
            SupportedExtensions = supportedExtensions;
            InfoHash = meta.InfoHash;
            _reader = new BigEndianBinaryReader(transportStream.Stream);
            _writer = new BigEndianBinaryWriter(transportStream.Stream);
            Available = new Bitfield(meta.Pieces.Count);
            RequestedByRemotePeer = new HashSet<BlockRequest>();
            Requested = new HashSet<BlockRequest>();

            IsRemotePeerInterested = false;
            IsInterestedInRemotePeer = false;
            IsChokedByRemotePeer = true;
            IsChokingRemotePeer = true;
        }

        /// <summary>
        /// Gets the ID for this peer.
        /// </summary>
        public PeerId PeerId { get; }

        /// <summary>
        /// Gets the reserved bytes sent by this peer in the connection handshake.
        /// </summary>
        public IReadOnlyList<byte> ReservedBytes { get; }

        /// <summary>
        /// Gets the address of this peer.
        /// </summary>
        public string Address => _transportStream.DisplayAddress;

        /// <summary>
        /// Gets the protocol extensions supported by this peer.
        /// </summary>
        public ProtocolExtension SupportedExtensions { get; }

        /// <summary>
        /// Gets the info hash used by the stream.
        /// </summary>
        public Sha1Hash InfoHash { get; }

        public bool IsRemotePeerInterested { get; internal set; }

        public bool IsInterestedInRemotePeer { get; internal set; }

        public bool IsChokedByRemotePeer { get; internal set; }

        public bool IsChokingRemotePeer { get; internal set; }

        public Bitfield Available { get; internal set; }

        public HashSet<BlockRequest> RequestedByRemotePeer { get; }

        public HashSet<BlockRequest> Requested { get; }

        public void SendMessage(IPeerMessage message)
        {
            using (var ms = new MemoryStream())
            {
                BinaryWriter w = new BigEndianBinaryWriter(ms);
                message.Send(w);
                Send(ms.ToArray());
            }
        }

        public void Send(byte[] data)
        {
            _writer.Write(data.Length);
            _writer.Write(data);
            _writer.Flush();
        }

        public void Disconnect()
        {
            _transportStream.Disconnect();
            _messageHandler.PeerDisconnected(this);
        }

        internal Dictionary<string, object> GetCustomValues(IModule module)
        {
            if (!_customValues.TryGetValue(module, out Dictionary<string, object> moduleValues))
            {
                moduleValues = new Dictionary<string, object>();
                _customValues.Add(module, moduleValues);
            }

            return moduleValues;
        }

        internal void ReceiveData()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        // Read message length
                        int length = _reader.ReadInt32();

                        // Read data
                        byte[] data = _reader.ReadBytes(length);

                        _messageHandler.MessageReceived(this, data);
                    }
                }
                catch (IOException)
                {
                    // Disconnected
                    _messageHandler.PeerDisconnected(this);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
