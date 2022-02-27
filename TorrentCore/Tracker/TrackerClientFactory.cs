// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TorrentCore.Tracker.Http;
using TorrentCore.Tracker.Udp;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Tracker;

/// <summary>
/// Default implementation of <see cref="ITrackerClientFactory"/> supporting HTTP and UDP trackers.
/// </summary>
public class TrackerClientFactory : ITrackerClientFactory
{
    private readonly LocalTcpConnectionOptions _connectionDetails;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IHttpClientFactory _httpClientFactory;

    public TrackerClientFactory(ILoggerFactory loggerFactory, IOptions<LocalTcpConnectionOptions> connectionDetails, IHttpClientFactory httpClientFactory)
    {
        _connectionDetails = connectionDetails.Value;
        _loggerFactory = loggerFactory;
        _httpClientFactory = httpClientFactory;
    }

    public ITracker? CreateTrackerClient(Uri trackerUri)
    {
        switch (trackerUri.Scheme)
        {
            case "http":
            case "https":
                return new HttpTracker(_loggerFactory.CreateLogger<HttpTracker>(), _connectionDetails, trackerUri, _httpClientFactory.CreateClient());
            case "udp":
                return new UdpTracker(_connectionDetails, trackerUri);
            default:
                // Unknown protocol
                return null;
        }
    }
}
