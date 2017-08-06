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
using System.Text;
using System.Threading.Tasks;

namespace TorrentCore
{
    /// <summary>
    /// Represents the current download state of a set of files.
    /// </summary>
    [Flags]
    public enum DownloadState
    {
        Pending = 0,

        /// <summary>
        /// The downloaded files are being hashed.
        /// </summary>
        Checking = 2,

        /// <summary>
        /// The torrent is fully downloaded.
        /// </summary>
        Completed = 4,

        /// <summary>
        /// The torrent is currently downloading.
        /// </summary>
        Downloading = 8,

        /// <summary>
        /// The torrent is not currently downloading and has not finished downloading.
        /// </summary>
        Stopped = 16,

        /// <summary>
        /// There is a problem wih the torrent.
        /// </summary>
        Error = 32,

        /// <summary>
        /// The metadata is being downloaded before the download can begin.
        /// </summary>
        DownloadingMetadata = 64,
    }
}
