// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2016 Sam Fisher.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Data;

namespace TorrentCore.Transport
{
    class PeerStream : TcpBasedTransportStream, ITransportStream
    {
        private const string BitTorrentProtocol = "BitTorrent protocol";
        private const int BitTorrentProtocolReservedBytes = 8;

        /// <summary>
        /// Gets the DirectTransportProtocol that controls this stream.
        /// </summary>
        public new TcpTransportProtocol TransportProtocol => (TcpTransportProtocol)base.TransportProtocol;

        /// <summary>
        /// Gets the info hash used by the stream.
        /// </summary>
        public Sha1Hash InfoHash { get; private set; }

        /// <summary>
        /// Gets an address that uniquely identifies the peer this stream connects to.
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// Creates a new PeerStream which can later connect to the remote peer at the specified address and port.
        /// </summary>
        /// <param name="owner">The DirectTransportProtocol that controls the stream.</param>
        /// <param name="infoHash">Info hash used by the stream.</param>
        /// <param name="address">IP address of remote peer.</param>
        /// <param name="port">Port of remote peer.</param>
        public PeerStream(TcpTransportProtocol owner, Sha1Hash infoHash, IPAddress address, int port)
            : base(owner, address, port)
        {
            Address = $"{address}:{port}";
            InfoHash = infoHash;
            owner.PeerStreams.Add(this);
        }

        /// <summary>
        /// Creates a new PeerStream from the existing TcpClient.
        /// </summary>
        /// <param name="owner">The DirectTransportProtocol that controls the stream.</param>
        /// <param name="client">Existing connection.</param>
        public PeerStream(TcpTransportProtocol owner, TcpClient client)
            : base(owner, client)
        {
            Address = client.Client.RemoteEndPoint.ToString();
            owner.PeerStreams.Add(this);
        }

        /// <summary>
        /// Sends the specified block of data.
        /// </summary>
        /// <param name="data">Data to send.</param>
        public void SendData(byte[] data)
        {
            TransportProtocol.MainLoop.AddTask(() =>
                                               {
                                                   if (!IsConnected)
                                                       return;

                                                   Writer.Write(data.Length);
                                                   Writer.Write(data);
                                                   Writer.Flush();
                                               });
        }

        public override void ReceiveData()
        {
            Task.Factory.StartNew(() =>
                                  {
                                      try
                                      {
                                          while (true)
                                          {
                                              // Read message length
                                              int length = Reader.ReadInt32();

                                              // Read data
                                              byte[] data = Reader.ReadBytes(length);

                                              TransportProtocol.MessageReceived(this, data);
                                          }
                                      }
                                      catch (IOException)
                                      {
                                          // Disconnected
                                      }
                                  }, TaskCreationOptions.LongRunning);
        }

        public override void SendConnectionHeader()
        {
            // Length of protocol string
            Writer.Write((byte)BitTorrentProtocol.Length);

            // Protocol
            Writer.Write(BitTorrentProtocol.ToCharArray());

            // Reserved bytes
            Writer.Write(new byte[BitTorrentProtocolReservedBytes]);

            // Info hash
            Writer.Write(InfoHash);

            // Peer ID
            Writer.Write(new byte[20]);

            Writer.Flush();
        }

        public override bool ReceiveConnectionHeader()
        {
            // Length of protocol string
            byte protocolStringLength = Reader.ReadByte();

            // Protocol
            string protocol = new string(Reader.ReadChars(protocolStringLength));

            // Reserved bytes
            Reader.ReadBytes(8);

            // Info hash
            var infoHash = new Sha1Hash(Reader.ReadBytes(20));

            // Peer ID
            byte[] peerID = Reader.ReadBytes(20);

            // Check info hash matches
            if (InfoHash == null)
                InfoHash = infoHash;
            else if (infoHash != InfoHash)
                return false;

            IsHeaderReceived = true;

            return true;
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
