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
    class ConnectionRequestMessage : UdpTrackerRequestMessage
    {
        public ConnectionRequestMessage()
        {
            Action = MessageAction.Connect;
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(ConnectionId);
            writer.Write((int)Action);
            writer.Write(TransactionId);
        }
    }
}
