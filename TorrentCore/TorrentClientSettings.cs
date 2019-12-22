// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TorrentCore.Application.BitTorrent;

namespace TorrentCore
{
    public sealed class TorrentClientSettings
    {
        public TorrentClientSettings()
        {
            PeerId = PeerId.CreateNew();
            ListenPort = 6881;
            AdapterAddress = IPAddress.Any;
            UseUPnP = false;
        }

        /// <summary>
        /// Gets or sets the Peer ID for the local client.
        /// </summary>
        public PeerId PeerId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use UPnP for port forwarding.
        /// </summary>
        public bool UseUPnP { get; set; }

        /// <summary>
        /// Gets or sets the port to listen for incoming connections on.
        /// </summary>
        public int ListenPort { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the next available port
        /// if the specified port is already in use.
        /// </summary>
        public bool FindAvailablePort { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the local adapter to use for connections.
        /// </summary>
        public IPAddress AdapterAddress { get; set; }
    }
}
