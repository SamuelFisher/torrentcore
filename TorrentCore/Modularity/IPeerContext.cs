// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using TorrentCore.Application.BitTorrent;

namespace TorrentCore.Modularity
{
    public interface IPeerContext : ITorrentContext
    {
        BitTorrentPeer Peer { get; }

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
