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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TorrentCore.Tracker.Udp
{
    class ConnectionResponseMessage : UdpTrackerResponseMessage
    {
        public ConnectionResponseMessage()
        {
            Action = MessageAction.Connect;
        }

        public long ConnectionId { get; set; }

        public override void ReadFrom(BinaryReader reader)
        {
            int action = reader.ReadInt32();
            if (action != (int)Action)
                throw new InvalidDataException($"Unexpected action: {action}");

            TransactionId = reader.ReadInt32();
            ConnectionId = reader.ReadInt64();
        }
    }
}
