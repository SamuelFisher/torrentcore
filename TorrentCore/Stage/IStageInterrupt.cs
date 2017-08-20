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
    public interface IStageInterrupt
    {
        /// <summary>
        /// Gets a value indicating whether the stage should temporarily pause its execution such
        /// that it can continue from the same point when requested to resume.
        /// The Execute(...) method should not return to the caller.
        /// </summary>
        bool IsPauseRequested { get; }

        /// <summary>
        /// Gets a value indicating whether the stage should permanently stop execution.
        /// the Execute(...) method is expected to return to the caller shortly after a
        /// stop is requested.
        /// </summary>
        bool IsStopRequested { get; }

        /// <summary>
        /// Gets a <see cref="WaitHandle"/> that is signalled when the
        /// stage is requested to pause.
        /// </summary>
        WaitHandle PauseWaitHandle { get; }

        /// <summary>
        /// Gets a <see cref="WaitHandle"/> that is signalled when the
        /// stage is requested to stop.
        /// </summary>
        WaitHandle StopWaitHandle { get; }

        /// <summary>
        /// Gets a <see cref="WaitHandle"/> that is signalled when the
        /// requested state changes.
        /// </summary>
        WaitHandle InterruptHandle { get; }
    }
}
