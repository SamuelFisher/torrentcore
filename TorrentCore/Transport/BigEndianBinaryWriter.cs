// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2016 Sam Fisher.
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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TorrentCore.Transport
{
    class BigEndianBinaryWriter : BinaryWriter
    {
        public BigEndianBinaryWriter(Stream stream)
            : base(stream)
        {
        }

        public override void Write(short value)
        {
            value = IPAddress.HostToNetworkOrder(value);
            base.Write(value);
        }

        public override void Write(ushort value)
        {
            int networkOrderInt = IPAddress.HostToNetworkOrder((int)value);
            byte[] bytes = BitConverter.GetBytes(networkOrderInt);
            base.Write(bytes, 2, 2);
        }

        public override void Write(int value)
        {
            value = IPAddress.HostToNetworkOrder(value);
            base.Write(value);
        }

        public override void Write(long value)
        {
            value = IPAddress.HostToNetworkOrder(value);
            base.Write(value);
        }
    }
}
