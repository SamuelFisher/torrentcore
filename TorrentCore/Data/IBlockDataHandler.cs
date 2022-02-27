// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace TorrentCore.Data;

/// <summary>
/// Provides access to a collection of files as a single block of data.
/// </summary>
public interface IBlockDataHandler
{
    /// <summary>
    /// Gets the metainfo describing the layout of the collection of files.
    /// </summary>
    Metainfo Metainfo { get; }

    /// <summary>
    /// Returns a copy of the contiguous file data starting at the specified offset.
    /// </summary>
    /// <param name="offset">Offset to read from.</param>
    /// <param name="length">Number of bytes to read.</param>
    /// <returns>Block data from specified region.</returns>
    byte[]? ReadBlockData(long offset, long length);

    /// <summary>
    /// Attempts to read a copy of the contiguous file data starting at the specified offset.
    /// </summary>
    /// <param name="offset">Offset to read from.</param>
    /// <param name="length">Number of bytes to read.</param>
    /// <param name="data">Block data from specified region.</param>
    /// <returns>Value indicating whether the operation was successful.</returns>
    bool TryReadBlockData(long offset, long length, [NotNullWhen(true)] out byte[]? data);

    /// <summary>
    /// Writes the specified contiguous data from the given offset position.
    /// </summary>
    /// <param name="offset">Offset at which to start writing.</param>
    /// <param name="data">Block data to write.</param>
    void WriteBlockData(long offset, byte[] data);

    /// <summary>
    /// Flushes any underlying streams to ensure all data has been written.
    /// </summary>
    void Flush();
}
