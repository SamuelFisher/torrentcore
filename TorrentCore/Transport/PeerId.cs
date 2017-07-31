// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2017 Sam Fisher.
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TorrentCore.Transport
{
    public sealed class PeerId
    {
        private static readonly Dictionary<string, string> ClientIds;

        static PeerId()
        {
            var assembly = typeof(PeerId).GetTypeInfo().Assembly;
            using (var peerIdClientsStream = new StreamReader(assembly.GetManifestResourceStream("TorrentCore.Transport.ClientPeerIds.txt")))
            {
                ClientIds =
                    peerIdClientsStream.ReadToEnd()
                                       .Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(x => x.Trim().Split('='))
                                       .ToDictionary(x => x[0], x => x[1]);
            }
        }

        public PeerId(byte[] peerId)
        {
            if (peerId.Length != 20)
                throw new ArgumentException("Peer ID must be 20 bytes long.", nameof(peerId));

            Value = new ReadOnlyCollection<byte>(peerId);

            string str = ToString();
            if (str[0] == '-' && str[7] == '-')
            {
                // Azureus-style peer id
                string clientId = str.Substring(1, 2);
                string clientName;
                if (!ClientIds.TryGetValue(clientId, out clientName))
                    clientName = null;
                ClientName = clientName;

                string clientVersion = str.Substring(3, 4);
                int version;
                if (int.TryParse(clientVersion, out version))
                    ClientVersion = version;
            }
        }

        /// <summary>
        /// Gets the name of the BitTorrent client the peer is using.
        /// <remarks>Null if the client name cannot be determined.</remarks>
        /// </summary>
        public string ClientName { get; }

        /// <summary>
        /// Gets the version of the BitTorrent client the peer is using.
        /// <remarks>Null if the client version cannot be determined.</remarks>
        /// </summary>
        public int? ClientVersion { get; }

        public IReadOnlyList<byte> Value { get; }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(Value.ToArray());
        }
    }
}
