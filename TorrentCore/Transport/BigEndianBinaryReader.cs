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
    class BigEndianBinaryReader : BinaryReader
    {
        public BigEndianBinaryReader(Stream stream)
            : base(stream)
        {
        }

        public override short ReadInt16()
        {
            short value = base.ReadInt16();
            return IPAddress.NetworkToHostOrder(value);
        }

        public override int ReadInt32()
        {
            int value = base.ReadInt32();
            return IPAddress.NetworkToHostOrder(value);
        }

        public override long ReadInt64()
        {
            long value = base.ReadInt64();
            return IPAddress.NetworkToHostOrder(value);
        }
    }
}
