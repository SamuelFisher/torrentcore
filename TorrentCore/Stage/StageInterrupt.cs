// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TorrentCore.Stage
{
    public class StageInterrupt : IStageInterrupt
    {
        private readonly ManualResetEvent pauseEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent stopEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent stateChangeEvent = new ManualResetEvent(false);

        public bool IsPauseRequested { get; private set; }

        public bool IsStopRequested { get; private set; }

        public WaitHandle PauseWaitHandle => pauseEvent;

        public WaitHandle StopWaitHandle => stopEvent;

        public WaitHandle InterruptHandle => stateChangeEvent;

        public void Pause()
        {
            IsPauseRequested = true;
            pauseEvent.Set();
            stateChangeEvent.Set();
        }

        public void Resume()
        {
            IsPauseRequested = false;
            pauseEvent.Reset();
            stateChangeEvent.Set();
        }

        public void Stop()
        {
            IsStopRequested = true;
            stopEvent.Set();
            stateChangeEvent.Set();
        }

        public void Reset()
        {
            IsStopRequested = false;
            IsPauseRequested = false;
            pauseEvent.Reset();
            stopEvent.Reset();
            stateChangeEvent.Reset();
        }
    }
}
