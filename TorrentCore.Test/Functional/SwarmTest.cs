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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TorrentCore.Data;

namespace TorrentCore.Test.Functional
{
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

            var seed = new TorrentClient(0);
            var seedDownload = seed.Add(metainfo, tracker.CreateTrackerClient(null), sourceFiles);
            tracker.RegisterPeer(seed.Transport.Port);

            var peer = new TorrentClient(0);
            var peerFileHandler = new MemoryFileHandler();
            var peerDownload = peer.Add(metainfo, tracker.CreateTrackerClient(null), peerFileHandler);

            seedDownload.Start();
            peerDownload.Start();
            await peerDownload.WaitForDownloadCompletionAsync(TimeSpan.FromSeconds(10));

            seed.Dispose();
            peer.Dispose();

            // Verify downloaded data
            var resultFile = peerFileHandler.GetFileStream("file.dat") as MemoryStream;
            Assert.That(resultFile, Is.Not.Null, "Result file does not exist.");
            Assert.That(resultFile.ToArray().SequenceEqual(fileData), "Downloaded data is not correct.");
        }
    }
}
