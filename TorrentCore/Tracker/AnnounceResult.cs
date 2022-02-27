﻿// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Transport;

namespace TorrentCore.Tracker;

public class AnnounceResult
{
    public AnnounceResult(IEnumerable<ITransportStream> peers)
    {
        Peers = peers.ToArray();
    }

    public IReadOnlyList<ITransportStream> Peers { get; }
}
