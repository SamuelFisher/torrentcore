// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TorrentCore.Tracker.Http;
using TorrentCore.Tracker.Udp;
using TorrentCore.Transport;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Tracker
{
    /// <summary>
    /// Default implementation of <see cref="ITrackerClientFactory"/> supporting HTTP and UDP trackers.
    /// </summary>
    public class TrackerClientFactory : ITrackerClientFactory
    {
        private readonly LocalTcpConnectionOptions _connectionDetails;
        private readonly ILoggerFactory _loggerFactory;

        public TrackerClientFactory(ILoggerFactory loggerFactory, IOptions<LocalTcpConnectionOptions> connectionDetails)
        {
            _connectionDetails = connectionDetails.Value;
            _loggerFactory = loggerFactory;
        }

        public ITracker CreateTrackerClient(Uri trackerUri)
        {
            switch (trackerUri.Scheme)
            {
                case "http":
                    return new HttpTracker(_loggerFactory.CreateLogger<HttpTracker>(), _connectionDetails, trackerUri);
                case "udp":
                    return new UdpTracker(_connectionDetails, trackerUri);
                default:
                    // Unknown protocol
                    return null;
            }
        }
    }
}
