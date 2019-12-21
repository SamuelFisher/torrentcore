// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using TorrentCore.Extensions.ExtensionProtocol;
using TorrentCore.Serialization;
using TorrentCore.Transport;

namespace TorrentCore.Extensions.PeerExchange
{
    class PeerExchangeMessage : IExtensionProtocolMessage
    {
        public const string MessageType = "ut_pex";

        string IExtensionProtocolMessage.MessageType => MessageType;

        public IList<IPEndPoint> Added { get; set; }

        public IList<IPEndPoint> Dropped { get; set; }

        public byte[] Serialize()
        {
            var dict = new BDictionary
            {
                ["added"] = EncodeEndPoints(Added),
                ["dropped"] = EncodeEndPoints(Dropped),
            };

            using (var ms = new MemoryStream())
            {
                dict.EncodeTo(ms);
                return ms.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            var dictParser = new BDictionaryParser(new BencodeParser());
            var dict = dictParser.Parse(data);

            if (dict.TryGetValue("added", out IBObject added))
                Added = ParseEndPoints((BString)added).ToList();
        }

        private BString EncodeEndPoints(IList<IPEndPoint> endpoints)
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BigEndianBinaryWriter(ms);
                foreach (var endpoint in endpoints)
                {
                    writer.Write(endpoint.Address.GetAddressBytes());
                    writer.Write((ushort)endpoint.Port);
                }
                return new BString(ms.ToArray());
            }
        }

        private IEnumerable<IPEndPoint> ParseEndPoints(BString input)
        {
            using (var ms = new MemoryStream(input.Value.ToArray()))
            {
                var reader = new BigEndianBinaryReader(ms);
                while (ms.Position < ms.Length)
                {
                    yield return reader.ReadIpV4EndPoint();
                }
            }
        }
    }
}
