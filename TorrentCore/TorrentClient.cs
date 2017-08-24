// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TorrentCore.Application;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Application.BitTorrent.Connection;
using TorrentCore.Data;
using TorrentCore.Engine;
using TorrentCore.Modularity;
using TorrentCore.Tracker;
using TorrentCore.Transport;
using TorrentCore.Transport.Tcp;

namespace TorrentCore
{
    public class TorrentClient : IDisposable
    {
        private static readonly ILogger Log = LogManager.GetLogger<TorrentClient>();

        private readonly IDictionary<Sha1Hash, TorrentDownload> downloads;
        private readonly MainLoop mainLoop;
        private readonly TcpTransportProtocol transport;
        private readonly ITrackerClientFactory trackerClientFactory;
        private readonly BitTorrentPeerInitiator peerInitiator;
        private readonly IModuleManager moduleManager;
        private Timer updateStatisticsTimer;

        public TorrentClient()
            : this(new TorrentClientSettings { FindAvailablePort = true })
        {
        }

        public TorrentClient(int listenPort)
            : this(new TorrentClientSettings { ListenPort = listenPort })
        {
        }

        public TorrentClient(TorrentClientSettings settings)
        {
            downloads = new Dictionary<Sha1Hash, TorrentDownload>();
            mainLoop = new MainLoop();
            moduleManager = new ModuleManager();
            moduleManager.Register(new CoreMessagingModule());
            mainLoop.Start();
            peerInitiator = new BitTorrentPeerInitiator(infoHash => (BitTorrentApplicationProtocol<BitTorrentPeerInitiator.IContext>)downloads[infoHash].Manager.ApplicationProtocol, moduleManager);
            LocalPeerId = settings.PeerId;

            transport = new TcpTransportProtocol(
                settings.ListenPort,
                settings.FindAvailablePort,
                settings.AdapterAddress,
                AcceptConnection);

            transport.Start();
            trackerClientFactory = new TrackerClientFactory(transport.LocalConection);
            updateStatisticsTimer = new Timer(UpdateStatistics, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            AdapterAddress = settings.AdapterAddress;
        }

        /// <summary>
        /// Gets the Peer ID for the local client.
        /// </summary>
        public PeerId LocalPeerId { get; }

        /// <summary>
        /// Gets the address of the adapter used for connections.
        /// </summary>
        public IPAddress AdapterAddress { get; }

        public IModuleManager Modules => moduleManager;

        internal TcpTransportProtocol Transport => transport;

        public IReadOnlyCollection<TorrentDownload> Downloads => new ReadOnlyCollection<TorrentDownload>(downloads.Values.ToList());

        private void AcceptConnection(AcceptConnectionEventArgs e)
        {
            var applicationProtocol = peerInitiator.PrepareAcceptIncomingConnection(e.TransportStream, out BitTorrentPeerInitiator.IContext context);
            applicationProtocol.AcceptConnection(new AcceptPeerConnectionEventArgs<PeerConnection>(e.TransportStream, () =>
            {
                e.Accept();
                var c = new PeerConnectionArgs(LocalPeerId, applicationProtocol.Metainfo, new QueueingMessageHandler(mainLoop, applicationProtocol));
                return peerInitiator.AcceptIncomingConnection(e.TransportStream, context, c);
            }));
        }

        public TorrentDownload Add(string torrentFile, string downloadDirectory)
        {
            using (var stream = File.OpenRead(torrentFile))
                return Add(TorrentParsers.TorrentParser.ReadFromStream(stream), downloadDirectory);
        }

        public TorrentDownload Add(Stream torrentFileStream, string downloadDirectory)
        {
            return Add(TorrentParsers.TorrentParser.ReadFromStream(torrentFileStream), downloadDirectory);
        }

        public TorrentDownload Add(Metainfo metainfo, string downloadDirectory)
        {
            return Add(metainfo, new AggregatedTracker(trackerClientFactory, metainfo.Trackers), new DiskFileHandler(downloadDirectory));
        }

        internal TorrentDownload Add(Metainfo metainfo, ITracker tracker, IFileHandler fileHandler)
        {
            var dataHandler = new PieceCheckerHandler(new BlockDataHandler(fileHandler, metainfo));
            var bitTorrentApplicationProtocol = new BitTorrentApplicationProtocol<BitTorrentPeerInitiator.IContext>(LocalPeerId, metainfo, peerInitiator, m => new QueueingMessageHandler(mainLoop, m), moduleManager, dataHandler);
            var downloadManager = new TorrentDownloadManager(LocalPeerId,
                                                             mainLoop,
                                                             bitTorrentApplicationProtocol,
                                                             tracker,
                                                             metainfo);

            var download = new TorrentDownload(downloadManager);

            downloads.Add(metainfo.InfoHash, download);
            return download;
        }

        private void UpdateStatistics(object state)
        {
            foreach (var dl in downloads)
                dl.Value.Manager.UpdateStatistics();
        }

        public void Dispose()
        {
            foreach (var download in Downloads)
                download.Stop();
            mainLoop.Stop();
        }
    }
}
