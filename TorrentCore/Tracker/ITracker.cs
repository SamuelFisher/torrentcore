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
using TorrentCore.Transport;

namespace TorrentCore.Tracker
{
    /// <summary>
    /// Manages the communication with a remote tracker.
    /// </summary>
    public interface ITracker
    {
        /// <summary>
        /// Sends the specified announce request to the tracker.
        /// </summary>
        /// <param name="request">The request to send.</param>
        Task<AnnounceResult> Announce(AnnounceRequest request);

        /// <summary>
        /// Gets the type of this tracker.
        /// </summary>
        string Type { get; }
    }
}
