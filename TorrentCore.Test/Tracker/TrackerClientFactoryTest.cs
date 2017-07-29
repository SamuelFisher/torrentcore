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
using System.Text;
using NUnit.Framework;
using TorrentCore.Tracker;
using TorrentCore.Tracker.Http;
using TorrentCore.Tracker.Udp;

namespace TorrentCore.Test.Tracker
{
    [TestFixture]
    public class TrackerClientFactoryTest
    {
        private readonly TrackerClientFactory factory = new TrackerClientFactory();

        [Test]
        public void HttpTracker()
        {
            var tracker = factory.CreateTrackerClient(new Uri("http://example.com:8000/announce"));
            Assert.That(tracker, Is.InstanceOf<HttpTracker>());
        }

        [Test]
        public void UdpTracker()
        {
            var tracker = factory.CreateTrackerClient(new Uri("udp://example.com:8000/announce"));
            Assert.That(tracker, Is.InstanceOf<UdpTracker>());
        }
    }
}
