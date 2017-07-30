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
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TorrentCore.Data;

namespace TorrentCore.Test.Data
{
    [TestFixture]
    public class PieceCheckerHandlerTest
    {
        private List<Tuple<long, byte[]>> writtenData;
        private PieceCheckerHandler pieceChecker;

        [SetUp]
        public void Setup()
        {
            writtenData = new List<Tuple<long, byte[]>>();

            Sha1Hash hash;
            using (var sha1 = SHA1.Create())
                hash = new Sha1Hash(sha1.ComputeHash(Enumerable.Repeat((byte)0, 50).ToArray()));

            var metainfo = new Metainfo("test",
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
                                        null);

            var baseHandler = new Mock<IBlockDataHandler>();
            baseHandler.Setup(x => x.Metainfo)
                       .Returns(metainfo);
            baseHandler.Setup(x => x.WriteBlockData(It.IsAny<long>(),
                                                    It.IsAny<byte[]>()))
                       .Callback<long, byte[]>((offset, data) => writtenData.Add(Tuple.Create(offset, data)));

            pieceChecker = new PieceCheckerHandler(baseHandler.Object);
        }

        [Test]
        public void WriteCompleted()
        {
            var completed = new List<Piece>();

            pieceChecker.PieceCompleted += args => { completed.Add(args.Piece); };

            pieceChecker.PieceCorrupted += args => { Assert.Fail("Piece marked as corrupted."); };

            var data = Enumerable.Repeat((byte)0, 50).ToArray();
            pieceChecker.WriteBlockData(0, data);

            Assert.That(completed, Has.Count.EqualTo(1));
            Assert.That(completed[0].Index, Is.EqualTo(0));
            Assert.That(completed[0].Size, Is.EqualTo(50));

            Assert.That(writtenData, Has.Count.EqualTo(1));
            Assert.That(writtenData[0].Item1, Is.EqualTo(0));
            CollectionAssert.AreEqual(data, writtenData[0].Item2);
        }

        [Test]
        public void WriteCorrupted()
        {
            var corrupted = new List<Piece>();

            pieceChecker.PieceCompleted += args => { Assert.Fail("Piece marked as completed."); };

            pieceChecker.PieceCorrupted += args => { corrupted.Add(args.Piece); };

            pieceChecker.WriteBlockData(0, Enumerable.Repeat((byte)1, 50).ToArray());

            Assert.That(corrupted, Has.Count.EqualTo(1));
            Assert.That(corrupted[0].Index, Is.EqualTo(0));
            Assert.That(corrupted[0].Size, Is.EqualTo(50));

            Assert.That(writtenData, Is.Empty);
        }
    }
}
