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
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Data;
using TorrentCore.Engine;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent
{
    public class PeerConnection
    {
        private readonly IMessageHandler messageHandler;
        private readonly BigEndianBinaryReader reader;
        private readonly BigEndianBinaryWriter writer;
        private readonly ITransportStream transportStream;

        internal PeerConnection(Metainfo meta,
                                PeerId peerId,
                                ProtocolExtension supportedExtensions,
                                IMessageHandler messageHandler,
                                ITransportStream transportStream)
        {
            this.messageHandler = messageHandler;
            this.transportStream = transportStream;
            PeerId = peerId;
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

            ReceiveData();
        }

        /// <summary>
        /// Gets the ID for this peer.
        /// </summary>
        public PeerId PeerId { get; }

        /// <summary>
        /// Gets the address of this peer.
        /// </summary>
        public string Address => transportStream.Address;

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

        public void Send(byte[] data)
        {
            writer.Write(data.Length);
            writer.Write(data);
            writer.Flush();
        }

        private void ReceiveData()
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
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
