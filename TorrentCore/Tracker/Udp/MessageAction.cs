// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Tracker.Udp;

enum MessageAction
{
    Connect = 0,
    Announce = 1,
    Error = 3,
}
