﻿// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentCore.Stage
{
    public sealed class StatusUpdate
    {
        public StatusUpdate(DownloadState state, double progress)
        {
            State = state;
            Progress = progress;
        }

        public DownloadState State { get; }

        public double Progress { get; }
    }
}
