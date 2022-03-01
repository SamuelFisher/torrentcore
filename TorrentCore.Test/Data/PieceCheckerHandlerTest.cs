// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using TorrentCore.Data;

namespace TorrentCore.Test.Data;

[TestFixture]
public class PieceCheckerHandlerTest
{
    private List<Tuple<long, byte[]>> _writtenData;
    private PieceCheckerHandler _pieceChecker;

    [SetUp]
    public void Setup()
    {
        _writtenData = new List<Tuple<long, byte[]>>();

        Sha1Hash hash;
        using (var sha1 = SHA1.Create())
            hash = new Sha1Hash(sha1.ComputeHash(Enumerable.Repeat((byte)0, 50).ToArray()));

        var metainfo = new Metainfo(
            "test",
            Sha1Hash.Empty,
            new[]
            {
                    new ContainedFile("File1.txt", 100),
            },
            new[]
            {
                    new Piece(0, 50, hash),
                    new Piece(50, 50, hash),
            },
            new IEnumerable<Uri>[0],
            new byte[0]);

        var baseHandler = new Mock<IBlockDataHandler>();
        baseHandler.Setup(x => x.Metainfo)
                   .Returns(metainfo);
        baseHandler.Setup(x => x.WriteBlockData(
            It.IsAny<long>(),
            It.IsAny<byte[]>()))
                   .Callback<long, byte[]>((offset, data) => _writtenData.Add(Tuple.Create(offset, data)));

        _pieceChecker = new PieceCheckerHandler(new NullLogger<PieceCheckerHandler>(), baseHandler.Object);
    }

    [Test]
    public void WriteCompleted()
    {
        var completed = new List<Piece>();

        _pieceChecker.PieceCompleted += args => { completed.Add(args); };

        _pieceChecker.PieceCorrupted += args => { Assert.Fail("Piece marked as corrupted."); };

        var data = Enumerable.Repeat((byte)0, 50).ToArray();
        _pieceChecker.WriteBlockData(0, data);

        Assert.That(completed, Has.Count.EqualTo(1));
        Assert.That(completed[0].Index, Is.EqualTo(0));
        Assert.That(completed[0].Size, Is.EqualTo(50));

        Assert.That(_writtenData, Has.Count.EqualTo(1));
        Assert.That(_writtenData[0].Item1, Is.EqualTo(0));
        CollectionAssert.AreEqual(data, _writtenData[0].Item2);
    }

    [Test]
    public void WriteCorrupted()
    {
        var corrupted = new List<Piece>();

        _pieceChecker.PieceCompleted += args => { Assert.Fail("Piece marked as completed."); };

        _pieceChecker.PieceCorrupted += args => { corrupted.Add(args); };

        _pieceChecker.WriteBlockData(0, Enumerable.Repeat((byte)1, 50).ToArray());

        Assert.That(corrupted, Has.Count.EqualTo(1));
        Assert.That(corrupted[0].Index, Is.EqualTo(0));
        Assert.That(corrupted[0].Size, Is.EqualTo(50));

        Assert.That(_writtenData, Is.Empty);
    }
}
