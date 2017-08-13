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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Application;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Application.BitTorrent.Messages;
using TorrentCore.Data;
using TorrentCore.Data.Pieces;
using TorrentCore.Engine;

namespace TorrentCore.Stage
{
    class DownloadPiecesStage : ITorrentStage
    {
        private readonly IApplicationProtocol<PeerConnection> application;
        private readonly IMainLoop mainLoop;
        private readonly IPiecePicker piecePicker;

        public DownloadPiecesStage(IApplicationProtocol<PeerConnection> application,
                                   IMainLoop mainLoop,
                                   IPiecePicker piecePicker)
        {
            this.application = application;
            this.mainLoop = mainLoop;
            this.piecePicker = piecePicker;
        }

        public void Run(IStageInterrupt interrupt, IProgress<StatusUpdate> progress)
        {
            var iterate = mainLoop.AddRegularTask(Iterate);
            interrupt.StopWaitHandle.WaitOne();
            iterate.Cancel();
        }

        private void Iterate()
        {
            if (application.DataHandler.IncompletePieces().Any())
                RequestPieces();
            SendPieces();
            ConnectToPeers();
        }

        private void RequestPieces()
        {
            var availability = new Bitfield(application.Metainfo.Pieces.Count);
            foreach (var peer in application.Peers)
                availability.Union(peer.Available);

            var blocksToRequest = piecePicker.BlocksToRequest(application.DataHandler.IncompletePieces().ToList(),
                                                              availability,
                                                              application.Peers,
                                                              application.BlockRequests);

            foreach (var block in blocksToRequest)
            {
                var peer = FindPeerWithPiece(application.Metainfo.Pieces[block.PieceIndex]);
                if (peer != null)
                {
                    peer.Requested.Add(block);
                    application.BlockRequests.BlockRequested(block);
                    peer.SendMessage(new RequestMessage(block));
                }
            }
        }

        private PeerConnection FindPeerWithPiece(Piece piece)
        {
            foreach (var peer in application.Peers.OrderBy(x => x.Requested.Count))
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
            foreach (var peer in application.Peers)
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

        private void SendPiece(PeerConnection peer, BlockRequest request)
        {
            long dataOffset = application.Metainfo.PieceSize * request.PieceIndex + request.Offset;
            byte[] data = application.DataHandler.ReadBlockData(dataOffset, request.Length);

            peer.SendMessage(new PieceMessage(request.ToBlock(data)));
        }

        private void ConnectToPeers()
        {
            if (application.Peers.Count +
                application.ConnectingPeers.Count < 5 &&
                application.AvailablePeers.Count > 0)
            {
                var peer = application.AvailablePeers.First();
                application.ConnectToPeer(peer);
            }
        }
    }
}
