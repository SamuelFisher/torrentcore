// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Application.Pipelines;

public sealed class StatusUpdate
{
    public StatusUpdate(DownloadState state, double progress)
    {
        State = state;
        Progress = progress;
    }

    public DownloadState State { get; }

    public double Progress { get; }

    public override string ToString() => $"{State} ({Progress:P})";
}
