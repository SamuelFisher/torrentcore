// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using TorrentCore.Application.BitTorrent.Messages;
using TorrentCore.Data;
using TorrentCore.Modularity;

namespace TorrentCore.Application.BitTorrent
{
    /// <summary>
    /// Handles messages that are part of the core BitTorrent protocol.
    /// </summary>
    class CoreMessagingModule : IModule
    {
        private static readonly byte[] MessageIds =
        {
            ChokeMessage.MessageID,
            UnchokeMessage.MessageID,
            InterestedMessage.MessageID,
            NotInterestedMessage.MessageID,
            HaveMessage.MessageId,
            BitfieldMessage.MessageId,
            RequestMessage.MessageID,
            PieceMessage.MessageId,
            CancelMessage.MessageID
        };

        public void OnPrepareHandshake(IPrepareHandshakeContext context)
        {
            // Do nothing
        }

        public void OnPeerConnected(IPeerContext context)
        {
            foreach (var messageId in MessageIds)
                context.RegisterMessageHandler(messageId);

            context.Peer.SendMessage(new BitfieldMessage(new Bitfield(context.Metainfo.Pieces.Count, context.DataHandler.CompletedPieces)));
        }

        public void OnMessageReceived(IMessageReceivedContext context)
        {
            // Read message
            CommonPeerMessage message = MessageHandler.ReadMessage(context.Metainfo, context.Reader, context.MessageLength, (byte)context.MessageId);
            if (message == null)
            {
                // Something went wrong
                context.Peer.Disconnect();
                return;
            }

            var peer = context.Peer;

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
                    SetPeerBitfield(context, peer, haveMessage.Piece.Index, true);
                    break;
                }
                case BitfieldMessage.MessageId:
                {
                    BitfieldMessage bitfieldMessage = message as BitfieldMessage;
                    SetPeerBitfield(peer, bitfieldMessage.Bitfield);
                    if (IsBitfieldInteresting(context, bitfieldMessage.Bitfield))
                    {
                        peer.IsInterestedInRemotePeer = true;
                        peer.SendMessage(new InterestedMessage());
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
                    BlockReceived(context, peer, pieceMessage.Block);
                    break;
                }
            }
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

        private void SetPeerBitfield(IMessageReceivedContext context, PeerConnection peer, int pieceIndex, bool available)
        {
            peer.Available.SetPieceAvailable(pieceIndex, available);

            if (!peer.IsInterestedInRemotePeer
                && IsBitfieldInteresting(context, peer.Available))
            {
                peer.IsInterestedInRemotePeer = true;
                peer.SendMessage(new InterestedMessage());
            }
        }

        private void SetPeerInterested(PeerConnection peer, bool isInterested)
        {
            peer.IsRemotePeerInterested = isInterested;
        }

        private void UnchokePeer(PeerConnection peer)
        {
            peer.IsChokingRemotePeer = false;
            peer.SendMessage(new UnchokeMessage());
        }

        private void SetChokedByPeer(PeerConnection peer, bool choked)
        {
            peer.IsChokedByRemotePeer = choked;
        }

        private void BlockReceived(IMessageReceivedContext context, PeerConnection peer, Block block)
        {
            peer.Requested.Remove(block.AsRequest());
            context.BlockRequests.BlockReceived(block);
            long dataOffset = context.Metainfo.PieceSize * block.PieceIndex + block.Offset;
            context.DataHandler.WriteBlockData(dataOffset, block.Data);
        }

        private bool IsBitfieldInteresting(IMessageReceivedContext context, Bitfield bitfield)
        {
            var clientBitfield = new Bitfield(context.Metainfo.Pieces.Count, context.DataHandler.CompletedPieces);
            return Bitfield.NotSubset(bitfield, clientBitfield);
        }
    }
}
