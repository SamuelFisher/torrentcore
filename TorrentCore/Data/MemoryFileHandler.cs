// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Data;

/// <summary>
/// An IFileHandler that stores data in memory.
/// </summary>
public class MemoryFileHandler : IFileHandler
{
    private readonly Dictionary<string, MemoryStream> _files;

    public MemoryFileHandler()
    {
        _files = new Dictionary<string, MemoryStream>();
    }

    public MemoryFileHandler(Dictionary<string, byte[]> existingFileData)
    {
        _files = new Dictionary<string, MemoryStream>();
        foreach (var existing in existingFileData)
            _files.Add(existing.Key, new MemoryStream(existing.Value));
    }

    public MemoryFileHandler(string existingFile, byte[] existingData)
    {
        _files = new Dictionary<string, MemoryStream>();
        _files.Add(existingFile, new MemoryStream(existingData));
    }

    public Stream GetFileStream(string fileName)
    {
        MemoryStream? stream;
        if (!_files.TryGetValue(fileName, out stream))
        {
            stream = new MemoryStream();
            _files.Add(fileName, stream);
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
        foreach (var file in _files)
            file.Value.Dispose();
    }
}
