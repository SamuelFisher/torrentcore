// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Application.BitTorrent;

/// <summary>
/// Represents a region of a piece with a specified offset and length, without data.
/// </summary>
public class BlockRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockRequest"/> class, using the specified piece index and offset.
    /// </summary>
    /// <param name="pieceIndex">Index of the piece the block belongs to.</param>
    /// <param name="offset">Offset into the piece at which the data starts.</param>
    /// <param name="length">Length of the block.</param>
    internal BlockRequest(int pieceIndex, int offset, int length)
    {
        PieceIndex = pieceIndex;
        Offset = offset;
        Length = length;
    }

    /// <summary>
    /// Gets a value indicating the index of the piece to which the block belongs.
    /// </summary>
    public int PieceIndex { get; }

    /// <summary>
    /// Gets a value indicating the offset into the piece the data contained within the block represents.
    /// </summary>
    public int Offset { get; }

    /// <summary>
    /// Gets a value indicating the length of the data contained within the block.
    /// </summary>
    public int Length { get; }

    public static bool operator ==(BlockRequest? x, BlockRequest? y)
    {
        if (object.ReferenceEquals(x, y))
        {
            return true;
        }
        else if (((object?)x == null) || ((object?)y == null))
        {
            return false;
        }
        else
        {
            return x.PieceIndex == y.PieceIndex
                   && x.Offset == y.Offset
                   && x.Length == y.Length;
        }
    }

    public static bool operator !=(BlockRequest? x, BlockRequest? y)
    {
        return !(x == y);
    }

    public override bool Equals(object? obj)
    {
        if (obj is BlockRequest)
        {
            BlockRequest other = (BlockRequest)obj;
            return this == other;
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        int hash = 13;
        hash = (hash * 7) + PieceIndex.GetHashCode();
        hash = (hash * 7) + Offset.GetHashCode();
        hash = (hash * 7) + Length.GetHashCode();
        return hash;
    }
}
