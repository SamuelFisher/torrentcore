// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Transport;

namespace TorrentCore.Application;

public class AcceptPeerConnectionEventArgs : EventArgs
{
    public AcceptPeerConnectionEventArgs(ITransportStream ts, Func<IPeer> accept)
    {
        TransportStream = ts;
        Accept = accept;
    }

    public ITransportStream TransportStream { get; }

    public Func<IPeer> Accept { get; }
}
