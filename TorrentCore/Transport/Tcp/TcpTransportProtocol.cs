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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TorrentCore.Transport.Tcp
{
    /// <summary>
    /// Base class for transport protocols using TCP.
    /// Provides a TCP listener for incoming connections.
    /// </summary>
    class TcpTransportProtocol : ITransportProtocol
    {
        private static readonly ILogger Log = LogManager.GetLogger<TcpTransportProtocol>();

        private readonly bool bindToNextAvailablePort;
        private readonly ConcurrentBag<TcpTransportStream> streams;

        private TcpListener listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpTransportProtocol"/> class that will listen on the specified port.
        /// </summary>
        /// <param name="port">Port to listen on for incoming connections.</param>
        /// <param name="bindToNextAvailablePort">If the specified port is in use, attempts to bind to the next available port.</param>
        /// <param name="localBindAddress">The local address to use for connections.</param>
        /// <param name="acceptConnectionHandler">Invoked to accept the connection.</param>
        public TcpTransportProtocol(
            int port,
            bool bindToNextAvailablePort,
            IPAddress localBindAddress,
            Action<AcceptConnectionEventArgs> acceptConnectionHandler)
        {
            streams = new ConcurrentBag<TcpTransportStream>();
            AcceptConnectionHandler = acceptConnectionHandler;
            this.bindToNextAvailablePort = bindToNextAvailablePort;
            Port = port;
            LocalBindAddress = localBindAddress;
            LocalConection = new LocalTcpConnectionDetails(port, null, localBindAddress);
            RateLimiter = new RateLimiter();
        }

        public Action<AcceptConnectionEventArgs> AcceptConnectionHandler { get; }

        /// <summary>
        /// Gets or sets the maximum upload and download rates for all streams using this transport protocol.
        /// </summary>
        public RateLimiter RateLimiter { get; set; }

        public LocalTcpConnectionDetails LocalConection { get; private set; }

        /// <summary>
        /// Gets the port on which incoming connections can be made.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the public address that is used to listen for incoming connections.
        /// </summary>
        public IPAddress PublicListenAddress { get; }

        /// <summary>
        /// Gets the address of the local adapter used for connections.
        /// </summary>
        public IPAddress LocalBindAddress { get; }

        /// <summary>
        /// Gets a collection of the active transport streams.
        /// </summary>
        public IEnumerable<TcpTransportStream> Streams => streams;

        /// <summary>
        /// Gets a collection of the active transport streams.
        /// </summary>
        IEnumerable<ITransportStream> ITransportProtocol.Streams => streams;
        
        void AcceptConnection(TransportConnectionEventArgs e)
        {
            var stream = new TcpTransportStream(e.Client);

            // Notify application protocol
            bool accepted = false;
            var applicationEE = new AcceptConnectionEventArgs(stream, () =>
            {
                Log.LogInformation($"Accepted connection from {stream.RemoteEndPoint}");

                accepted = true;
                streams.Add(stream);
            });
            AcceptConnectionHandler(applicationEE);

            if (!accepted)
                e.Client.Dispose();
        }

        /// <summary>
        /// Starts the transport protocol.
        /// </summary>
        public void Start()
        {
            int port = Port;
            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    listener = new TcpListener(LocalBindAddress, port + attempt);
                    listener.Start();
                }
                catch (SocketException ex)
                    when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse && bindToNextAvailablePort)
                {
                    // Try next available port
                    continue;
                }

                break;
            }

            // If port=0 was supplied, set the actual port we are listening on.
            Port = ((IPEndPoint)listener.LocalEndpoint).Port;
            LocalConection = new LocalTcpConnectionDetails(Port, null, LocalBindAddress);
            ListenForIncomingConnections();
        }

        /// <summary>
        /// Stops the transport protocol.
        /// </summary>
        public void Stop()
        {
            // Stop listening for new connections
            listener.Stop();

            foreach (var stream in streams)
                stream.Disconnect();
        }

        void ListenForIncomingConnections()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        var client = await listener.AcceptTcpClientAsync();
                        AcceptConnection(new TransportConnectionEventArgs(client));
                    }
                }
                catch
                {
                    // Socket closed
                }
            });
        }
    }
}
