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
    /// An IFileHandler that stores data in memory.
    /// </summary>
    public class MemoryFileHandler : IFileHandler
    {
        private readonly Dictionary<string, MemoryStream> files;

        public MemoryFileHandler()
        {
            files = new Dictionary<string, MemoryStream>();
        }

        public MemoryFileHandler(Dictionary<string, byte[]> existingFileData)
        {
            files = new Dictionary<string, MemoryStream>();
            foreach (var existing in existingFileData)
                files.Add(existing.Key, new MemoryStream(existing.Value));
        }

        public MemoryFileHandler(string existingFile, byte[] existingData)
        {
            files = new Dictionary<string, MemoryStream>();
            files.Add(existingFile, new MemoryStream(existingData));
        }

        public Stream GetFileStream(string fileName)
        {
            MemoryStream stream;
            if (!files.TryGetValue(fileName, out stream))
            {
                stream = new MemoryStream();
                files.Add(fileName, stream);
            }

            return stream;
        }

        public void CloseFileStream(Stream file)
        {
            // Don't close the stream or the data will be lost
        }

        public void Flush()
        {
            // Don't need to do anything
        }

        public void Dispose()
        {
            foreach (var file in files)
                file.Value.Dispose();
        }
    }
}
