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

using TorrentCore.Application.BitTorrent.Messages;
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
using TorrentCore.Data;
using TorrentCore.Engine;
using TorrentCore.Modularity;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent
{
    /// <summary>
    /// Represents the network protocol for BitTorrent.
    /// </summary>
    /// <remarks>This class is not thread-safe. All methods should be called from the <see cref="MainLoop"/> thread.</remarks>
    class BitTorrentApplicationProtocol<TConnectionContext> : IApplicationProtocol<PeerConnection>, IPeerMessageHandler
    {
        private static readonly ILogger Log = LogManager.GetLogger<BitTorrentApplicationProtocol<TConnectionContext>>();

        private readonly PeerId localPeerId;
        private readonly IApplicationProtocolPeerInitiator<PeerConnection, TConnectionContext, PeerConnectionArgs> peerInitiator;
        private readonly Func<IPeerMessageHandler, IPeerMessageHandler> messageHandlerFactory;
        private readonly IModuleManager modules;
        private readonly IPiecePicker picker;
        private readonly HashSet<PeerConnection> peers = new HashSet<PeerConnection>();
        private readonly List<ITransportStream> connectingPeers = new List<ITransportStream>();
        private readonly List<ITransportStream> availablePeers = new List<ITransportStream>();
        private readonly Dictionary<Tuple<PeerConnection, byte>, IModule> messageHandlerRegistrations = new Dictionary<Tuple<PeerConnection, byte>, IModule>();
        
        /// <summary>
        /// Creates a new instance of the BitTorrent protocol.
        /// </summary>
        public BitTorrentApplicationProtocol(PeerId localPeerId,
                                             ITorrentDownloadManager manager,
                                             IApplicationProtocolPeerInitiator<PeerConnection, TConnectionContext, PeerConnectionArgs> peerInitiator,
                                             Func<IPeerMessageHandler, IPeerMessageHandler> messageHandlerFactory,
                                             IModuleManager modules)
        {
            this.localPeerId = localPeerId;
            this.peerInitiator = peerInitiator;
            this.messageHandlerFactory = messageHandlerFactory;
            this.modules = modules;
            Manager = manager;
            picker = new PiecePicker();
        }

        public ITorrentDownloadManager Manager { get; }

        public IReadOnlyCollection<PeerConnection> Peers => peers;

        /// <summary>
        /// Called when new peers become available to connect to.
        /// </summary>
        /// <param name="streams">Result containing information from the tracker.</param>
        public void PeersAvailable(IEnumerable<ITransportStream> streams)
        {
            // TODO: check for duplicates
            availablePeers.AddRange(streams);
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

        /// <summary>
        /// Performs actions in each cycle of the main loop.
        /// </summary>
        public void Iterate()
        {
            RequestPieces();

            SendPieces();

            ConnectToPeers();
        }

        /// <summary>
        /// Invoked when a piece has been fully downloaded and passes its hash check.
        /// </summary>
        /// <param name="e">Details of the completed piece.</param>
        public void PieceCompleted(PieceCompletedEventArgs e)
        {
            picker.PieceCompleted(e.Piece);

            foreach (var peer in peers)
            {
                SendMessage(peer, new HaveMessage(e.Piece));
            }
        }

        /// <summary>
        /// Invoked when a pieces has been fully downloaded but fails its hash check.
        /// </summary>
        /// <param name="e">Details of the corrupted piece.</param>
        public void PieceCorrupted(PieceCompletedEventArgs e)
        {
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
                var messageReceivedContext = new MessageReceivedContext(peer,
                                                                        messageId,
                                                                        data.Length - 1,
                                                                        reader,
                                                                        customValues,
                                                                        rMessageId => RegisterModuleForMessageId(peer, module, rMessageId),
                                                                        PeersAvailable);

                module.OnMessageReceived(messageReceivedContext);
                Log.LogDebug($"Message of type {messageId} handled by module {module.GetType().Name}");
                return;
            }

            // Read message
            CommonPeerMessage message = MessageHandler.ReadMessage(Manager.Description, reader, data.Length, messageId);
            if (message == null)
            {
                // Something went wrong
                //stream.Dispose();
                throw new NotImplementedException();
            }

            // Process message
            switch (message.ID)
            {
                case ChokeMessage.MessageID:
                    SetChokedByPeer(peer, true);
                    break;
                case UnchokeMessage.MessageID:
                    SetChokedByPeer(peer, false);
                    break;
                case InterestedMessage.MessageID:
                    SetPeerInterested(peer, true);
                    UnchokePeer(peer);
                    break;
                case NotInterestedMessage.MessageID:
                    SetPeerInterested(peer, false);
                    break;
                case HaveMessage.MessageId:
                {
                    HaveMessage haveMessage = message as HaveMessage;
                    SetPeerBitfield(peer, haveMessage.Piece.Index, true);
                    break;
                }
                case BitfieldMessage.MessageId:
                {
                    BitfieldMessage bitfieldMessage = message as BitfieldMessage;
                    SetPeerBitfield(peer, bitfieldMessage.Bitfield);
                    if (IsBitfieldInteresting(bitfieldMessage.Bitfield))
                    {
                        peer.IsInterestedInRemotePeer = true;
                        SendMessage(peer, new InterestedMessage());
                    }
                    break;
                }
                case RequestMessage.MessageID:
                {
                    RequestMessage requestMessage = message as RequestMessage;
                    SetBlockRequestedByPeer(peer, requestMessage.Block);
                    break;
                }
                case CancelMessage.MessageID:
                {
                    CancelMessage cancelMessage = message as CancelMessage;
                    SetBlockCancelledByPeer(peer, cancelMessage.Block);
                    break;
                }
                case PieceMessage.MessageId:
                {
                    PieceMessage pieceMessage = message as PieceMessage;
                    BlockReceived(peer, pieceMessage.Block);
                    break;
                }
            }
        }

        private void RegisterModuleForMessageId(PeerConnection peer, IModule module, byte messageId)
        {
            messageHandlerRegistrations[Tuple.Create(peer, messageId)] = module;
        }

        private void RequestPieces()
        {
            var availability = new Bitfield(Manager.Description.Pieces.Count);
            foreach (var peer in peers)
                availability.Union(peer.Available);

            var blocksToRequest = picker.BlocksToRequest(Manager.IncompletePieces, availability);

            foreach (var block in blocksToRequest)
            {
                var peer = FindPeerWithPiece(Manager.Description.Pieces[block.PieceIndex]);
                if (peer != null)
                {
                    peer.Requested.Add(block);
                    picker.BlockRequested(block);
                    SendMessage(peer, new RequestMessage(block));
                }
            }
        }

        private PeerConnection FindPeerWithPiece(Piece piece)
        {
            foreach (var peer in peers.OrderBy(x => x.Requested.Count))
            {
                if (peer.Available.IsPieceAvailable(piece.Index)
                    && !peer.IsChokedByRemotePeer)
                    return peer;
            }

            // Piece is not available
            return null;
        }

        private void SendPieces()
        {
            foreach (var peer in peers)
            {
                var sent = new List<BlockRequest>();
                foreach (var request in peer.RequestedByRemotePeer)
                {
                    sent.Add(request);
                    SendPiece(peer, request);
                }

                foreach (var request in sent)
                    peer.RequestedByRemotePeer.Remove(request);
            }
        }

        private void ConnectToPeers()
        {
            // If seeding, allow incoming connections only - don't initiate connections
            if (Manager.State != DownloadState.Downloading)
                return;

            if (peers.Count + connectingPeers.Count < 5 && availablePeers.Count > 0)
            {
                var transportStream = availablePeers.First();

                try
                {
                    availablePeers.Remove(transportStream);
                    connectingPeers.Add(transportStream);
                    transportStream.Connect().ContinueWith(antecedent =>
                    {
                        // TODO: run on main loop thread
                        if (antecedent.Status != TaskStatus.RanToCompletion
                            || !transportStream.IsConnected)
                        {
                            Log.LogInformation($"Failed to connect to peer at {transportStream.Address}");

                            // Connection failed
                            connectingPeers.Remove(transportStream);
                            return;
                        }

                        Log.LogInformation($"Connected to peer at {transportStream.Address}");

                        var connectionSettings = new PeerConnectionArgs(localPeerId,
                                                                        Manager.Description,
                                                                        messageHandlerFactory(this));
                        var peer = peerInitiator.InitiateOutgoingConnection(transportStream, connectionSettings);
                        connectingPeers.Remove(transportStream);
                        PeerConnected(peer);
                    });
                }
                catch
                {
                    if (connectingPeers.Contains(transportStream))
                        connectingPeers.Remove(transportStream);
                }
            }
        }

        private void SendPiece(PeerConnection peer, BlockRequest request)
        {
            long dataOffset = Manager.Description.PieceSize * request.PieceIndex + request.Offset;
            byte[] data = Manager.ReadData(dataOffset, request.Length);

            SendMessage(peer, new PieceMessage(request.ToBlock(data)));
        }

        private void SendMessage(PeerConnection peer, IPeerMessage message)
        {
            using (var ms = new MemoryStream())
            {
                BinaryWriter writer = new BigEndianBinaryWriter(ms);
                message.Send(writer);
                peer.Send(ms.ToArray());
            }
        }

        private void BlockReceived(PeerConnection peer, Block block)
        {
            peer.Requested.Remove(block.AsRequest());
            picker.BlockReceived(block);
            long dataOffset = Manager.Description.PieceSize * block.PieceIndex + block.Offset;
            Manager.DataReceived(dataOffset, block.Data);
        }

        private bool IsBitfieldInteresting(Bitfield bitfield)
        {
            var clientBitfield = new Bitfield(Manager.Description.Pieces.Count, Manager.CompletedPieces);
            return Bitfield.NotSubset(bitfield, clientBitfield);
        }

        private void SetBlockCancelledByPeer(PeerConnection peer, BlockRequest blockRequest)
        {
            peer.RequestedByRemotePeer.Remove(blockRequest);
        }

        private void SetBlockRequestedByPeer(PeerConnection peer, BlockRequest blockRequest)
        {
            peer.RequestedByRemotePeer.Add(blockRequest);
        }

        private void SetPeerBitfield(PeerConnection peer, Bitfield bitfield)
        {
            peer.Available = bitfield;
        }

        private void SetPeerBitfield(PeerConnection peer, int pieceIndex, bool available)
        {
            peer.Available.SetPieceAvailable(pieceIndex, available);

            if (!peer.IsInterestedInRemotePeer
                && IsBitfieldInteresting(peer.Available))
            {
                peer.IsInterestedInRemotePeer = true;
                SendMessage(peer, new InterestedMessage());
            }
        }

        private void SetPeerInterested(PeerConnection peer, bool isInterested)
        {
            peer.IsRemotePeerInterested = isInterested;
        }

        private void UnchokePeer(PeerConnection peer)
        {
            peer.IsChokingRemotePeer = false;
            SendMessage(peer, new UnchokeMessage());
        }

        private void SetChokedByPeer(PeerConnection peer, bool choked)
        {
            peer.IsChokedByRemotePeer = choked;
        }
        
        private void PeerConnected(PeerConnection peer)
        {
            peers.Add(peer);

            SendMessage(peer, new BitfieldMessage(new Bitfield(Manager.Description.Pieces.Count, Manager.CompletedPieces)));

            foreach (var module in modules.Modules)
            {
                var context = new PeerContext(peer,
                                              peer.GetCustomValues(module),
                                              messageId => RegisterModuleForMessageId(peer, module, messageId),
                                              PeersAvailable);
                module.OnPeerConnected(context);
            }

            peer.ReceiveData();
        }

        public void PeerDisconnected(PeerConnection peer)
        {
            peers.Remove(peer);
            // TODO: optimise this
            foreach (var r in messageHandlerRegistrations.Where(x => x.Key.Item1 == peer).ToList())
                messageHandlerRegistrations.Remove(r.Key);
        }
    }
}
