// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2016 Sam Fisher.
// 
// TorrentCore is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, version 3.
// 
// TorrentCore is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with TorrentCore.  If not, see <http://www.gnu.org/licenses/>.

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
