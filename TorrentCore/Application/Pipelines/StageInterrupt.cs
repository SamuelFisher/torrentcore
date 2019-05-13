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

namespace TorrentCore.Application.Pipelines
{
    public class StageInterrupt : IStageInterrupt
    {
        private readonly ManualResetEvent _pauseEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent _stateChangeEvent = new ManualResetEvent(false);

        public bool IsPauseRequested { get; private set; }

        public bool IsStopRequested { get; private set; }

        public WaitHandle PauseWaitHandle => _pauseEvent;

        public WaitHandle StopWaitHandle => _stopEvent;

        public WaitHandle InterruptHandle => _stateChangeEvent;

        public void Pause()
        {
            IsPauseRequested = true;
            _pauseEvent.Set();
            _stateChangeEvent.Set();
        }

        public void Resume()
        {
            IsPauseRequested = false;
            _pauseEvent.Reset();
            _stateChangeEvent.Set();
        }

        public void Stop()
        {
            IsStopRequested = true;
            _stopEvent.Set();
            _stateChangeEvent.Set();
        }

        public void Reset()
        {
            IsStopRequested = false;
            IsPauseRequested = false;
            _pauseEvent.Reset();
            _stopEvent.Reset();
            _stateChangeEvent.Reset();
        }
    }
}
