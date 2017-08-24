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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TorrentCore.Application.BitTorrent.Connection;
using TorrentCore.Application.BitTorrent.ExtensionModule;
using TorrentCore.Application.BitTorrent.Messages;
using TorrentCore.Data;
using TorrentCore.Data.Pieces;
using TorrentCore.Engine;
using TorrentCore.Modularity;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent
{
    /// <summary>
    /// Manages the connections to peers.
    /// </summary>
    /// <remarks>This class is not thread-safe. All methods should be called from the <see cref="MainLoop"/> thread.</remarks>
    /// <typeparam name="TConnectionContext">The type of connection context.</typeparam>
    class BitTorrentApplicationProtocol<TConnectionContext> :
        IApplicationProtocol<PeerConnection>,
        IPeerMessageHandler,
        ITorrentContext
    {
        private static readonly ILogger Log = LogManager.GetLogger<BitTorrentApplicationProtocol<TConnectionContext>>();

        private readonly object peersLock = new object();
        private readonly PeerId localPeerId;
        private readonly IApplicationProtocolPeerInitiator<PeerConnection, TConnectionContext, PeerConnectionArgs> peerInitiator;
        private readonly Func<IPeerMessageHandler, IPeerMessageHandler> messageHandlerFactory;
        private readonly IModuleManager modules;
        private readonly HashSet<PeerConnection> peers = new HashSet<PeerConnection>();
        private readonly List<ITransportStream> connectingPeers = new List<ITransportStream>();
        private readonly HashSet<ITransportStream> availablePeers = new HashSet<ITransportStream>(new TransportStreamAddressEqualityComparer());
        private readonly Dictionary<Tuple<PeerConnection, byte>, IModule> messageHandlerRegistrations = new Dictionary<Tuple<PeerConnection, byte>, IModule>();
        private readonly BlockRequestManager blockRequests;

        public BitTorrentApplicationProtocol(
            PeerId localPeerId,
            Metainfo metainfo,
            IApplicationProtocolPeerInitiator<PeerConnection, TConnectionContext, PeerConnectionArgs> peerInitiator,
            Func<IPeerMessageHandler, IPeerMessageHandler> messageHandlerFactory,
            IModuleManager modules,
            IPieceDataHandler dataHandler)
        {
            this.localPeerId = localPeerId;
            this.peerInitiator = peerInitiator;
            this.messageHandlerFactory = messageHandlerFactory;
            this.modules = modules;
            DataHandler = dataHandler;
            dataHandler.PieceCompleted += PieceCompleted;
            dataHandler.PieceCorrupted += PieceCorrupted;
            Metainfo = metainfo;
            blockRequests = new BlockRequestManager();
        }

        public event EventHandler DownloadCompleted;

        public Metainfo Metainfo { get; }

        public IReadOnlyCollection<PeerConnection> Peers => peers;

        public IReadOnlyCollection<ITransportStream> AvailablePeers => availablePeers;

        public IReadOnlyCollection<ITransportStream> ConnectingPeers => connectingPeers;

        public IBlockRequests BlockRequests => blockRequests;

        public IPieceDataHandler DataHandler { get; }

        public void PeersAvailable(IEnumerable<ITransportStream> newPeers)
        {
            foreach (var peer in newPeers)
            {
                lock (peersLock)
                {
                    bool added = availablePeers.Add(peer);
                    if (!added)
                        Log.LogInformation($"Discarded duplicate peer {peer}");
                }
            }
        }

        /// <summary>
        /// Handles new incoming connection requests.
        /// </summary>
        /// <param name="e">Event args for handling the request.</param>
        public void AcceptConnection(AcceptPeerConnectionEventArgs<PeerConnection> e)
        {
            var peer = e.Accept();
            PeerConnected(peer);
        }

        public void PieceCompleted(Piece piece)
        {
            blockRequests.ClearBlocksForPiece(piece);
            foreach (var peer in peers)
                peer.SendMessage(new HaveMessage(piece));
            if (!DataHandler.IncompletePieces().Any())
                DownloadCompleted?.Invoke(this, new EventArgs());
        }

        public void PieceCorrupted(Piece piece)
        {
            blockRequests.ClearBlocksForPiece(piece);
        }

        /// <summary>
        /// Invoked when a message is received from a connected peer.
        /// </summary>
        /// <param name="peer">Peer that sent the message.</param>
        /// <param name="data">Received message data.</param>
        public void MessageReceived(PeerConnection peer, byte[] data)
        {
            if (data.Length == 0)
                return;

            var reader = new BigEndianBinaryReader(new MemoryStream(data));

            byte messageId = reader.ReadByte();

            if (messageHandlerRegistrations.TryGetValue(Tuple.Create(peer, messageId), out IModule module))
            {
                var customValues = peer.GetCustomValues(module);
                var messageReceivedContext = new MessageReceivedContext(
                    peer,
                    this,
                    messageId,
                    data.Length - 1,
                    reader,
                    customValues,
                    rMessageId => RegisterModuleForMessageId(peer, module, rMessageId));

                module.OnMessageReceived(messageReceivedContext);
                Log.LogTrace($"Message of type {messageId} handled by module {module.GetType().Name}");
            }
            else
            {
                // Unknown message type
                Log.LogWarning($"Received unknown message type {messageId} from {peer.Address}");
                peer.Disconnect();
            }
        }

        public void ConnectToPeer(ITransportStream peerTransport)
        {
            try
            {
                lock (peersLock)
                {
                    availablePeers.Remove(peerTransport);
                    connectingPeers.Add(peerTransport);
                }

                peerTransport.Connect().ContinueWith(antecedent =>
                {
                    // TODO: run on main loop thread
                    if (antecedent.Status != TaskStatus.RanToCompletion
                        || !peerTransport.IsConnected)
                    {
                        Log.LogInformation($"Failed to connect to peer at {peerTransport.DisplayAddress}");

                        // Connection failed
                        lock (peersLock)
                            connectingPeers.Remove(peerTransport);

                        // TODO: keep a record of failed connection peers
                        return;
                    }

                    var connectionSettings = new PeerConnectionArgs(
                        localPeerId,
                        Metainfo,
                        messageHandlerFactory(this));

                    var peer = peerInitiator.InitiateOutgoingConnection(peerTransport, connectionSettings);
                    lock (peersLock)
                        connectingPeers.Remove(peerTransport);
                    PeerConnected(peer);
                });
            }
            catch
            {
                lock (peersLock)
                {
                    if (connectingPeers.Contains(peerTransport))
                        connectingPeers.Remove(peerTransport);
                }
            }
        }

        private void RegisterModuleForMessageId(PeerConnection peer, IModule module, byte messageId)
        {
            lock (peersLock)
                messageHandlerRegistrations[Tuple.Create(peer, messageId)] = module;
        }

        private void PeerConnected(PeerConnection peer)
        {
            Log.LogInformation($"Connected to peer at {peer.Address}");

            lock (peersLock)
                peers.Add(peer);

            foreach (var module in modules.Modules)
            {
                var context = new PeerContext(
                    peer,
                    peer.GetCustomValues(module),
                    this,
                    messageId => RegisterModuleForMessageId(peer, module, messageId));
                module.OnPeerConnected(context);
            }

            peer.ReceiveData();
        }

        public void PeerDisconnected(PeerConnection peer)
        {
            lock (peersLock)
            {
                Log.LogInformation($"Disconnected from peer at {peer.Address}");
                peers.Remove(peer);

                // TODO: optimise this
                foreach (var r in messageHandlerRegistrations.Where(x => x.Key.Item1 == peer).ToList())
                    messageHandlerRegistrations.Remove(r.Key);
            }
        }
    }
}
