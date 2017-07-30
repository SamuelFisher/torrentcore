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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TorrentCore.Transport;

namespace TorrentCore.Web.Controllers
{
    [Route("api/[controller]")]
    public class PeersController : Controller
    {
        private readonly TorrentClient client;

        public PeersController(TorrentClient client)
        {
            this.client = client;
        }

        [HttpGet]
        public IList<PeerDetails> Get()
        {
            return client.Transport.PeerStreams.Where(x => x.IsConnected).Select(x => new PeerDetails(x)).ToList();
        }

        public class PeerDetails
        {
            internal PeerDetails(PeerStream x)
            {
                Address = x.Address;

                var hostEntry = Dns.GetHostEntryAsync(Address.Split(':').First()).Result;
                Host = hostEntry.HostName;
            }

            public string Address { get; }
            public string Host { get; }
        }
    }
}
