// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TorrentCore.Data;

namespace TorrentCore.Modularity.MetainfoProvider
{
    /// <summary>
    /// Provides the ability to download the 'info' section of a torrent file, given the infohash.
    /// </summary>
    public interface IMetainfoProvider
    {
        Task<Metainfo> GetMetainfo(ITorrentContext context, CancellationToken ct);
    }
}
