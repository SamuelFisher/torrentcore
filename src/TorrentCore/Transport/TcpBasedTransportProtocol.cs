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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Data;
using TorrentCore.Engine;

namespace TorrentCore.Transport
{
    /// <summary>
    /// Base class for transport protocols using TCP.
    /// Provides a TCP listener for incoming connections.
    /// </summary>
    abstract class TcpBasedTransportProtocol : ITransportProtocol
    {
        private readonly TcpListener listener;

        /// <summary>
        /// Creates a new TcpTransportProtocol which will listen on the specified port.
        /// </summary>
        /// <param name="messageHandler">The message handler to use.</param>
        /// <param name="mainLoop">The main loop to use for queuing incoming and outgoing messages.</param>
        /// <param name="port">Port to listen on for incoming connections.</param>
        /// <param name="localAddress">The local address to use for connections.</param>
        protected TcpBasedTransportProtocol(IMessageHandler messageHandler, IMainLoop mainLoop, int port, IPAddress localAddress)
        {
            Port = port;
            LocalAddress = localAddress;
            MessageHandler = messageHandler;
            MainLoop = mainLoop;
            listener = new TcpListener(localAddress, port);
            RateLimiter = new RateLimiter();
        }

        /// <summary>
        /// Gets the MainLoop to which this TcpTransportProtocol belongs.
        /// </summary>
        public IMainLoop MainLoop { get; }

        /// <summary>
        /// Gets or sets the maximum upload and download rates for all streams using this transport protocol.
        /// </summary>
        public RateLimiter RateLimiter { get; set; }

        /// <summary>
        /// Gets or sets the message handler.
        /// </summary>
        public IMessageHandler MessageHandler { get; set; }

        /// <summary>
        /// Gets the port on which incoming connections can be made.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the local address used for connections.
        /// </summary>
        public IPAddress LocalAddress { get; }

        /// <summary>
        /// Gets a collection of the active transport streams.
        /// </summary>
        public abstract IEnumerable<ITransportStream> Streams { get; }

        /// <summary>
        /// Handles new incoming connection requests.
        /// </summary>
        /// <param name="e">Event args for handling the request.</param>
        public abstract void AcceptConnection(TransportConnectionEventArgs e);

        /// <summary>
        /// Starts the transport protocol.
        /// </summary>
        public void Start()
        {
            listener.Start();
            AcceptConnection();
        }

        /// <summary>
        /// Stops the transport protocol.
        /// </summary>
        public virtual void Stop()
        {
            listener.Stop();
        }

        public abstract ITransportStream CreateTransportStream(IPAddress address, int port, Sha1Hash infoHash);

        /// <summary>
        /// Called by ITransportStreams when a message is received.
        /// </summary>
        /// <param name="stream">Stream which received the message.</param>
        /// <param name="data">Message data.</param>
        public void MessageReceived(ITransportStream stream, byte[] data)
        {
            MainLoop.AddTask(() => MessageHandler.MessageReceived(stream, data));
        }

        void AcceptConnection()
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
