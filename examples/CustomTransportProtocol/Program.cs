// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TorrentCore;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Data;
using TorrentCore.Engine;
using TorrentCore.Tracker;
using TorrentCore.Transport;

namespace CustomTransportProtocol
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var transportDir = Path.Combine(Environment.CurrentDirectory, "transport");
            var seederDir = Path.Combine(Environment.CurrentDirectory, "seeder");
            var downloaderDir = Path.Combine(Environment.CurrentDirectory, "downloader");

            // Clear old data
            if (Directory.Exists(transportDir))
                Directory.Delete(transportDir, true);
            if (Directory.Exists(seederDir))
                Directory.Delete(seederDir, true);
            if (Directory.Exists(downloaderDir))
                Directory.Delete(downloaderDir, true);

            // Create directories
            Directory.CreateDirectory(transportDir);
            Directory.CreateDirectory(Path.Combine(transportDir, "announce"));
            Directory.CreateDirectory(seederDir);
            Directory.CreateDirectory(downloaderDir);
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TorrentContent.txt"), Path.Combine(seederDir, "TorrentContent.txt"), true);

            // Share a main loop between both peers to make this easier to follow as an example
            var mainLoop = new MainLoop();

            var metainfo = new MetainfoBuilder("CustomTransportProtocol")
                .AddFile("TorrentContent.txt", File.ReadAllBytes("TorrentContent.txt"))
                .WithTracker(new Uri("http://example.com/announce")) // Tracker URI is not used in this example
                .Build();

            var cancelSeeder = new CancellationTokenSource();
            var seeder = RunSeederAsync(mainLoop, transportDir, seederDir, metainfo, cancelSeeder.Token);

            // Run downloader and awit until finished downloading
            await RunDownloaderAsync(mainLoop, transportDir, downloaderDir, metainfo);

            // Stop seeding
            cancelSeeder.Cancel();

            mainLoop.Stop();

            Console.WriteLine("Finished...");
            Console.ReadKey();
        }

        static async Task RunSeederAsync(IMainLoop mainLoop, string transportDir, string contentDir, Metainfo metainfo, CancellationToken ct)
        {
            var client = new TorrentClientBuilder()
                .UsePeerId(new PeerId(Encoding.ASCII.GetBytes("SEEDER".PadRight(20, 'X'))))
                .ConfigureServices(services =>
                {
                    services.AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Debug));

                    services.AddSingleton(mainLoop);
                    services.AddSingleton<ITransportProtocol>(s => new FileTransportProtocol(new DirectoryInfo(transportDir), s.GetRequiredService<PeerId>()));
                    services.AddSingleton<ITrackerClientFactory>(s => new SingletonTracker(new FileTracker(transportDir, s.GetRequiredService<PeerId>())));
                })
                .AddBitTorrentApplicationProtocol()
                .AddDefaultPipeline()
                .Build();

            var torrent = client.Add(metainfo, contentDir);
            torrent.Start();

            await Task.Run(() => ct.WaitHandle.WaitOne());

            torrent.Stop();
            client.Dispose();
        }

        static async Task RunDownloaderAsync(IMainLoop mainLoop, string transportDir, string contentDir, Metainfo metainfo)
        {
            var client = new TorrentClientBuilder()
                .UsePeerId(new PeerId(Encoding.ASCII.GetBytes("DOWNLOADER".PadRight(20, 'X'))))
                .ConfigureServices(services =>
                {
                    services.AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Debug));

                    services.AddSingleton(mainLoop);
                    services.AddSingleton<ITransportProtocol>(s => new FileTransportProtocol(new DirectoryInfo(transportDir), s.GetRequiredService<PeerId>()));
                    services.AddSingleton<ITrackerClientFactory>(s => new SingletonTracker(new FileTracker(transportDir, s.GetRequiredService<PeerId>())));
                })
                .AddBitTorrentApplicationProtocol()
                .AddDefaultPipeline()
                .Build();

            var torrent = client.Add(metainfo, contentDir);
            torrent.Start();

            await torrent.WaitForDownloadCompletionAsync();

            torrent.Stop();
            client.Dispose();
        }
    }
}
