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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TorrentCore.Application;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Data;
using TorrentCore.Engine;
using TorrentCore.Tracker;
using TorrentCore.Transport;

namespace TorrentCore
{
    public class TorrentClient : IMessageHandler, IDisposable
    {
        private static readonly ILogger Log = LogManager.GetLogger<TorrentClient>();

        private readonly IDictionary<Sha1Hash, TorrentDownload> downloads;
        private readonly MainLoop mainLoop;
        private readonly TcpTransportProtocol transport;

        public TorrentClient(int listenPort)
            : this(new TorrentClientSettings {ListenPort = listenPort})
        {
        }

        public TorrentClient(TorrentClientSettings settings)
        {
            downloads = new Dictionary<Sha1Hash, TorrentDownload>();
            mainLoop = new MainLoop();
            transport = new TcpTransportProtocol(this,
                                                 mainLoop,
                                                 settings.ListenPort,
                                                 settings.AdapterAddress,
                                                 AcceptConnection);
        }

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
            return Add(metainfo, new HttpTracker(metainfo.Trackers[0][0]), new DiskFileHandler(downloadDirectory));
        }

        internal TorrentDownload Add(Metainfo metainfo, ITracker tracker, IFileHandler fileHandler)
        {
            var downloadManager = new TorrentDownloadManager(mainLoop,
                                                             manager => transport,
                                                             manager => new BitTorrentApplicationProtocol(manager),
                                                             tracker,
                                                             fileHandler,
                                                             metainfo);

            var download = new TorrentDownload(downloadManager);

            downloads.Add(metainfo.InfoHash, download);
            return download;
        }

        void IMessageHandler.MessageReceived(ITransportStream stream, byte[] data)
        {
            // Route to the ApplicationProtocol responsible for this infohash
            downloads[stream.InfoHash].Manager.ApplicationProtocol.MessageReceived(stream, data);
        }

        private void AcceptConnection(AcceptConnectionEventArgs e)
        {
            Log.LogDebug("A peer wants to connect");

            downloads[e.Stream.InfoHash].Manager.ApplicationProtocol.AcceptConnection(e);
        }

        public void Dispose()
        {
            foreach (var download in Downloads)
                download.Stop();
        }
    }
}
