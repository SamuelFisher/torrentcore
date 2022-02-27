// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Application.BitTorrent;

namespace TorrentCore.Application;

public interface IPeer
{
    PeerId PeerId { get; }

    string Address { get; }

    Bitfield Available { get; }
}
