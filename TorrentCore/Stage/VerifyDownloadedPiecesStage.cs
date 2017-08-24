// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Application;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Data;
using TorrentCore.Data.Pieces;

namespace TorrentCore.Stage
{
    class VerifyDownloadedPiecesStage : ITorrentStage
    {
        private readonly IApplicationProtocol<PeerConnection> application;

        public VerifyDownloadedPiecesStage(IApplicationProtocol<PeerConnection> application)
        {
            this.application = application;
        }

        public void Run(IStageInterrupt interrupt, IProgress<StatusUpdate> progress)
        {
            if (application.DataHandler.IncompletePieces().Any())
                HashPiecesData(interrupt, progress);
        }

        private void HashPiecesData(IStageInterrupt interrupt, IProgress<StatusUpdate> progress)
        {
            progress.Report(new StatusUpdate(DownloadState.Checking, 0.0));
            using (var sha1 = SHA1.Create())
            {
                foreach (Piece piece in application.Metainfo.Pieces)
                {
                    // Verify piece hash
                    long pieceOffset = application.Metainfo.PieceOffset(piece);
                    byte[] pieceData;
                    if (!application.DataHandler.TryReadBlockData(pieceOffset, piece.Size, out pieceData))
                    {
                        progress.Report(new StatusUpdate(DownloadState.Checking,
                            (piece.Index + 1d) / application.Metainfo.Pieces.Count));
                        continue;
                    }

                    var hash = new Sha1Hash(sha1.ComputeHash(pieceData));
                    if (hash == piece.Hash)
                        application.DataHandler.MarkPieceAsCompleted(piece);

                    progress.Report(new StatusUpdate(DownloadState.Checking,
                        (piece.Index + 1d) / application.Metainfo.Pieces.Count));

                    if (interrupt.IsPauseRequested)
                        interrupt.InterruptHandle.WaitOne();
                    if (interrupt.IsStopRequested)
                        return;
                }
            }
        }
    }
}
