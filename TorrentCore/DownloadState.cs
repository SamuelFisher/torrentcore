// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentCore
{
    /// <summary>
    /// Represents the current download state of a set of files.
    /// </summary>
    public enum DownloadState
    {
        Pending,

        DownloadingMetadata,

        /// <summary>
        /// The downloaded files are being hashed.
        /// </summary>
        Checking,

        /// <summary>
        /// The torrent is currently downloading.
        /// </summary>
        Downloading,

        /// <summary>
        /// The torrent is fully downloaded.
        /// </summary>
        Seeding,

        /// <summary>
        /// The torrent is not currently active.
        /// </summary>
        Stopped,

        /// <summary>
        /// There is a problem wih the torrent.
        /// </summary>
        Error,
    }
}
