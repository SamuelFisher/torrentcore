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

namespace TorrentCore.Tracker
{
    /// <summary>
    /// Manages the communication with a remote tracker.
    /// </summary>
    public interface ITracker
    {
        /// <summary>
        /// Gets the type of this tracker.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Sends the specified announce request to the tracker.
        /// </summary>
        /// <param name="request">The request to send.</param>
        Task<AnnounceResult> Announce(AnnounceRequest request);
    }
}
