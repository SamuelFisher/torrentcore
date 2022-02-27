// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Transport;

public class AcceptConnectionEventArgs
{
    public AcceptConnectionEventArgs(ITransportStream ts, Action accept)
    {
        TransportStream = ts;
        Accept = accept;
    }

    public ITransportStream TransportStream { get; }

    public Action Accept { get; }
}
