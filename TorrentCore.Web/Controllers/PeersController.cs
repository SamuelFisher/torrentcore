// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Transport;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Web.Controllers
{
    [Route("api/[controller]")]
    public class PeersController : Controller
    {
        private readonly TorrentClient _client;

        public PeersController(TorrentClient client)
        {
            _client = client;
        }

        [HttpGet]
        public IList<PeerDetails> Get()
        {
            return _client.Downloads.SelectMany(x => x.Manager.ApplicationProtocol.Peers).Select(x => new PeerDetails((BitTorrentPeer)x)).ToList();
        }

        [HttpGet("{peerId}")]
        public PeerDetails GetPeer(string peerId)
        {
            return Get().First(x => x.PeerId == peerId);
        }

        public class PeerDetails
        {
            internal PeerDetails(BitTorrentPeer x)
            {
                Address = x.Address;
                PeerId = Convert.ToBase64String(x.PeerId.Value.ToArray());
                Client = x.PeerId.ClientName;
                ClientVersion = x.PeerId.ClientVersion;
                string ipAddress = Address.Split(':').First();
                SupportedExtensions = GetProtocolExtensions(x.SupportedExtensions);

                try
                {
                    var hostEntry = Dns.GetHostEntryAsync(ipAddress).Result;
                    Host = hostEntry.HostName;
                }
                catch
                {
                    Host = ipAddress;
                }
            }

            public string Address { get; }

            public string Host { get; }

            public string PeerId { get; }

            public string Client { get; }

            public int? ClientVersion { get; }

            public IReadOnlyList<string> SupportedExtensions { get; }

            private List<string> GetProtocolExtensions(ProtocolExtension extensions)
            {
                var results = new List<string>();
                foreach (ProtocolExtension e in Enum.GetValues(typeof(ProtocolExtension)))
                {
                    if ((extensions & e) != 0)
                        results.Add(e.ToString());
                }
                return results;
            }
        }
    }
}
