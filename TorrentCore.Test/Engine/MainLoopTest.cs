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
using NUnit.Framework;
using TorrentCore.Engine;

namespace TorrentCore.Test.Engine
{
    [TestFixture]
    public class MainLoopTest
    {
        private IMainLoop _mainLoop;

        [SetUp]
        public void Setup()
        {
            _mainLoop = new MainLoop();
            _mainLoop.Start();
        }

        [Test]
        public void RunTask()
        {
            int invoked = 0;
            _mainLoop.AddTask(() => invoked++);

            Task.Delay(TimeSpan.FromMilliseconds(200)).Wait();

            Assert.That(invoked, Is.EqualTo(1));
        }

        [Test]
        public void IsRunning()
        {
            Assert.That(_mainLoop.IsRunning);
        }

        [Test]
        public void Stop()
        {
            _mainLoop.Stop();
            Assert.That(_mainLoop.IsRunning, Is.False);
        }

        [TearDown]
        public void TearDown()
        {
            _mainLoop.Stop();
        }
    }
}
