// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Data;
using TorrentCore.Data.Pieces;
using TorrentCore.Transport;

namespace TorrentCore.Modularity
{
    public interface ITorrentContext
    {
        /// <summary>
        /// Gets the metainfo for the torrent.
        /// </summary>
        Metainfo Metainfo { get; }

        /// <summary>
        /// Gets the collection of currently connected peers.
        /// </summary>
        IReadOnlyCollection<PeerConnection> Peers { get; }

        /// <summary>
        /// Gets the handler providing access to downloaded pieces and block data.
        /// </summary>
        IPieceDataHandler DataHandler { get; }

        /// <summary>
        /// Gets details on outstanding block requests.
        /// </summary>
        IBlockRequests BlockRequests { get; }

        /// <summary>
        /// Notifies that new peers are available to connect to.
        /// </summary>
        /// <param name="peers">The new peers.</param>
        void PeersAvailable(IEnumerable<ITransportStream> peers);
    }
}
