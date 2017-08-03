// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2017 Sam Fisher.
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TorrentCore.Application;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Application.BitTorrent.Connection;
using TorrentCore.Data;
using TorrentCore.Engine;
using TorrentCore.ExtensionModule;
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
            : this (new TorrentClientSettings { FindAvailablePort = true})
        {
        }

        public TorrentClient(int listenPort)
            : this(new TorrentClientSettings {ListenPort = listenPort})
        {
        }

        public TorrentClient(TorrentClientSettings settings)
        {
            downloads = new Dictionary<Sha1Hash, TorrentDownload>();
            mainLoop = new MainLoop();
            moduleManager = new ModuleManager();
            mainLoop.Start();
            peerInitiator = new BitTorrentPeerInitiator(infoHash => (BitTorrentApplicationProtocol<BitTorrentPeerInitiator.IContext>)downloads[infoHash].Manager.ApplicationProtocol, moduleManager);
            LocalPeerId = settings.PeerId;
            transport = new TcpTransportProtocol(settings.ListenPort,
                                                 settings.FindAvailablePort,
                                                 settings.AdapterAddress,
                                                 AcceptConnection);
            transport.Start();
            trackerClientFactory = new TrackerClientFactory(transport.LocalConection);
            updateStatisticsTimer = new Timer(UpdateStatistics, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        private void AcceptConnection(AcceptConnectionEventArgs e)
        {
            var applicationProtocol = peerInitiator.PrepareAcceptIncomingConnection(e.TransportStream, out BitTorrentPeerInitiator.IContext context);
            applicationProtocol.AcceptConnection(new AcceptPeerConnectionEventArgs<PeerConnection>(e.TransportStream, () =>
            {
                e.Accept();
                var c = new PeerConnectionArgs(LocalPeerId, applicationProtocol.Manager.Description, new QueueingMessageHandler(mainLoop, applicationProtocol));
                return peerInitiator.AcceptIncomingConnection(e.TransportStream, context, c);
            }));
        }

        /// <summary>
        /// Gets the Peer ID for the local client.
        /// </summary>
        public PeerId LocalPeerId { get; }

        public IModuleManager Modules => moduleManager;

        internal TcpTransportProtocol Transport => transport;

        public IReadOnlyCollection<TorrentDownload> Downloads => new ReadOnlyCollection<TorrentDownload>(downloads.Values.ToList());

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
            var downloadManager = new TorrentDownloadManager(LocalPeerId,
                                                             mainLoop,
                                                             manager => new BitTorrentApplicationProtocol<BitTorrentPeerInitiator.IContext>(LocalPeerId, manager, peerInitiator, m => new QueueingMessageHandler(mainLoop, m), moduleManager),
                                                             tracker,
                                                             fileHandler,
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
        }
    }
}
