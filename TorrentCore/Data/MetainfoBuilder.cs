// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Data;

#nullable disable
public sealed class MetainfoBuilder
{
    private readonly string _name;
    private readonly IReadOnlyCollection<Tuple<string, byte[]>> _files;
    private readonly IReadOnlyCollection<Uri> _trackers;

    public MetainfoBuilder(string torrentName)
    {
        _name = torrentName;
        _files = new List<Tuple<string, byte[]>>();
        _trackers = new List<Uri>();
    }

    private MetainfoBuilder(
        IEnumerable<Tuple<string, byte[]>> files,
        IEnumerable<Uri> trackers)
    {
        _files = files.ToList();
        _trackers = trackers.ToList();
    }

    public MetainfoBuilder AddFile(string fileName, byte[] data)
    {
        return new MetainfoBuilder(_files.Concat(new[] { Tuple.Create(fileName, data) }),
                                   _trackers);
    }

    public MetainfoBuilder WithTracker(Uri trackerUri)
    {
        return new MetainfoBuilder(_files,
                                   _trackers.Concat(new[] { trackerUri }));
    }

    public Metainfo Build()
    {
        var containedFiles = _files.Select(x => new ContainedFile(x.Item1, x.Item2.Length)).ToList();
        var fileHandler = new MemoryFileHandler(_files.ToDictionary(x => x.Item1, x => x.Item2));

        var pieces = PieceCalculator.ComputePieces(containedFiles, 256000, fileHandler);
        return new Metainfo(_name, Sha1Hash.Empty, containedFiles, pieces, new[] { _trackers }, null);
    }
}
