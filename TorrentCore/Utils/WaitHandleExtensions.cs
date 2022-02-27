// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Utils;

static class WaitHandleExtensions
{
    public static Task WaitOneAsync(this WaitHandle waitHandle)
    {
        if (waitHandle == null)
            throw new ArgumentNullException("waitHandle");

        var tcs = new TaskCompletionSource<bool>();
        var rwh = ThreadPool.RegisterWaitForSingleObject(
            waitHandle,
            (state, timedOut) => tcs.TrySetResult(true),
            null,
            -1,
            true);
        var t = tcs.Task;
        t.ContinueWith((antecedent) => rwh.Unregister(null));
        return t;
    }
}
