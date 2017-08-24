// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorrentCore.Data;

namespace TorrentCore.Engine
{
    /// <summary>
    /// Manages the download of a torrent.
    /// </summary>
    interface ITorrentDownloadManager
    {
        Metainfo Description { get; }

        /// <summary>
        /// Gets the current state of the download.
        /// </summary>
        DownloadState State { get; }
    }
}
