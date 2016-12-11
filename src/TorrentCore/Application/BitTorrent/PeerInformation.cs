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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Data;

namespace TorrentCore.Application.BitTorrent
{
    class PeerInformation
    {
        public bool IsRemotePeerInterested { get; set; }

        public bool IsInterestedInRemotePeer { get; set; }

        public bool IsChokedByRemotePeer { get; set; }

        public bool IsChokingRemotePeer { get; set; }

        public Bitfield Available { get; set; }

        public HashSet<BlockRequest> RequestedByRemotePeer { get; private set; }

        public HashSet<BlockRequest> Requested { get; private set; }

        public PeerInformation(Metainfo meta)
        {
            Available = new Bitfield(meta.Pieces.Count);
            RequestedByRemotePeer = new HashSet<BlockRequest>();
            Requested = new HashSet<BlockRequest>();

            IsRemotePeerInterested = false;
            IsInterestedInRemotePeer = false;
            IsChokedByRemotePeer = true;
            IsChokingRemotePeer = true;
        }
    }
}
