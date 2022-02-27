// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Application.BitTorrent;
using TorrentCore.Tracker;

namespace CustomTransportProtocol;

/// <summary>
/// Represents a tracker where peers announce by writing a file into a specified directory.
/// </summary>
class FileTracker : ITracker
{
    private readonly string _baseDir;
    private readonly PeerId _peerId;

    public FileTracker(string baseDir, PeerId peerId)
    {
        _baseDir = baseDir;
        _peerId = peerId;
    }

    public string Type => "File";

    public Task<AnnounceResult> Announce(AnnounceRequest request)
    {
        File.WriteAllText(Path.Combine(_baseDir, "announce", $"{_peerId}.txt"), _peerId.ToString());

        var peers = Directory.GetFiles(Path.Combine(_baseDir, "announce")).Where(x => Path.GetFileName(x) != $"{_peerId}.txt")
            .Select(x =>
            {
                var peerId = File.ReadAllText(x);
                return new FileTransportStream(
                    new DirectoryInfo(Path.Combine(_baseDir, _peerId.ToString(), peerId)),
                    new DirectoryInfo(Path.Combine(_baseDir, peerId, _peerId.ToString())));
            }).ToList();

        return Task.FromResult(new AnnounceResult(peers));
    }
}
