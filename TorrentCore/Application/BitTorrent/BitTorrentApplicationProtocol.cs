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

        private readonly object _peersLock = new object();
        private readonly PeerId _localPeerId;
        private readonly IApplicationProtocolPeerInitiator<PeerConnection, TConnectionContext, PeerConnectionArgs> _peerInitiator;
        private readonly Func<IPeerMessageHandler, IPeerMessageHandler> _messageHandlerFactory;
        private readonly IModuleManager _modules;
        private readonly HashSet<PeerConnection> _peers = new HashSet<PeerConnection>();
        private readonly List<ITransportStream> _connectingPeers = new List<ITransportStream>();
        private readonly HashSet<ITransportStream> _availablePeers = new HashSet<ITransportStream>(new TransportStreamAddressEqualityComparer());
        private readonly Dictionary<Tuple<PeerConnection, byte>, IModule> _messageHandlerRegistrations = new Dictionary<Tuple<PeerConnection, byte>, IModule>();
        private readonly BlockRequestManager _blockRequests;

        public BitTorrentApplicationProtocol(
            PeerId localPeerId,
            Metainfo metainfo,
            IApplicationProtocolPeerInitiator<PeerConnection, TConnectionContext, PeerConnectionArgs> peerInitiator,
            Func<IPeerMessageHandler, IPeerMessageHandler> messageHandlerFactory,
            IModuleManager modules,
            IPieceDataHandler dataHandler)
        {
            _localPeerId = localPeerId;
            _peerInitiator = peerInitiator;
            _messageHandlerFactory = messageHandlerFactory;
            _modules = modules;
            DataHandler = dataHandler;
            dataHandler.PieceCompleted += PieceCompleted;
            dataHandler.PieceCorrupted += PieceCorrupted;
            Metainfo = metainfo;
            _blockRequests = new BlockRequestManager();
        }

        public event EventHandler DownloadCompleted;

        public Metainfo Metainfo { get; }

        public IReadOnlyCollection<PeerConnection> Peers => _peers;

        public IReadOnlyCollection<ITransportStream> AvailablePeers => _availablePeers;

        public IReadOnlyCollection<ITransportStream> ConnectingPeers => _connectingPeers;

        public IBlockRequests BlockRequests => _blockRequests;

        public IPieceDataHandler DataHandler { get; }

        public void PeersAvailable(IEnumerable<ITransportStream> newPeers)
        {
            foreach (var peer in newPeers)
            {
                lock (_peersLock)
                {
                    bool added = _availablePeers.Add(peer);
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
            _blockRequests.ClearBlocksForPiece(piece);
            foreach (var peer in _peers)
                peer.SendMessage(new HaveMessage(piece));
            if (!DataHandler.IncompletePieces().Any())
                DownloadCompleted?.Invoke(this, new EventArgs());
        }

        public void PieceCorrupted(Piece piece)
        {
            _blockRequests.ClearBlocksForPiece(piece);
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

            if (_messageHandlerRegistrations.TryGetValue(Tuple.Create(peer, messageId), out IModule module))
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
                lock (_peersLock)
                {
                    _availablePeers.Remove(peerTransport);
                    _connectingPeers.Add(peerTransport);
                }

                peerTransport.Connect().ContinueWith(antecedent =>
                {
                    // TODO: run on main loop thread
                    if (antecedent.Status != TaskStatus.RanToCompletion
                        || !peerTransport.IsConnected)
                    {
                        Log.LogInformation($"Failed to connect to peer at {peerTransport.DisplayAddress}");

                        // Connection failed
                        lock (_peersLock)
                            _connectingPeers.Remove(peerTransport);

                        // TODO: keep a record of failed connection peers
                        return;
                    }

                    var connectionSettings = new PeerConnectionArgs(
                        _localPeerId,
                        Metainfo,
                        _messageHandlerFactory(this));

                    var peer = _peerInitiator.InitiateOutgoingConnection(peerTransport, connectionSettings);
                    lock (_peersLock)
                        _connectingPeers.Remove(peerTransport);
                    PeerConnected(peer);
                });
            }
            catch
            {
                lock (_peersLock)
                {
                    if (_connectingPeers.Contains(peerTransport))
                        _connectingPeers.Remove(peerTransport);
                }
            }
        }

        private void RegisterModuleForMessageId(PeerConnection peer, IModule module, byte messageId)
        {
            lock (_peersLock)
                _messageHandlerRegistrations[Tuple.Create(peer, messageId)] = module;
        }

        private void PeerConnected(PeerConnection peer)
        {
            Log.LogInformation($"Connected to peer at {peer.Address}");

            lock (_peersLock)
                _peers.Add(peer);

            foreach (var module in _modules.Modules)
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
            lock (_peersLock)
            {
                Log.LogInformation($"Disconnected from peer at {peer.Address}");
                _peers.Remove(peer);

                // TODO: optimise this
                foreach (var r in _messageHandlerRegistrations.Where(x => x.Key.Item1 == peer).ToList())
                    _messageHandlerRegistrations.Remove(r.Key);
            }
        }
    }
}
