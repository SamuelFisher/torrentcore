// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2017 Sam Fisher.
// 
// TorrentCore is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, version 3.
// 
// TorrentCore is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with TorrentCore.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentCore.Application.BitTorrent
{
    static class ProtocolExtensions
    {
        public static ProtocolExtension DetermineSupportedProcotolExtensions(byte[] reserved)
        {
            var protocols = ProtocolExtension.None;

            if ((reserved[7] & 0x04) != 0)
                protocols |= ProtocolExtension.FastPeers;

            if ((reserved[7] & 0x01) != 0)
                protocols |= ProtocolExtension.Dht;

            if ((reserved[5] & 0x10) != 0)
                protocols |= ProtocolExtension.ExtensionProtocol;

            return protocols;
        }
    }

    [Flags]
    public enum ProtocolExtension
    {
        None = 0,

        /// <summary>
        /// BEP 6 Fast Extension.
        /// </summary>
        FastPeers = 1,

        /// <summary>
        /// BEP 5 Distributed Hash Table.
        /// </summary>
        Dht = 2,

        /// <summary>
        /// BEP 10 Extension Protocol.
        /// </summary>
        ExtensionProtocol = 3
    }
}
