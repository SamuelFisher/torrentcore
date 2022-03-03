// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;
using TorrentCore.Data;
using TorrentCore.TorrentParsers;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Test.Functional;

[TestFixture(Category = "Functional", Explicit = true)]
public class ExternalTest
{
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(Serilog.Events.LogEventLevel.Information)
            .WriteTo.Console(outputTemplate: "[{Level:u4}][{SourceContext:l}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    [Test]
    public async Task TestDownload()
    {
        var torrentContent = GetFileData();
        var torrent = new BencodeNET.Torrents.Torrent();
        torrent.File = new BencodeNET.Torrents.SingleFileInfo
        {
            FileName = "test.bin",
            FileSize = torrentContent.Length,
        };
        torrent.PieceSize = 256000;

        var pieces = PieceCalculator.ComputePieces(
            new[] { new ContainedFile("test.bin", torrentContent.Length) },
            (int)torrent.PieceSize,
            new MemoryFileHandler("test.bin", torrentContent));
        torrent.Pieces = pieces.Aggregate(Enumerable.Empty<byte>(), (a, b) => a.Concat(b.Hash.Value)).ToArray();

        var torrentContentMs = new MemoryStream();
        torrent.EncodeTo(torrentContentMs);
        var torrentFileContent = torrentContentMs.ToArray();

        var workDir = Path.Combine(Path.GetTempPath(), $"TorrentCore_test_{Guid.NewGuid()}");
        if (Directory.Exists(workDir))
        {
            throw new ApplicationException();
        }
        else
        {
            Directory.CreateDirectory(workDir);
            Directory.CreateDirectory(Path.Combine(workDir, "seed"));
            Directory.CreateDirectory(Path.Combine(workDir, "output"));

            await File.WriteAllBytesAsync(Path.Combine(workDir, "test.torrent"), torrentFileContent);
            await File.WriteAllBytesAsync(Path.Combine(workDir, "seed", "test.bin"), torrentContent);
        }

        Process aria2 = null;
        try
        {
            aria2 = Process.Start(new ProcessStartInfo
            {
                FileName = "aria2c",
                Arguments = "test.torrent --listen-port 6882 --enable-dht=false --seed-ratio=0.0 -V -d seed",
                WorkingDirectory = workDir,
            });

            var tracker = new MockTracker();
            tracker.RegisterPeer(IPAddress.Loopback, 6882);

            using var underTest = (TorrentClient)TorrentClientBuilder.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
                    services.Configure<LocalTcpConnectionOptions>(x => x.BindAddress = IPAddress.Any);
                })
                .Build();
            torrentContentMs.Seek(0, SeekOrigin.Begin);
            var download = underTest.Add(
                TorrentParser.ReadFromStream(torrentContentMs),
                tracker.CreateTrackerClient(null),
                new DiskFileHandler(Path.Combine(workDir, "output")));

            download.Start();
            try
            {
                await download.WaitForDownloadCompletionAsync(TimeSpan.FromSeconds(30));
            }
            catch (TimeoutException)
            {
                Assert.Fail($"Timed out at {download.Progress:P}. Seed status: HasExited={aria2.HasExited}.");
            }

            download.Stop();
            download.Dispose();

            var resultFile = Path.Combine(workDir, "output", "test.bin");
            Assert.That(File.Exists(resultFile), Is.True, "Result file does not exit");
            var result = await File.ReadAllBytesAsync(resultFile);
            Assert.That(result, Has.Length.EqualTo(torrentContent.Length));
            Assert.That(result, Is.EquivalentTo(torrentContent));
        }
        finally
        {
            if (aria2 != null)
            {
                aria2.Kill();
                aria2.WaitForExit();
                aria2.Dispose();
            }

            Directory.Delete(workDir, true);
        }
    }

    private static byte[] GetFileData()
    {
        return Enumerable.Repeat(Enumerable.Range(0, 255), 10000).SelectMany(x => x).Select(x => (byte)x).ToArray();
    }
}
