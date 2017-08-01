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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TorrentCore.Application;
using TorrentCore.Data;
using TorrentCore.Engine;

namespace TorrentCore.Transport
{
    class TcpTransportProtocol : TcpBasedTransportProtocol
    {
        private static readonly ILogger Log = LogManager.GetLogger<TcpTransportProtocol>();

        public TcpTransportProtocol(IMessageHandler messageHandler,
                                    IMainLoop mainLoop,
                                    int port,
                                    IPAddress localAddress,
                                    PeerId localPeerId,
                                    Action<AcceptConnectionEventArgs> acceptConnectionHandler)
            : base(messageHandler, mainLoop, port, localAddress, localPeerId)
        {
            PeerStreams = new ConcurrentBag<PeerStream>();
            AcceptConnectionHandler = acceptConnectionHandler;
        }

        /// <summary>
        /// Gets the list of streams.
        /// </summary>
        public ConcurrentBag<PeerStream> PeerStreams { get; }

        /// <summary>
        /// Gets a collection of the active transport streams.
        /// </summary>
        public override IEnumerable<ITransportStream> Streams => PeerStreams;

        public Action<AcceptConnectionEventArgs> AcceptConnectionHandler { get; }

        /// <summary>
        /// Handles new incoming connection requests.
        /// </summary>
        /// <param name="e">Event args for handling the request.</param>
        public override void AcceptConnection(TransportConnectionEventArgs e)
        {
            var stream = new PeerStream(this, e.Client);
            if (!stream.ReceiveConnectionHeader())
            {
                Debug.Write("Connection header mismatch.");
            }

            // Notify application protocol
            bool accepted = false;
            var applicationEE = new AcceptConnectionEventArgs(stream, () =>
                                                              {
                                                                  Log.LogInformation($"Accepted connection from {stream.Address}");

                                                                  accepted = true;
                                                                  stream.SendConnectionHeader();
                                                                  PeerStreams.Add(stream);
                                                                  stream.ReceiveData();
                                                              });
            AcceptConnectionHandler(applicationEE);

            if (!accepted)
                e.Client.Dispose();
        }

        public override ITransportStream CreateTransportStream(IPAddress address, int port, Sha1Hash infoHash)
        {
            return new PeerStream(this, infoHash, address, port);
        }

        public override void Stop()
        {
            // Stop listening for new connections
            base.Stop();

            foreach (var stream in PeerStreams)
                stream.Dispose();
        }
    }
}
