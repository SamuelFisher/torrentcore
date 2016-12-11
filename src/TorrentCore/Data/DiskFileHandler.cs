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
    /// Provides a disk-based implementation of an IFileHandler.
    /// </summary>
    public class DiskFileHandler : IFileHandler
    {
        private readonly Dictionary<string, FileStream> openFiles;

        /// <summary>
        /// Creates a new DiskFileHandler with the specified directory.
        /// </summary>
        /// <param name="directory">Base directory for all files.</param>
        public DiskFileHandler(string directory)
        {
            Directory = directory;
            openFiles = new Dictionary<string, FileStream>();
        }

        /// <summary>
        /// Gets the base directory for files.
        /// </summary>
        public string Directory { get; }

        public Stream GetFileStream(string fileName)
        {
            string path = FullName(fileName);
            if (!File.Exists(path))
                File.WriteAllText(path, "");

            FileStream stream;
            if (!openFiles.TryGetValue(fileName, out stream))
            {
                stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                openFiles.Add(fileName, stream);
            }

            return stream;
        }

        public void CloseFileStream(Stream file)
        {
            if (!openFiles.ContainsValue((FileStream)file))
                throw new InvalidOperationException("Cannot close stream. File is not open.");

            // Remove from collection of open files
            string fileName = null;
            foreach (var stream in openFiles)
            {
                if (stream.Value == file)
                {
                    fileName = stream.Key;
                    break;
                }
            }
            openFiles.Remove(fileName);

            file.Dispose();
        }

        public void Flush()
        {
            foreach (var file in openFiles)
            {
                file.Value.Flush();
            }
        }

        public void Dispose()
        {
            foreach (var file in openFiles)
                file.Value.Dispose();
        }

        string FullName(string fileName)
        {
            return Path.Combine(Directory, fileName);
        }
    }
}
