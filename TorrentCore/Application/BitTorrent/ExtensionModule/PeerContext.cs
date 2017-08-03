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
using System.IO;
using System.Text;
using TorrentCore.ExtensionModule;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent.ExtensionModule
{
    class PeerContext : IPeerContext
    {
        private readonly PeerConnection peer;
        private readonly Dictionary<string, object> customValues;
        private readonly Action<byte> registerMessageHandler;

        public PeerContext(PeerConnection peer,
                           Dictionary<string, object> customValues,
                           Action<byte> registerMessageHandler)
        {
            this.peer = peer;
            this.customValues = customValues;
            this.registerMessageHandler = registerMessageHandler;
        }

        public IReadOnlyList<byte> ReservedBytes => peer.ReservedBytes;

        public T GetValue<T>(string key)
        {
            if (!customValues.TryGetValue(key, out object value))
                return default(T);

            return (T)value;
        }

        public void SetValue<T>(string key, T value)
        {
            customValues[key] = value;
        }

        public void RegisterMessageHandler(byte messageId)
        {
            registerMessageHandler(messageId);
        }

        public void SendMessage(byte messageId, byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                BinaryWriter writer = new BigEndianBinaryWriter(ms);
                writer.Write(messageId);
                writer.Write(data);
                writer.Flush();
                peer.Send(ms.ToArray());
            }
        }
    }
}
