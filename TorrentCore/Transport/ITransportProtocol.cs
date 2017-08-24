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
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Data;
using TorrentCore.Tracker;

namespace TorrentCore.Transport
{
    interface ITransportProtocol
    {
        /// <summary>
        /// Gets a collection of the active transport streams.
        /// </summary>
        IEnumerable<ITransportStream> Streams { get; }

        /// <summary>
        /// Starts the transport protocol.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the transport protocol.
        /// </summary>
        void Stop();
    }
}
