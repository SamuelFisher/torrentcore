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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TorrentCore.Data;

namespace TorrentCore.Test.Data
{
    [TestFixture]
    public class BlockDataHandlerTest
    {
        private MemoryStream file1Stream;
        private MemoryStream file2Stream;
        private BlockDataHandler blockDataHandler;

        [SetUp]
        public void Setup()
        {
            var fileHandler = new Mock<IFileHandler>();

            file1Stream = new MemoryStream();
            fileHandler.Setup(x => x.GetFileStream("File1.txt"))
                       .Returns(file1Stream);

            file2Stream = new MemoryStream();
            fileHandler.Setup(x => x.GetFileStream("File2.txt"))
                       .Returns(file2Stream);

            var metainfo = new Metainfo("test",
                                        Sha1Hash.Empty,
                                        new[]
                                        {
                                            new ContainedFile("File1.txt", 100),
                                            new ContainedFile("File2.txt", 50),
                                        },
                                        new[]
                                        {
                                            new Piece(0, 50, Sha1Hash.Empty),
                                            new Piece(50, 50, Sha1Hash.Empty),
                                            new Piece(100, 50, Sha1Hash.Empty),
                                        },
                                        new IEnumerable<Uri>[0],
                                        new byte[0]);

            blockDataHandler = new BlockDataHandler(fileHandler.Object, metainfo);
        }

        [Test]
        public void ReadBlockData()
        {
            file1Stream.Write(Enumerable.Range(0, 100).Select(x => (byte)x).ToArray(), 0, 100);
            file2Stream.Write(Enumerable.Range(100, 50).Select(x => (byte)x).ToArray(), 0, 50);

            var read = blockDataHandler.ReadBlockData(0, 150);

            CollectionAssert.AreEqual(Enumerable.Range(0, 150).Select(x => (byte)x).ToArray(), read);
        }

        [Test]
        public void WriteBlockData()
        {
            blockDataHandler.WriteBlockData(0, Enumerable.Range(0, 150).Select(x => (byte)x).ToArray());

            // Check contents of file 1
            CollectionAssert.AreEqual(Enumerable.Range(0, 100).Select(x => (byte)x).ToArray(),
                                      file1Stream.ToArray());

            // Check contents of file 2
            CollectionAssert.AreEqual(Enumerable.Range(100, 50).Select(x => (byte)x).ToArray(),
                                      file2Stream.ToArray());
        }
    }
}
