// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TorrentCore.Tracker.Udp
{
    abstract class UdpTrackerRequestMessage
    {
        public long ConnectionId { get; set; }

        protected MessageAction Action { get; set; }

        public int TransactionId { get; set; }

        public abstract void WriteTo(BinaryWriter writer);
    }
}
