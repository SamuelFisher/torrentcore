// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }

        public void Deserialize(byte[] data)
        {
            var dictParser = new BDictionaryParser(new BencodeParser());
            var dict = dictParser.Parse(data);

            if (dict.TryGetValue("added", out IBObject added))
                Added = ParseEndPoints((BString)added).ToList();
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
