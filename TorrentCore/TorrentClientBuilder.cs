// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using TorrentCore.Application;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Application.BitTorrent.Connection;
using TorrentCore.Application.BitTorrent.Pipelines;
using TorrentCore.Application.Pipelines;
using TorrentCore.Engine;
using TorrentCore.Modularity;
using TorrentCore.Tracker;
using TorrentCore.Transport;
using TorrentCore.Transport.Tcp;

namespace TorrentCore;

public class TorrentClientBuilder
{
    private readonly IServiceCollection _services;

    public TorrentClientBuilder()
        : this(new ServiceCollection())
    {
    }

    public TorrentClientBuilder(IServiceCollection services)
    {
        _services = services;
        _services.AddOptions();
    }

    public TorrentClientBuilder UsePeerId(PeerId peerId)
    {
        _services.AddSingleton(peerId);
        return this;
    }

    public TorrentClientBuilder AddTcpTransportProtocol()
    {
        _services.AddSingleton<ITcpTransportProtocol, TcpTransportProtocol>();
        _services.AddSingleton<ITransportProtocol>(s => s.GetRequiredService<ITcpTransportProtocol>());
        _services.AddSingleton<ITrackerClientFactory, TrackerClientFactory>();
        return this;
    }

    public TorrentClientBuilder AddBitTorrentApplicationProtocol()
    {
        _services.AddSingleton<IApplicationProtocolPeerInitiator, BitTorrentPeerInitiator>();
        _services.AddScoped<IModule, CoreMessagingModule>();
        _services.AddScoped<IPiecePicker, PiecePicker>();
        _services.AddScoped<IApplicationProtocolFactory>(s => new ApplicationProtocolFactory<BitTorrentApplicationProtocol>(s));
        _services.AddScoped<IApplicationProtocol, BitTorrentApplicationProtocol>();
        return this;
    }

    public TorrentClientBuilder AddDefaultPipeline()
    {
        _services.AddSingleton(new PipelineBuilder()
            .AddStage<VerifyDownloadedPiecesStage>()
            .AddStage<DownloadPiecesStage>()
            .Build());
        return this;
    }

    public TorrentClientBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        configure(_services);
        return this;
    }

    /// <summary>
    /// Listens for incoming connections on the specified port.
    /// </summary>
    /// <param name="port">Port to listen for incoming connections on.</param>
    /// <returns>TorrentClientBuilder.</returns>
    public TorrentClientBuilder UsePort(int port)
    {
        _services.Configure<LocalTcpConnectionOptions>(options => options.Port = port);
        return this;
    }

    /// <summary>
    /// Sets up a default Torrent Client using the TCP transport protocol and the BitTorrent application protocol.
    /// </summary>
    /// <returns>Builder configured to construct a default torrent client.</returns>
    public static TorrentClientBuilder CreateDefaultBuilder()
    {
        return new TorrentClientBuilder()
            .UsePeerId(PeerId.CreateNew())
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddSingleton<IMainLoop, MainLoop>();
            })
            .AddTcpTransportProtocol()
            .AddBitTorrentApplicationProtocol()
            .AddDefaultPipeline();
    }

    public ITorrentClient Build()
    {
        var serviceProvider = _services.BuildServiceProvider();
        return ActivatorUtilities.CreateInstance<TorrentClient>(serviceProvider);
    }
}
