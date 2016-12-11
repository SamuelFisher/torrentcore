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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TorrentCore.Transport
{
    abstract class TcpBasedTransportStream
    {
        private readonly ManualResetEvent connectionEvent = new ManualResetEvent(false);
        private readonly TcpClient client;

        /// <summary>
        /// Creates a new TcpTransportStream which can later connect to the remote peer at the specified address and port.
        /// </summary>
        /// <param name="owner">The TcpTransportProtocol that controls the stream.</param>
        /// <param name="address">IP address of remote peer.</param>
        /// <param name="port">Port of remote peer.</param>
        protected TcpBasedTransportStream(TcpBasedTransportProtocol owner, IPAddress address, int port)
        {
            TransportProtocol = owner;
            client = new TcpClient(address.AddressFamily);

            // Use the adapter for the IPAddress specified
            client.Client.Bind(new IPEndPoint(owner.LocalAddress, 0));

            Address = address;
            Port = port;
        }

        /// <summary>
        /// Creates a new TcpTransportStream from the existing TcpClient.
        /// </summary>
        /// <param name="owner">The TcpTransportProtocol that controls the stream.</param>
        /// <param name="client">Existing connection.</param>
        protected TcpBasedTransportStream(TcpBasedTransportProtocol owner, TcpClient client)
        {
            TransportProtocol = owner;
            this.client = client;
            Stream = new RateLimitedStream(client.GetStream(), TransportProtocol.RateLimiter);
            Reader = new BigEndianBinaryReader(Stream);
            Writer = new BigEndianBinaryWriter(Stream);
        }

        protected RateLimitedStream Stream { get; private set; }
        protected BinaryReader Reader { get; private set; }
        protected BinaryWriter Writer { get; private set; }
        protected IPAddress Address { get; }
        protected int Port { get; }

        protected bool IsHeaderReceived { get; set; }

        /// <summary>
        /// Gets the <see cref="TcpBasedTransportProtocol"/> instance that controls this stream.
        /// </summary>
        public TcpBasedTransportProtocol TransportProtocol { get; }

        /// <summary>
        /// Gets a value indicating whether this connection is active.
        /// </summary>
        public bool IsConnected => IsHeaderReceived && client.Connected;

        /// <summary>
        /// Gets a value indicating whether a connection attempt is in progress for this stream.
        /// </summary>
        public bool IsConnecting { get; private set; }

        /// <summary>
        /// Attempts to initiate this connection.
        /// </summary>
        /// <returns>Task which completes when the connection is made.</returns>
        public async Task Connect()
        {
            if (IsConnected)
                throw new InvalidOperationException("Already connected.");

            if (Address == null)
                throw new InvalidOperationException("Address and port have not been specified.");

            if (IsConnecting)
            {
                connectionEvent.WaitOne();
                return;
            }

            IsConnecting = true;

            try
            {
                await client.ConnectAsync(Address, Port);
            }
            finally
            {
                IsConnecting = false;
                connectionEvent.Set();
            }

            Stream = new RateLimitedStream(client.GetStream(), TransportProtocol.RateLimiter);
            Reader = new BigEndianBinaryReader(Stream);
            Writer = new BigEndianBinaryWriter(Stream);

            SendConnectionHeader();

            bool success = ReceiveConnectionHeader();

            if (!success)
            {
                throw new IOException("Connection header mismatch.");
            }
            else
            {
                ReceiveData();
            }

            IsConnecting = false;
            connectionEvent.Set();
        }

        public void Disconnect()
        {
            client.Dispose();
        }

        public abstract void SendConnectionHeader();

        public abstract bool ReceiveConnectionHeader();

        public abstract void ReceiveData();
    }
}
