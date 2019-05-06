// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Data;
using TorrentCore.Modularity;

namespace TorrentCore
{
    /// <summary>
    /// Manages the downloads for multiple torrents.
    /// </summary>
    public interface ITorrentClient : IDisposable
    {
        IReadOnlyCollection<TorrentDownload> Downloads { get; }

        PeerId LocalPeerId { get; }

        IModuleManager Modules { get; }

        TorrentDownload Add(Metainfo metainfo, string downloadDirectory);

        TorrentDownload Add(Stream torrentFileStream, string downloadDirectory);

        TorrentDownload Add(string torrentFile, string downloadDirectory);
    }
}
