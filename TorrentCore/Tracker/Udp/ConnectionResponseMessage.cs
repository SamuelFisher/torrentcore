// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

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
