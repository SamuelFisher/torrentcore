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
using System.Threading.Tasks;
using NUnit.Framework;
using TorrentCore.Engine;

namespace TorrentCore.Test.Engine
{
    [TestFixture]
    public class MainLoopTest
    {
        private IMainLoop mainLoop;

        [SetUp]
        public void Setup()
        {
            mainLoop = new MainLoop();
            mainLoop.Start();
        }

        [Test]
        public void RegularTask()
        {
            int invoked = 0;
            mainLoop.AddRegularTask(() => invoked++);

            Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();

            Assert.That(invoked, Is.GreaterThan(0));
        }

        [Test]
        public void RunTask()
        {
            int invoked = 0;
            mainLoop.AddTask(() => invoked++);

            Task.Delay(TimeSpan.FromMilliseconds(200)).Wait();

            Assert.That(invoked, Is.EqualTo(1));
        }

        [Test]
        public void IsRunning()
        {
            Assert.That(mainLoop.IsRunning);
        }

        [Test]
        public void Stop()
        {
            mainLoop.Stop();
            Assert.That(mainLoop.IsRunning, Is.False);
        }

        [TearDown]
        public void TearDown()
        {
            mainLoop.Stop();
        }
    }
}
