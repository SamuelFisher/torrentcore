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
using System.Net;
using System.Threading.Tasks;
using TorrentCore.Serialization;

namespace TorrentCore.Tracker.Udp
{
    class AnnounceResponseMessage : UdpTrackerResponseMessage
    {
        public AnnounceResponseMessage()
        {
            Action = MessageAction.Announce;
        }

        public int Interval { get; set; }

        public int Leechers { get; set; }

        public int Seeders { get; set; }

        public IList<AnnounceResultPeer> Peers { get; set; }

        public override void ReadFrom(BinaryReader reader)
        {
            int action = reader.ReadInt32();
            if (action != (int)Action)
                throw new InvalidDataException($"Unexpected action: {action}");

            TransactionId = reader.ReadInt32();
            Interval = reader.ReadInt32();
            Leechers = reader.ReadInt32();
            Seeders = reader.ReadInt32();

            Peers = new List<AnnounceResultPeer>();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var endPoint = reader.ReadIpV4EndPoint();
                Peers.Add(new AnnounceResultPeer(endPoint.Address, endPoint.Port));
            }
        }
    }
}
