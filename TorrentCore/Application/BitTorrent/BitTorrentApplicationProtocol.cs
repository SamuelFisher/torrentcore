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
using TorrentCore.Data;
using TorrentCore.Engine;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent
{
    /// <summary>
    /// Represents the network protocol for BitTorrent.
    /// </summary>
    class BitTorrentApplicationProtocol : IApplicationProtocol
    {
        private static readonly ILogger Log = LogManager.GetLogger<BitTorrentApplicationProtocol>();

        private readonly IPiecePicker picker;
        private readonly ConcurrentDictionary<ITransportStream, PeerInformation> peers = new ConcurrentDictionary<ITransportStream, PeerInformation>();
        private readonly List<ITransportStream> connectingPeers = new List<ITransportStream>();

        /// <summary>
        /// Gets a list of streams available to be initiated.
        /// </summary>
        internal List<ITransportStream> TrackerStreams { get; set; }

        /// <summary>
        /// Creates a new instance of the BitTorrent protocol.
        /// </summary>
        public BitTorrentApplicationProtocol(ITorrentDownloadManager manager)
        {
            Manager = manager;
            picker = new PiecePicker();
            TrackerStreams = new List<ITransportStream>();
        }

        public ITorrentDownloadManager Manager { get; }

        public IReadOnlyCollection<BitTorrentPeerDetails> Peers => peers.Select(x => new BitTorrentPeerDetails(x.Key.Address, x.Key.PeerId)).ToList();

        public IEnumerable<BlockRequest> OutstandingBlockRequests => peers.SelectMany(x => x.Value.Requested);

        /// <summary>
        /// Called when new peers become available to connect to.
        /// </summary>
        /// <param name="streams">Result containing information from the tracker.</param>
        public void PeersAvailable(IEnumerable<ITransportStream> streams)
        {
            TrackerStreams.AddRange(streams);
        }

        /// <summary>
        /// Handles new incoming connection requests.
        /// </summary>
        /// <param name="e">Event args for handling the request.</param>
        public void AcceptConnection(AcceptConnectionEventArgs e)
        {
            if (!peers.TryAdd(e.Stream, new PeerInformation(Manager.Description)))
            {
                throw new InvalidOperationException("Unable to accept the connection.");
            }
            e.Accept();

            SendMessage(e.Stream, new BitfieldMessage(new Bitfield(Manager.Description.Pieces.Count, Manager.CompletedPieces)));
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

            foreach (var peer in peers.Keys)
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
        /// Invoked when a message is received from a connected stream.
        /// </summary>
        /// <param name="stream">Stream the message was received on.</param>
        /// <param name="data">Received message data.</param>
        public void MessageReceived(ITransportStream stream, byte[] data)
        {
            if (data.Length == 0)
                return;

            BinaryReader reader = new BigEndianBinaryReader(new MemoryStream(data));

            byte messageId = reader.ReadByte();

            // Read message
            CommonPeerMessage message = MessageHandler.ReadMessage(Manager.Description, reader, data.Length, messageId);
            if (message == null)
            {
                // Something went wrong
                stream.Dispose();

                return;
            }

            // Process message
            switch (message.ID)
            {
                case ChokeMessage.MessageID:
                    SetChokedByPeer(stream, true);
                    break;
                case UnchokeMessage.MessageID:
                    SetChokedByPeer(stream, false);
                    break;
                case InterestedMessage.MessageID:
                    SetPeerInterested(stream, true);
                    UnchokePeer(stream);
                    break;
                case NotInterestedMessage.MessageID:
                    SetPeerInterested(stream, false);
                    break;
                case HaveMessage.MessageId:
                {
                    HaveMessage haveMessage = message as HaveMessage;
                    SetPeerBitfield(stream, haveMessage.Piece.Index, true);
                    break;
                }
                case BitfieldMessage.MessageId:
                {
                    BitfieldMessage bitfieldMessage = message as BitfieldMessage;
                    SetPeerBitfield(stream, bitfieldMessage.Bitfield);
                    if (IsBitfieldInteresting(bitfieldMessage.Bitfield))
                    {
                        peers[stream].IsInterestedInRemotePeer = true;
                        SendMessage(stream, new InterestedMessage());
                    }
                    break;
                }
                case RequestMessage.MessageID:
                {
                    RequestMessage requestMessage = message as RequestMessage;
                    SetBlockRequestedByPeer(stream, requestMessage.Block);
                    break;
                }
                case CancelMessage.MessageID:
                {
                    CancelMessage cancelMessage = message as CancelMessage;
                    SetBlockCancelledByPeer(stream, cancelMessage.Block);
                    break;
                }
                case PieceMessage.MessageId:
                {
                    PieceMessage pieceMessage = message as PieceMessage;
                    BlockReceived(stream, pieceMessage.Block);
                    break;
                }
            }
        }

        private void RequestPieces()
        {
            var availability = new Bitfield(Manager.Description.Pieces.Count);
            foreach (var peer in peers)
                availability.Union(peer.Value.Available);

            var blocksToRequest = picker.BlocksToRequest(Manager.IncompletePieces, availability);

            foreach (var block in blocksToRequest)
            {
                var peer = FindPeerWithPiece(Manager.Description.Pieces[block.PieceIndex]);
                if (peer != null)
                {
                    var peerInfo = peers[peer];
                    peerInfo.Requested.Add(block);
                    picker.BlockRequested(block);
                    SendMessage(peer, new RequestMessage(block));
                }
            }
        }

        private ITransportStream FindPeerWithPiece(Piece piece)
        {
            foreach (var peer in peers.OrderBy(x => x.Value.Requested.Count))
            {
                if (peer.Value.Available.IsPieceAvailable(piece.Index)
                    && !peer.Value.IsChokedByRemotePeer)
                    return peer.Key;
            }

            // Piece is not available
            return null;
        }

        private void SendPieces()
        {
            foreach (var peer in peers)
            {
                var sent = new List<BlockRequest>();
                foreach (var request in peer.Value.RequestedByRemotePeer)
                {
                    sent.Add(request);
                    SendPiece(peer.Key, request);
                }

                foreach (var request in sent)
                    peer.Value.RequestedByRemotePeer.Remove(request);
            }
        }

        private void ConnectToPeers()
        {
            // If seeding, allow incoming connections only - don't initiate connections
            if (Manager.State != DownloadState.Downloading)
                return;

            if (peers.Count + connectingPeers.Count < 5 && TrackerStreams.Count > 0)
            {
                var peer = TrackerStreams.First();

                try
                {
                    TrackerStreams.Remove(peer);
                    connectingPeers.Add(peer);
                    peer.Connect().ContinueWith(antecedent =>
                    {
                        if (antecedent.Status != TaskStatus.RanToCompletion
                            || !peer.IsConnected)
                        {
                            Log.LogInformation($"Failed to connect to peer at {peer.Address}");

                            // Connection failed
                            connectingPeers.Remove(peer);
                            return;
                        }

                        Log.LogInformation($"Connected to peer at {peer.Address}");

                        peers.TryAdd(peer, new PeerInformation(Manager.Description));
                        connectingPeers.Remove(peer);
                        SendMessage(peer, new BitfieldMessage(new Bitfield(Manager.Description.Pieces.Count, Manager.CompletedPieces)));
                    });
                }
                catch
                {
                    if (connectingPeers.Contains(peer))
                        connectingPeers.Remove(peer);
                }
            }
        }

        private void SendPiece(ITransportStream stream, BlockRequest request)
        {
            long dataOffset = Manager.Description.PieceSize * request.PieceIndex + request.Offset;
            byte[] data = Manager.ReadData(dataOffset, request.Length);

            SendMessage(stream, new PieceMessage(request.ToBlock(data)));
        }

        private void SendMessage(ITransportStream stream, IPeerMessage message)
        {
            using (var ms = new MemoryStream())
            {
                BinaryWriter writer = new BigEndianBinaryWriter(ms);
                message.Send(writer);
                stream.SendData(ms.ToArray());
            }
        }

        private void BlockReceived(ITransportStream stream, Block block)
        {
            peers[stream].Requested.Remove(block.AsRequest());
            picker.BlockReceived(block);
            long dataOffset = Manager.Description.PieceSize * block.PieceIndex + block.Offset;
            Manager.DataReceived(dataOffset, block.Data);
        }

        private bool IsBitfieldInteresting(Bitfield bitfield)
        {
            var clientBitfield = new Bitfield(Manager.Description.Pieces.Count, Manager.CompletedPieces);
            return Bitfield.NotSubset(bitfield, clientBitfield);
        }

        private void SetBlockCancelledByPeer(ITransportStream stream, BlockRequest blockRequest)
        {
            peers[stream].RequestedByRemotePeer.Remove(blockRequest);
        }

        private void SetBlockRequestedByPeer(ITransportStream stream, BlockRequest blockRequest)
        {
            peers[stream].RequestedByRemotePeer.Add(blockRequest);
        }

        private void SetPeerBitfield(ITransportStream stream, Bitfield bitfield)
        {
            PeerInformation peerInfo;
            while (!peers.TryGetValue(stream, out peerInfo))
            {
            }
            peerInfo.Available = bitfield;
        }

        private void SetPeerBitfield(ITransportStream stream, int pieceIndex, bool available)
        {
            var peer = peers[stream];
            peer.Available.SetPieceAvailable(pieceIndex, available);

            if (!peer.IsInterestedInRemotePeer
                && IsBitfieldInteresting(peer.Available))
            {
                peers[stream].IsInterestedInRemotePeer = true;
                SendMessage(stream, new InterestedMessage());
            }
        }

        private void SetPeerInterested(ITransportStream stream, bool isInterested)
        {
            peers[stream].IsRemotePeerInterested = isInterested;
        }

        private void UnchokePeer(ITransportStream stream)
        {
            peers[stream].IsChokingRemotePeer = false;
            SendMessage(stream, new UnchokeMessage());
        }

        private void SetChokedByPeer(ITransportStream stream, bool choked)
        {
            peers[stream].IsChokedByRemotePeer = choked;
        }
    }
}
