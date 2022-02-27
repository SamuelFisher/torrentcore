// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using Moq;
using NUnit.Framework;
using TorrentCore.Data;

namespace TorrentCore.Test.Data;

[TestFixture]
public class BlockDataHandlerTest
{
    private MemoryStream _file1Stream;
    private MemoryStream _file2Stream;
    private BlockDataHandler _blockDataHandler;

    [SetUp]
    public void Setup()
    {
        var fileHandler = new Mock<IFileHandler>();

        _file1Stream = new MemoryStream();
        fileHandler.Setup(x => x.GetFileStream("File1.txt"))
                   .Returns(_file1Stream);

        _file2Stream = new MemoryStream();
        fileHandler.Setup(x => x.GetFileStream("File2.txt"))
                   .Returns(_file2Stream);

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

        _blockDataHandler = new BlockDataHandler(fileHandler.Object, metainfo);
    }

    [Test]
    public void ReadBlockData()
    {
        _file1Stream.Write(Enumerable.Range(0, 100).Select(x => (byte)x).ToArray(), 0, 100);
        _file2Stream.Write(Enumerable.Range(100, 50).Select(x => (byte)x).ToArray(), 0, 50);

        var read = _blockDataHandler.ReadBlockData(0, 150);

        CollectionAssert.AreEqual(Enumerable.Range(0, 150).Select(x => (byte)x).ToArray(), read);
    }

    [Test]
    public void WriteBlockData()
    {
        _blockDataHandler.WriteBlockData(0, Enumerable.Range(0, 150).Select(x => (byte)x).ToArray());

        // Check contents of file 1
        CollectionAssert.AreEqual(Enumerable.Range(0, 100).Select(x => (byte)x).ToArray(),
                                  _file1Stream.ToArray());

        // Check contents of file 2
        CollectionAssert.AreEqual(Enumerable.Range(100, 50).Select(x => (byte)x).ToArray(),
                                  _file2Stream.ToArray());
    }
}
