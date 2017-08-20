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
using TorrentCore.Application.BitTorrent;

namespace TorrentCore.Modularity
{
    public interface IPeerContext : ITorrentContext
    {
        PeerConnection Peer { get; }
        
        /// <summary>
        /// Retrieves a value with the specified key that is associated with this peer.
        /// If the key does not exist, returns null. If the value is not of the expected type, throws an
        /// <see cref="InvalidCastException"/>.
        /// </summary>
        /// <typeparam name="T">Type of the value to retrieve.</typeparam>
        /// <param name="key">Key of the value to retrieve.</param>
        /// <returns>The value corresponding to the specified key.</returns>
        T GetValue<T>(string key);
        
        void SetValue<T>(string key, T value);

        /// <summary>
        /// Registers the module to be called when a message of the specified type is received from a peer.
        /// </summary>
        /// <param name="messageId">The type of message to register for.</param>
        void RegisterMessageHandler(byte messageId);

        /// <summary>
        /// Sends a BitTorrent protocol message to this peer.
        /// </summary>
        /// <param name="messageId">The type of message being sent.</param>
        /// <param name="data">The message data.</param>
        void SendMessage(byte messageId, byte[] data);
    }
}
