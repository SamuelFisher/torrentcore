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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TorrentCore.Application.BitTorrent;

namespace TorrentCore.Web.Controllers
{
    [Route("api/torrents")]
    public class TorrentsController : Controller
    {
        private readonly TorrentClient client;

        public TorrentsController(TorrentClient client)
        {
            this.client = client;
        }

        [HttpGet]
        public IEnumerable<object> Get()
        {
            return client.Downloads.Select(x => new
            {
                x.Description.Name,
                InfoHash = x.Description.InfoHash.ToString(),
                Peers = (x.Manager.ApplicationProtocol as BitTorrentApplicationProtocol).Peers.Count,
                x.Progress,
                State = x.State.ToString()
            });
        }

        [HttpGet("{infoHash}")]
        public object Details(string infoHash)
        {
            var torrent = client.Downloads.First(x => x.Description.InfoHash.ToString() == infoHash);

            return new
            {
                infoHash,
                torrent.Description.Name,
                Size = torrent.Description.TotalSize,
                torrent.Manager.Downloaded,
                DownloadRate = torrent.Manager.DownloadRateMeasurer.AverageRate(),
                UploadRate = torrent.Manager.UploadRateMeasurer.AverageRate(),
                Peers = (torrent.Manager.ApplicationProtocol as BitTorrentApplicationProtocol).Peers.Select(p => new
                {
                    p.Address
                }),
                Pieces = torrent.Description.Pieces.Select(x => new
                {
                    x.Index,
                    x.Size,
                    Completed = torrent.Manager.CompletedPieces.Contains(x)
                }),
                BlockRequests = (torrent.Manager.ApplicationProtocol as BitTorrentApplicationProtocol).OutstandingBlockRequests.Select(x => new
                {
                    x.PieceIndex,
                    x.Offset,
                    x.Length
                })
            };
        }
    }
}
