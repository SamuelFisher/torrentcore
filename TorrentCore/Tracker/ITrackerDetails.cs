// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentCore.Tracker
{
    public interface ITrackerDetails
    {
        /// <summary>
        /// Gets the URI of this tracker.
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// Gets the number of peers obtained from this tracker.
        /// </summary>
        int Peers { get; }

        /// <summary>
        /// Gets the time of the last announce to this tracker.
        /// </summary>
        DateTime? LastAnnounce { get; }

        /// <summary>
        /// Gets the type of this tracker.
        /// </summary>
        string Type { get; }
    }
}
