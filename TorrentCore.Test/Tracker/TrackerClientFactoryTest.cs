// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using TorrentCore.Tracker;
using TorrentCore.Tracker.Http;
using TorrentCore.Tracker.Udp;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Test.Tracker;

[TestFixture]
public class TrackerClientFactoryTest
{
    private readonly TrackerClientFactory _factory =
        new TrackerClientFactory(new NullLoggerFactory(), Options.Create(new LocalTcpConnectionOptions()), Mock.Of<IHttpClientFactory>());

    [Test]
    public void HttpTracker()
    {
        var tracker = _factory.CreateTrackerClient(new Uri("http://example.com:8000/announce"));
        Assert.That(tracker, Is.InstanceOf<HttpTracker>());
    }

    [Test]
    public void UdpTracker()
    {
        var tracker = _factory.CreateTrackerClient(new Uri("udp://example.com:8000/announce"));
        Assert.That(tracker, Is.InstanceOf<UdpTracker>());
    }
}
