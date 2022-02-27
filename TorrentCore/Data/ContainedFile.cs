// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Data;

/// <summary>
/// Describes a single file within a download collection.
/// </summary>
public class ContainedFile
{
    public ContainedFile(string name, long size)
    {
        Name = name;
        Size = size;
    }

    /// <summary>
    /// Gets the name of this file, including directories.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the size of the file, in bytes.
    /// </summary>
    public long Size { get; }
}
