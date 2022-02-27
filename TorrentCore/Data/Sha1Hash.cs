// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Data;

/// <summary>
/// Represents a SHA-1 hash result.
/// </summary>
public sealed class Sha1Hash
{
    /// <summary>
    /// Gets the length in bytes of a raw hash data.
    /// </summary>
    public const int Length = 20;

    /// <summary>
    /// Gets the empty hash.
    /// </summary>
    public static readonly Sha1Hash Empty;

    static Sha1Hash()
    {
        Empty = new Sha1Hash(new byte[Length]);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sha1Hash"/> class with the specified value.
    /// </summary>
    /// <param name="value">20-byte value of the hash.</param>
    public Sha1Hash(byte[] value)
    {
        if (value == null || value.Length != Length)
            throw new ArgumentException(string.Format("Value must be {0} bytes.", Length));

        Value = value;
    }

    public byte[] Value { get; }

    public static implicit operator byte[](Sha1Hash hash)
    {
        return hash.Value;
    }

    public static bool operator ==(Sha1Hash x, Sha1Hash y)
    {
        if (ReferenceEquals(x, y))
            return true;
        else if ((object)x == null || ((object)y == null))
            return false;
        else
            return Enumerable.SequenceEqual(x.Value, y.Value);
    }

    public static bool operator !=(Sha1Hash x, Sha1Hash y)
    {
        return !(x == y);
    }

    public override bool Equals(object? obj)
    {
        if (obj is Sha1Hash)
        {
            Sha1Hash other = (Sha1Hash)obj;
            return Enumerable.SequenceEqual(Value, other.Value);
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            foreach (byte el in Value)
                hash = hash * 31 + el.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        string base64 = Convert.ToBase64String(Value);
        return base64.Substring(0, 8);
    }
}
