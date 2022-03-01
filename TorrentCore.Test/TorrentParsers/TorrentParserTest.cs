// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using BencodeNET.Torrents;
using NUnit.Framework;
using TorrentCore.Data;
using TorrentParser = TorrentCore.TorrentParsers.TorrentParser;

namespace TorrentCore.Test.TorrentParsers;

[TestFixture]
public class TorrentParserTest
{
    [Test]
    public void TestParseTorrentFullPieces()
    {
        var input = new Torrent
        {
            File = new SingleFileInfo
            {
                FileName = "test.bin",
                FileSize = 128,
            },
            PieceSize = 32,
            Pieces = GetPieceData().Take(4 * Sha1Hash.Length).ToArray(),
        };

        var result = ParseTorrentFile(input);
        Assert.That(result.Pieces, Has.Count.EqualTo(4));
    }

    [Test]
    public void TestParseTorrentPartialPieces()
    {
        var input = new Torrent
        {
            File = new SingleFileInfo
            {
                FileName = "text.bin",
                FileSize = 127,
            },
            PieceSize = 32,
            Pieces = GetPieceData().Take(4 * Sha1Hash.Length).ToArray(),
        };

        var result = ParseTorrentFile(input);
        Assert.That(result.Pieces, Has.Count.EqualTo(4));
    }

    private static Metainfo ParseTorrentFile(Torrent input)
    {
        using var ms = new MemoryStream();
        input.EncodeTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return TorrentParser.ReadFromStream(ms);
    }

    private static IEnumerable<byte> GetPieceData()
    {
        byte value = 0;
        while (true)
        {
            yield return value;

            if (value == byte.MaxValue)
            {
                value = 0;
            }
            else
            {
                value++;
            }
        }
    }
}
