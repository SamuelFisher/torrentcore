// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Tracker;

/// <summary>
/// Given the URI of a tracker, creates an appropriate client for contacting it.
/// </summary>
public interface ITrackerClientFactory
{
    /// <summary>
    /// Creates a client for communicating with the tracker at the supplied URI.
    /// </summary>
    /// <param name="trackerUri">URI of the tracker.</param>
    /// <returns>A client object for communicating with the tracker, or null if the uri is not supported.</returns>
    ITracker? CreateTrackerClient(Uri trackerUri);
}
