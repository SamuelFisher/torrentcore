// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Engine;

/// <summary>
/// Provides methods for queueing and processing tasks.
/// </summary>
public interface IMainLoop
{
    bool IsRunning { get; }

    void Start();

    void Stop();

    void AddTask(Action t);

    IRegularTask AddRegularTask(Action t);

    IRegularTask AddRegularTask(Func<Task> t);
}
