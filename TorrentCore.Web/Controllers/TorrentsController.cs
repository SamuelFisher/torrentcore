// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TorrentCore.Web.Controllers
{
    [Route("api/torrents")]
    public class TorrentsController : Controller
    {
        private readonly TorrentClient _client;

        public TorrentsController(TorrentClient client)
        {
            _client = client;
        }

        [HttpGet]
        public IEnumerable<object> Get()
        {
            return _client.Downloads.Select(x => new
            {
                x.Description.Name,
                InfoHash = x.Description.InfoHash.ToString(),
                Peers = x.Manager.ApplicationProtocol.Peers.Count,
                x.Progress,
                State = x.State.ToString(),
            });
        }

        [HttpGet("{infoHash}")]
        public object Details(string infoHash)
        {
            var torrent = _client.Downloads.First(x => x.Description.InfoHash.ToString() == infoHash);

            return new
            {
                infoHash,
                torrent.Description.Name,
                Size = torrent.Description.TotalSize,
                torrent.Manager.Downloaded,
                DownloadRate = torrent.Manager.DownloadRateMeasurer.AverageRate(),
                UploadRate = torrent.Manager.UploadRateMeasurer.AverageRate(),
                Peers = torrent.Manager.ApplicationProtocol.Peers.Select(p => new
                {
                    p.Address,
                    PeerId = Convert.ToBase64String(p.PeerId.Value.ToArray()),
                }),
                Pieces = torrent.Description.Pieces.Select(x => new
                {
                    x.Index,
                    x.Size,
                    Completed = torrent.Manager.ApplicationProtocol.DataHandler.CompletedPieces.Contains(x),
                }),
                torrent.Trackers,
            };
        }
    }
}
