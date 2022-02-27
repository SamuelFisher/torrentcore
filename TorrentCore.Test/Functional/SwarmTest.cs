// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using NUnit.Framework;
using TorrentCore.Data;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Test.Functional;

[TestFixture(Category = "Functional")]
public class SwarmTest
{
    [Test]
    public async Task Test()
    {
        // Create a 256kB file
        var fileData = Enumerable.Repeat(Enumerable.Range(0, 255).Select(x => (byte)x), 1000)
                                 .SelectMany(x => x).ToArray();

        var metainfo = new MetainfoBuilder("Test")
            .AddFile("file.dat", fileData)
            .Build();

        var tracker = new MockTracker();

        var sourceFiles = new MemoryFileHandler("file.dat", fileData);

        var seed = (TorrentClient)TorrentClient.Create();
        var seedDownload = seed.Add(metainfo, tracker.CreateTrackerClient(null), sourceFiles);
        tracker.RegisterPeer(((TcpTransportProtocol)seed.Transport).Port);

        var peer = (TorrentClient)TorrentClient.Create();
        var peerFileHandler = new MemoryFileHandler();
        var peerDownload = peer.Add(metainfo, tracker.CreateTrackerClient(null), peerFileHandler);

        seedDownload.Start();
        peerDownload.Start();

        try
        {
            await peerDownload.WaitForDownloadCompletionAsync();

            // Verify downloaded data
            var resultFile = peerFileHandler.GetFileStream("file.dat") as MemoryStream;
            Assert.That(resultFile, Is.Not.Null, "Result file does not exist.");
            Assert.That(resultFile.ToArray().SequenceEqual(fileData), "Downloaded data is not correct.");
        }
        finally
        {
            seed.Dispose();
            peer.Dispose();
        }
    }
}
