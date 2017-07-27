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
using System.Linq;
using System.Threading.Tasks;

namespace TorrentCore.Data
{
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

        public byte[] Value { get; private set; }

        /// <summary>
        /// Creates a new hash with the specified value.
        /// </summary>
        /// <param name="value">20-byte value of the hash.</param>
        public Sha1Hash(byte[] value)
        {
            if (value == null || value.Length != Length)
                throw new ArgumentException(String.Format("Value must be {0} bytes.", Length));

            this.Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Sha1Hash)
            {
                Sha1Hash other = (Sha1Hash)obj;
                return Enumerable.SequenceEqual(this.Value, other.Value);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public static bool operator ==(Sha1Hash x, Sha1Hash y)
        {
            if (System.Object.ReferenceEquals(x, y))
                return true;
            else if (((object)x == null) || ((object)y == null))
                return false;
            else
                return Enumerable.SequenceEqual(x.Value, y.Value);
        }

        public static bool operator !=(Sha1Hash x, Sha1Hash y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            string base64 = Convert.ToBase64String(Value);
            return base64.Substring(0, 8);
        }

        public static implicit operator byte[](Sha1Hash hash)
        {
            return hash.Value;
        }
    }
}
