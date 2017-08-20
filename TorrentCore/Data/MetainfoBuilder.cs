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
using System.Linq;
using System.Text;

namespace TorrentCore.Data
{
    public sealed class MetainfoBuilder
    {
        private readonly string name;
        private readonly IReadOnlyCollection<Tuple<string, byte[]>> files;
        private readonly IReadOnlyCollection<Uri> trackers;

        public MetainfoBuilder(string torrentName)
        {
            name = torrentName;
            files = new List<Tuple<string, byte[]>>();
            trackers = new List<Uri>();
        }

        private MetainfoBuilder(IEnumerable<Tuple<string, byte[]>> files,
                                IEnumerable<Uri> trackers)
        {
            this.files = files.ToList();
            this.trackers = trackers.ToList();
        }

        public MetainfoBuilder AddFile(string fileName, byte[] data)
        {
            return new MetainfoBuilder(files.Concat(new[] { Tuple.Create(fileName, data) }),
                                       trackers);
        }

        public MetainfoBuilder WithTracker(Uri trackerUri)
        {
            return new MetainfoBuilder(files,
                                       trackers.Concat(new[] { trackerUri }));
        }

        public Metainfo Build()
        {
            var containedFiles = files.Select(x => new ContainedFile(x.Item1, x.Item2.Length)).ToList();
            var fileHandler = new MemoryFileHandler(files.ToDictionary(x => x.Item1, x => x.Item2));

            var pieces = PieceCalculator.ComputePieces(containedFiles, 256000, fileHandler);
            return new Metainfo(name, Sha1Hash.Empty, containedFiles, pieces, new[] { trackers }, null);
        }
    }
}
