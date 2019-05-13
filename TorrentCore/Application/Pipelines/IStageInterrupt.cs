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
