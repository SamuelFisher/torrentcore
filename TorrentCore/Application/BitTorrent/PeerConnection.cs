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
    public class PeerConnection
    {
        private readonly IPeerMessageHandler messageHandler;
        private readonly BigEndianBinaryReader reader;
        private readonly BigEndianBinaryWriter writer;
        private readonly ITransportStream transportStream;
        private readonly Dictionary<IModule, Dictionary<string, object>> customValues;

        internal PeerConnection(
            Metainfo meta,
            PeerId peerId,
            IReadOnlyList<byte> reservedBytes,
            ProtocolExtension supportedExtensions,
            IPeerMessageHandler messageHandler,
            ITransportStream transportStream)
        {
            this.messageHandler = messageHandler;
            this.transportStream = transportStream;
            customValues = new Dictionary<IModule, Dictionary<string, object>>();
            PeerId = peerId;
            ReservedBytes = reservedBytes;
            SupportedExtensions = supportedExtensions;
            InfoHash = meta.InfoHash;
            reader = new BigEndianBinaryReader(transportStream.Stream);
            writer = new BigEndianBinaryWriter(transportStream.Stream);
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
        public string Address => transportStream.DisplayAddress;

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
            writer.Write(data.Length);
            writer.Write(data);
            writer.Flush();
        }

        public void Disconnect()
        {
            transportStream.Disconnect();
            messageHandler.PeerDisconnected(this);
        }

        internal Dictionary<string, object> GetCustomValues(IModule module)
        {
            if (!customValues.TryGetValue(module, out Dictionary<string, object> moduleValues))
            {
                moduleValues = new Dictionary<string, object>();
                customValues.Add(module, moduleValues);
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
                        int length = reader.ReadInt32();

                        // Read data
                        byte[] data = reader.ReadBytes(length);

                        messageHandler.MessageReceived(this, data);
                    }
                }
                catch (IOException)
                {
                    // Disconnected
                    messageHandler.PeerDisconnected(this);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
