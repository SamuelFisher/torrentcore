// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent
{
    class TransportStreamAddressEqualityComparer : IEqualityComparer<ITransportStream>
    {
        public bool Equals(ITransportStream x, ITransportStream y)
        {
            return x.Address.Equals(y.Address);
        }

        public int GetHashCode(ITransportStream obj)
        {
            return obj.Address.GetHashCode();
        }
    }
}
