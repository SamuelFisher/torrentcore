// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Modularity;

public static class PeerContextExtensions
{
    public static T GetRequiredValue<T>(this IPeerContext context, string key)
    {
        return context.GetValue<T>(key) ?? throw new InvalidOperationException();
    }
}
