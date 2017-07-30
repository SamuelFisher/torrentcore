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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TorrentCore.Application;
using TorrentCore.Data;
using TorrentCore.Engine;
using TorrentCore.Tracker;
using TorrentCore.Transport;

namespace TorrentCore
{
    /// <summary>
    /// Manages the download of a torrent.
    /// </summary>
    class TorrentDownloadManager : ITorrentDownloadManager
    {
        private static readonly ILogger Log = LogManager.GetLogger<TorrentDownloadManager>();

        private readonly IMainLoop mainLoop;
        private readonly PieceCheckerHandler dataHandler;

        private volatile int recentlyDownloaded;
        private volatile int recentlyUploaded;

        internal TorrentDownloadManager(IMainLoop loop,
                                        Func<ITorrentDownloadManager, ITransportProtocol> transportProtocol,
                                        Func<ITorrentDownloadManager, IApplicationProtocol> applicationProtocol,
                                        ITracker tracker,
                                        IFileHandler handler,
                                        Metainfo description)
        {
            mainLoop = loop;
            TransportProtocol = transportProtocol(this);
            ApplicationProtocol = applicationProtocol(this);
            dataHandler = new PieceCheckerHandler(new BlockDataHandler(handler, description));
            dataHandler.PieceCompleted += args => CompletedPieces.Add(args.Piece);
            Description = description;
            Tracker = tracker;
            State = DownloadState.Stopped;
            Downloaded = 0;
            CompletedPieces = new HashSet<Piece>();
            DownloadRateMeasurer = new RateMeasurer();
            UploadRateMeasurer = new RateMeasurer();
        }

        internal ITransportProtocol TransportProtocol { get; }

        internal IApplicationProtocol ApplicationProtocol { get; }

        internal ITracker Tracker { get; }

        /// <summary>
        /// Gets the metainfo describing the collection of files.
        /// </summary>
        public Metainfo Description { get; }

        /// <summary>
        /// Gets the number of bytes downloaded so far.
        /// </summary>
        public long Downloaded { get; private set; }

        /// <summary>
        /// Gets the number of bytes still to be downloaded.
        /// </summary>
        public long Remaining => Description.TotalSize - Downloaded;

        /// <summary>
        /// Gets a value indicating the percentage progress.
        /// </summary>
        public double Progress { get; private set; }

        /// <summary>
        /// Gets the current state of the download.
        /// </summary>
        public DownloadState State { get; private set; }

        /// <summary>
        /// Gets the set of completed pieces.
        /// </summary>
        public HashSet<Piece> CompletedPieces { get; }

        IReadOnlyCollection<Piece> ITorrentDownloadManager.CompletedPieces => CompletedPieces;

        /// <summary>
        /// Gets the set of incomplete pieces.
        /// </summary>
        public IEnumerable<Piece> IncompletePieces => Description.Pieces.Except(CompletedPieces);

        /// <summary>
        /// Gets the RateMeasurer used to measure the download rate.
        /// </summary>
        public RateMeasurer DownloadRateMeasurer { get; }

        /// <summary>
        /// Gets the RateMeasurer used to measure the upload rate.
        /// </summary>
        public RateMeasurer UploadRateMeasurer { get; }

        /// <summary>
        /// Occurs when all data has finished downloading.
        /// </summary>
        public event EventHandler Completed;

        public async Task Start()
        {
            if (State != DownloadState.Stopped)
                throw new InvalidOperationException("Already started.");

            Log.LogInformation("Starting download");

            // Make sure loop is not already running
            if (mainLoop.IsRunning)
                return;

            Log.LogInformation("Checking downloaded data...");

            State = DownloadState.Checking;

            // Check download progress
            await HashPiecesData();

            Log.LogInformation($"Download is {(double)Downloaded / Description.TotalSize:P} complete");

            // Set download state
            if (Downloaded < Description.TotalSize)
                State = DownloadState.Downloading;
            else
                State = DownloadState.Completed;

            SetDownloadProgress();

            if (State != DownloadState.Completed)
                await ContactTracker();

            // Listen for incoming connections
            TransportProtocol.Start();

            // Start main loop
            mainLoop.AddRegularTask(() => ApplicationProtocol.Iterate());
            mainLoop.Start();
        }

        private void SetDownloadProgress()
        {
            Progress = Downloaded / (double)Description.TotalSize;

            if (Remaining == 0)
                Completed?.Invoke(this, new EventArgs());
        }

        private Task HashPiecesData()
        {
            return Task.Run(() =>
            {
                Progress = 0;
                Downloaded = 0;
                CompletedPieces.Clear();
                using (var sha1 = SHA1.Create())
                {
                    foreach (Piece piece in Description.Pieces)
                    {
                        // Verify piece hash
                        long pieceOffset = Description.PieceOffset(piece);
                        byte[] pieceData;
                        if (!dataHandler.TryReadBlockData(pieceOffset, piece.Size, out pieceData))
                        {
                            Progress = (piece.Index + 1d) / Description.Pieces.Count;
                            continue;
                        }

                        var hash = new Sha1Hash(sha1.ComputeHash(pieceData));
                        if (hash == piece.Hash)
                        {
                            Downloaded += piece.Size;
                            CompletedPieces.Add(piece);
                        }
                        Progress = (piece.Index + 1d) / Description.Pieces.Count;
                    }
                }
            });
        }

        private async Task ContactTracker()
        {
            Log.LogInformation("Contacting tracker");

            try
            {
                var request = new AnnounceRequest(IPAddress.Loopback,
                                                  ((TcpTransportProtocol)TransportProtocol).Port,
                                                  Remaining,
                                                  Description.InfoHash);

                var result = await Tracker.Announce(request);

                Log.LogInformation($"{result.Peers.Count} peers available");

                ApplicationProtocol.PeersAvailable(result.Peers.Select(x => TransportProtocol.CreateTransportStream(x.IPAddress, x.Port, Description.InfoHash)));
            }
            catch (System.Net.Http.HttpRequestException)
            {
                // Cannot connect to tracker
                State = DownloadState.Error;
                return;
            }
        }

        public void Stop()
        {
            State = DownloadState.Stopped;
            TransportProtocol.Stop();
            mainLoop.Stop();
        }

        public void Dispose()
        {
            if (State != DownloadState.Stopped)
                Stop();
            dataHandler.FileHandler.Dispose();
        }

        public void DataReceived(long offset, byte[] data)
        {
            dataHandler.WriteBlockData(offset, data);
            Downloaded += data.Length;
            SetDownloadProgress();
            recentlyDownloaded += data.Length;

            if (Remaining == 0)
            {
                State = DownloadState.Completed;

                dataHandler.FileHandler.Flush();

                Debug.WriteLine("Completed download.");
            }
        }

        public byte[] ReadData(long offset, long length)
        {
            recentlyUploaded += (int)length;

            return dataHandler.ReadBlockData(offset, length);
        }

        internal void UpdateStatistics()
        {
            DownloadRateMeasurer.AddMeasure(recentlyDownloaded);
            recentlyDownloaded = 0;

            UploadRateMeasurer.AddMeasure(recentlyUploaded);
            recentlyUploaded = 0;
        }
    }
}
