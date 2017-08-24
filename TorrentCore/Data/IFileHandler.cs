// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentCore.Data
{
    /// <summary>
    /// Provides access to files as streams.
    /// </summary>
    public interface IFileHandler : IDisposable
    {
        Stream GetFileStream(string fileName);

        void CloseFileStream(Stream file);

        void Flush();
    }
}
