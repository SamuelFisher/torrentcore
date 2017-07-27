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
using System.Net;
using System.Threading.Tasks;

namespace TorrentCore
{
    public sealed class TorrentClientSettings
    {
        public TorrentClientSettings()
        {
            ListenPort = 6881;
            AdapterAddress = IPAddress.Any;
        }

        /// <summary>
        /// Gets or sets the port to listen for incoming connections on.
        /// </summary>
        public int ListenPort { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the local adapter to use for connections.
        /// </summary>
        public IPAddress AdapterAddress { get; set; }
    }
}
