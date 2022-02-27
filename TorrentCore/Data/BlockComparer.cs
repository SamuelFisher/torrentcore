// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Data;

internal class BlockComparer : IComparer<Block>
{
    public int Compare(Block? x, Block? y)
    {
        if (x?.Offset == y?.Offset)
            return 0;
        else if (x?.Offset < y?.Offset)
            return -1;
        else
            return 1;
    }
}
