// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2017 Samuel Fisher.
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
using System.Text;

namespace TorrentCore.ExtensionModule
{
    public interface IPeerContext
    {
        /// <summary>
        /// Gets the reserved bytes sent by this peer in the connected handshake.
        /// </summary>
        IReadOnlyList<byte> ReservedBytes { get; }

        /// <summary>
        /// Retrieves a value with the specified key that is associated with this peer.
        /// If the key does not exist, returns null. If the value is not of the expected type, throws an
        /// <see cref="InvalidCastException"/>.
        /// </summary>
        /// <typeparam name="T">Type of the value to retrieve.</typeparam>
        /// <param name="key">Key of the value to retrieve.</param>
        /// <returns></returns>
        T GetValue<T>(string key);

        void SetValue<T>(string key, T value);
    }
}
