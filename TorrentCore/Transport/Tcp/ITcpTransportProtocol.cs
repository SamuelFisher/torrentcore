// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TorrentCore.Transport.Tcp
{
    public interface ITcpTransportProtocol : ITransportProtocol
    {
        ITransportStream CreateTransportStream(IPAddress remoteAddress, int port);
    }
}
