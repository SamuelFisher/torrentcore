// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Transport;

namespace TorrentCore.Application
{
    public class AcceptPeerConnectionEventArgs<TConnection> : EventArgs
    {
        public AcceptPeerConnectionEventArgs(ITransportStream ts, Func<TConnection> accept)
        {
            TransportStream = ts;
            Accept = accept;
        }

        public ITransportStream TransportStream { get; }

        public Func<TConnection> Accept { get; }
    }
}
