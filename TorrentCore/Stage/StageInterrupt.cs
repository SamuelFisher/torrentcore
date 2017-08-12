// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2017 Samuel Fisher.
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
