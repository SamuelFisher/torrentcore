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
using System.Net;
using System.Threading.Tasks;
using TorrentCore.Data;

namespace TorrentCore.Tracker.Udp
{
    class AnnounceRequestMessage : UdpTrackerRequestMessage
    {
        public AnnounceRequestMessage()
        {
            Action = MessageAction.Announce;
        }

        public Sha1Hash InfoHash { get; set; }
        public byte[] PeerId { get; set; }
        public long Downloaded { get; set; }
        public long LeftToDownload { get; set; }
        public long Uploaded { get; set; }
        public EventType Event { get; set; }
        public IPAddress IPAddress { get; set; }
        public int Key { get; set; }
        public int NumWant { get; set; }
        public ushort Port { get; set; }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(ConnectionId);
            writer.Write((int)Action);
            writer.Write(TransactionId);
            writer.Write(InfoHash.Value);
            writer.Write(PeerId);
            writer.Write(Downloaded);
            writer.Write(LeftToDownload);
            writer.Write(Uploaded);
            writer.Write((int)Event);
            writer.Write(IPAddress.GetAddressBytes());
            writer.Write(Key);
            writer.Write(NumWant);
            writer.Write(Port);
        }

        public enum EventType
        {
            None = 0,
            Completed = 1,
            Started = 2,
            Stopped = 3
        }
    }
}
