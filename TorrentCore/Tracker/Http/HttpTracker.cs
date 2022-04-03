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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using Microsoft.Extensions.Logging;
using TorrentCore.Transport;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Tracker.Http
{
    /// <summary>
    /// Manages the communication with a remote tracker.
    /// </summary>
    class HttpTracker : ITracker
    {
        private readonly ILogger<HttpTracker> _logger;
        private readonly LocalTcpConnectionOptions _tcpConnectionDetails;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTracker"/> class,
        /// with the remote tracker at the specified URL.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="tcpConnectionDetails">Provides details on which port and local address to use.</param>
        /// <param name="baseUrl">URL of the remote tracker.</param>
        public HttpTracker(ILogger<HttpTracker> logger, LocalTcpConnectionOptions tcpConnectionDetails, Uri baseUrl)
        {
            _logger = logger;
            _tcpConnectionDetails = tcpConnectionDetails;
            BaseUrl = baseUrl;
        }

        public string Type => "http";

        /// <summary>
        /// Gets the base URL for the remote tracker.
        /// </summary>
        public Uri BaseUrl { get; }

        /// <summary>
        /// Sends the specified announce request to the tracker.
        /// </summary>
        /// <param name="request">The request to send.</param>
        public virtual async Task<AnnounceResult> Announce(AnnounceRequest request)
        {
            _logger.LogInformation($"Announcing to {BaseUrl}");

            var response = await SendRequest(request);
            return ProcessResponse(response);
        }

        private async Task<Stream> SendRequest(AnnounceRequest request)
        {
            var encodedPeerId = WebUtility.UrlEncodeToBytes(request.PeerId.Value.ToArray(), 0, request.PeerId.Value.Count);

            // Prepare query
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append("?event=started");
            if (_tcpConnectionDetails.PublicAddress != null)
                queryBuilder.Append(string.Format("&ip={0}", _tcpConnectionDetails.PublicAddress));
            queryBuilder.Append(string.Format("&port={0}", _tcpConnectionDetails.Port)); // TODO: use public port
            queryBuilder.Append(string.Format("&peer_id={0}", Encoding.UTF8.GetString(encodedPeerId)));
            queryBuilder.Append(string.Format("&left={0}", request.Remaining));
            queryBuilder.Append(string.Format("&uploaded={0}", request.Uploaded));
            queryBuilder.Append(string.Format("&downloaded={0}", request.Downloaded));
            queryBuilder.Append(string.Format("&compact=1", 0));
            queryBuilder.Append("&info_hash=" + Encoding.UTF8.GetString(WebUtility.UrlEncodeToBytes(request.InfoHash.Value, 0, request.InfoHash.Value.Length)));

            return await HttpGet(BaseUrl.AbsoluteUri + queryBuilder);
        }

        protected virtual async Task<Stream> HttpGet(string requestUri)
        {
            using (var client = new HttpClient())
            {
                // Send query
                var result = await client.GetAsync(requestUri);
                return await result.Content.ReadAsStreamAsync();
            }
        }

        private AnnounceResult ProcessResponse(Stream response)
        {
            _logger.LogInformation($"Received response from {BaseUrl}");

            var resultPeers = new List<AnnounceResultPeer>();

            var parser = new BencodeParser();

            var dict = parser.Parse<BDictionary>(response);

            if (dict["peers"] is BList)
            {
                var peers = dict["peers"] as BList;
                foreach (BDictionary peer in peers)
                {
                    string ip = (peer["ip"] as BString).ToString();
                    int port = (int)(peer["port"] as BNumber).Value;

                    resultPeers.Add(new AnnounceResultPeer(IPAddress.Parse(ip), port));
                }
            }
            else
            {
                // Compact response
                var peersString = dict["peers"] as BString;
                using (var peersStream = new MemoryStream(peersString.Value.ToArray()))
                {
                    var reader = new BinaryReader(peersStream);
                    for (int i = 0; i < peersStream.Length; i += 6)
                    {
                        byte[] peerIp = reader.ReadBytes(4);
                        byte firstB = reader.ReadByte();
                        byte secondB = reader.ReadByte();

                        // TODO fix endian encoding (assumes host is little endian)
                        ushort port = BitConverter.ToUInt16(new[] { secondB, firstB }, 0);

                        string ip = $"{peerIp[0]}.{peerIp[1]}.{peerIp[2]}.{peerIp[3]}";
                        resultPeers.Add(new AnnounceResultPeer(IPAddress.Parse(ip), port));
                    }
                }
            }

            return new AnnounceResult(resultPeers.Select(x => new TcpTransportStream(_tcpConnectionDetails.BindAddress, x.IPAddress, x.Port)));
        }
    }
}
