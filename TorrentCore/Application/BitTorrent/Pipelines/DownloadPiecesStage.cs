// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Application.BitTorrent.Messages;
using TorrentCore.Application.Pipelines;
using TorrentCore.Data;
using TorrentCore.Data.Pieces;
using TorrentCore.Engine;

namespace TorrentCore.Application.BitTorrent.Pipelines;

/// <summary>
/// Downloads the torrent and seeds to peers. Does not complete until signalled to stop.
/// </summary>
class DownloadPiecesStage : IPipelineStage
{
    private const int MaxConnectedPeers = 5;

    private readonly IMainLoop _mainLoop;
    private readonly IPiecePicker _piecePicker;

    public DownloadPiecesStage(BitTorrentApplicationProtocol applicationProtocol, IMainLoop mainLoop, IPiecePicker piecePicker)
    {
        ApplicationProtocol = applicationProtocol;
        _mainLoop = mainLoop;
        _piecePicker = piecePicker;
    }

    public BitTorrentApplicationProtocol ApplicationProtocol { get; set; }

    public void Run(IStageInterrupt interrupt, IProgress<StatusUpdate> progress)
    {
        using (var iterate = _mainLoop.AddRegularTask(() => Iterate(progress)))
        {
            interrupt.StopWaitHandle.WaitOne();
        }
    }

    private void Iterate(IProgress<StatusUpdate> progress)
    {
        progress.Report(new StatusUpdate(DownloadState.Downloading, ApplicationProtocol.DataHandler.CompletedPieces.Sum(x => (long)x.Size) / ApplicationProtocol.Metainfo.Pieces.Sum(x => (long)x.Size)));

        if (ApplicationProtocol.DataHandler.IncompletePieces().Any())
            RequestPieces();
        SendPieces();
        ConnectToPeers();
    }

    private void RequestPieces()
    {
        var availability = new Bitfield(ApplicationProtocol.Metainfo.Pieces.Count);
        foreach (var peer in ApplicationProtocol.Peers)
            availability.Union(peer.Available);

        var blocksToRequest = _piecePicker.BlocksToRequest(ApplicationProtocol.DataHandler.IncompletePieces().ToList(),
                                                          availability,
                                                          ApplicationProtocol.Peers,
                                                          ApplicationProtocol.BlockRequests);

        foreach (var block in blocksToRequest)
        {
            var peer = FindPeerWithPiece(ApplicationProtocol.Metainfo.Pieces[block.PieceIndex]);
            if (peer != null)
            {
                peer.Requested.Add(block);
                ApplicationProtocol.BlockRequests.BlockRequested(block);
                peer.SendMessage(new RequestMessage(block));
            }
        }
    }

    private BitTorrentPeer? FindPeerWithPiece(Piece piece)
    {
        foreach (var peer in ApplicationProtocol.Peers.OrderBy(x => x.Requested.Count))
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
        foreach (var peer in ApplicationProtocol.Peers)
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

    private void SendPiece(BitTorrentPeer peer, BlockRequest request)
    {
        long dataOffset = ApplicationProtocol.Metainfo.PieceSize * request.PieceIndex + request.Offset;
        byte[]? data = ApplicationProtocol.DataHandler.ReadBlockData(dataOffset, request.Length);

        peer.SendMessage(new PieceMessage(request.ToBlock(data!)));
        ApplicationProtocol.UploadedData(data!);
    }

    private void ConnectToPeers()
    {
        if (ApplicationProtocol.Peers.Count +
            ApplicationProtocol.ConnectingPeers.Count < MaxConnectedPeers &&
            ApplicationProtocol.AvailablePeers.Count > 0)
        {
            var peer = ApplicationProtocol.AvailablePeers.First();
            ApplicationProtocol.ConnectToPeer(peer);
        }
    }
}
