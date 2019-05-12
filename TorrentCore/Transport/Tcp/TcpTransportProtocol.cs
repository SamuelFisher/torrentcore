// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

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
using Microsoft.Extensions.Options;

namespace TorrentCore.Transport.Tcp
{
    /// <summary>
    /// Base class for transport protocols using TCP.
    /// Provides a TCP listener for incoming connections.
    /// </summary>
    class TcpTransportProtocol : ITransportProtocol
    {
        private static readonly ILogger Log = LogManager.GetLogger<TcpTransportProtocol>();

        private readonly ConcurrentBag<TcpTransportStream> _streams;

        private TcpListener _listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpTransportProtocol"/> class that will listen on the specified port.
        /// </summary>
        /// <param name="options">Listen options.</param>
        public TcpTransportProtocol(IOptions<LocalTcpConnectionOptions> options)
        {
            _streams = new ConcurrentBag<TcpTransportStream>();
            Port = options.Value.Port;
            LocalBindAddress = options.Value.BindAddress ?? IPAddress.Any;
            RateLimiter = new RateLimiter();
        }

        public event Action<AcceptConnectionEventArgs> AcceptConnectionHandler;

        /// <summary>
        /// Gets or sets the maximum upload and download rates for all streams using this transport protocol.
        /// </summary>
        public RateLimiter RateLimiter { get; set; }

        public LocalTcpConnectionOptions LocalConection { get; private set; }

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
        public IEnumerable<TcpTransportStream> Streams => _streams;

        /// <summary>
        /// Gets a collection of the active transport streams.
        /// </summary>
        IEnumerable<ITransportStream> ITransportProtocol.Streams => _streams;

        void AcceptConnection(TransportConnectionEventArgs e)
        {
            var stream = new TcpTransportStream(e.Client);

            // Notify application protocol
            bool accepted = false;
            var applicationEE = new AcceptConnectionEventArgs(stream, () =>
            {
                Log.LogInformation($"Accepted connection from {stream.RemoteEndPoint}");

                accepted = true;
                _streams.Add(stream);
            });
            AcceptConnectionHandler?.Invoke(applicationEE);

            if (!accepted)
                e.Client.Dispose();
        }

        /// <summary>
        /// Starts the transport protocol.
        /// </summary>
        public void Start()
        {
            _listener = new TcpListener(LocalBindAddress, Port);
            _listener.Start();

            // If port=0 was supplied, set the actual port we are listening on.
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            ListenForIncomingConnections();
        }

        /// <summary>
        /// Stops the transport protocol.
        /// </summary>
        public void Stop()
        {
            // Stop listening for new connections
            _listener.Stop();

            foreach (var stream in _streams)
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
                        var client = await _listener.AcceptTcpClientAsync();
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
