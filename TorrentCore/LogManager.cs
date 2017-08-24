// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TorrentCore
{
    public static class LogManager
    {
        private static readonly ILoggerFactory LoggerFactory = new LoggerFactory();

        public static void Configure(Action<ILoggerFactory> factory)
        {
            factory(LoggerFactory);
        }

        public static ILogger<T> GetLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
    }
}
