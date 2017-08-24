// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TorrentCore.Transport.Tcp
{
    sealed class TcpTransportStream : ITransportStream
    {
        private readonly ManualResetEvent connectionEvent = new ManualResetEvent(false);
        private readonly TcpClient client;

        /// <summary>
        /// Creates a new TcpTransportStream which can later connect to the remote peer at the specified address and port.
        /// </summary>
        /// <param name="adapterAddress">Local IP address of the adapter to bind to.</param>
        /// <param name="remoteAddress">IP address of remote peer.</param>
        /// <param name="port">Port of remote peer.</param>
        public TcpTransportStream(IPAddress adapterAddress, IPAddress remoteAddress, int port)
        {
            client = new TcpClient(adapterAddress.AddressFamily);

            // Use the adapter for the IPAddress specified
            client.Client.Bind(new IPEndPoint(adapterAddress, 0));

            RemoteEndPoint = new IPEndPoint(remoteAddress, port);
        }

        /// <summary>
        /// Creates a new TcpTransportStream from the existing TcpClient.
        /// </summary>
        /// <param name="client">Existing connection.</param>
        public TcpTransportStream(TcpClient client)
        {
            this.client = client;
            Stream = new RateLimitedStream(client.GetStream());
            RemoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
        }

        public Stream Stream { get; private set; }

        public IPEndPoint RemoteEndPoint { get; }

        string ITransportStream.DisplayAddress => RemoteEndPoint.ToString();

        object ITransportStream.Address => RemoteEndPoint;

        /// <summary>
        /// Gets a value indicating whether this connection is active.
        /// </summary>
        public bool IsConnected => client.Connected;

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

            if (RemoteEndPoint == null)
                throw new InvalidOperationException("Address and port have not been specified.");

            if (IsConnecting)
            {
                connectionEvent.WaitOne();
                return;
            }

            IsConnecting = true;

            try
            {
                await client.ConnectAsync(RemoteEndPoint.Address, RemoteEndPoint.Port);
            }
            finally
            {
                IsConnecting = false;
                connectionEvent.Set();
            }

            Stream = new RateLimitedStream(client.GetStream());

            IsConnecting = false;
            connectionEvent.Set();
        }

        public void Disconnect()
        {
            client.Dispose();
        }
    }
}
