// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TorrentCore.Application;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Data;
using TorrentCore.Engine;
using TorrentCore.Tracker;
using TorrentCore.Transport;

namespace TorrentCore;

public class TorrentClient : ITorrentClient
{
    private readonly ILogger _logger;
    private readonly IDictionary<Sha1Hash, TorrentDownload> _downloads;
    private readonly IMainLoop _mainLoop;
    private readonly ITrackerClientFactory _trackerClientFactory;
    private readonly IServiceProvider _services;
    private readonly IApplicationProtocolPeerInitiator _peerInitiator;
    private readonly Timer _updateStatisticsTimer;

    public TorrentClient(
        ILogger<TorrentClient> logger,
        PeerId localPeerId,
        IMainLoop mainLoop,
        ITransportProtocol transport,
        ITrackerClientFactory trackerClientFactory,
        IApplicationProtocolPeerInitiator peerInitiator,
        IServiceProvider services)
    {
        _logger = logger;
        _downloads = new Dictionary<Sha1Hash, TorrentDownload>();
        _mainLoop = mainLoop;
        _mainLoop.Start();
        _trackerClientFactory = trackerClientFactory;
        _services = services;
        _updateStatisticsTimer = new Timer(UpdateStatistics, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        _peerInitiator = peerInitiator;
        LocalPeerId = localPeerId;

        Transport = transport;
        transport.AcceptConnectionHandler += _peerInitiator.AcceptIncomingConnection;
        Transport.Start();
    }

    /// <summary>
    /// Gets the Peer ID for the local client.
    /// </summary>
    public PeerId LocalPeerId { get; }

    internal ITransportProtocol Transport { get; }

    public IReadOnlyCollection<TorrentDownload> Downloads => new ReadOnlyCollection<TorrentDownload>(_downloads.Values.ToList());

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
        return Add(metainfo, new AggregatedTracker(_trackerClientFactory, metainfo.Trackers), new DiskFileHandler(downloadDirectory));
    }

    internal TorrentDownload Add(Metainfo metainfo, ITracker tracker, IFileHandler fileHandler)
    {
        // Create a new scope for this download
        var scope = _services.CreateScope();
        var dataHandler = ActivatorUtilities.CreateInstance<PieceCheckerHandler>(scope.ServiceProvider, new BlockDataHandler(fileHandler, metainfo));
        var applicationProtocolFactory = scope.ServiceProvider.GetRequiredService<IApplicationProtocolFactory>();
        var applicationProtocol = applicationProtocolFactory.Create(metainfo, dataHandler);
        var pipelineRunner = ActivatorUtilities.CreateInstance<PipelineRunner>(scope.ServiceProvider, applicationProtocol, tracker);
        _peerInitiator.OnApplicationProtocolAdded(pipelineRunner.ApplicationProtocol);

        var download = new TorrentDownload(pipelineRunner);

        _downloads.Add(metainfo.InfoHash, download);
        return download;
    }

    private void UpdateStatistics(object? state)
    {
        foreach (var dl in _downloads)
            dl.Value.Manager.UpdateStatistics();
    }

    public void Dispose()
    {
        foreach (var download in Downloads)
            download.Stop();
        _mainLoop.Stop();
    }

    public static ITorrentClient Create()
    {
        return TorrentClientBuilder.CreateDefaultBuilder().Build();
    }
}
