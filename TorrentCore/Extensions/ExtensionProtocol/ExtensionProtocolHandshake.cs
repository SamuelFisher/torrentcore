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
using System.Text;
using BencodeNET.Objects;
using BencodeNET.Parsing;

namespace TorrentCore.Extensions.ExtensionProtocol
{
    class ExtensionProtocolHandshake
    {
        public Dictionary<string, byte> MessageIds { get; set; }

        public string Client { get; set; }

        public BDictionary Serialize()
        {
            var dict = new BDictionary(MessageIds.ToDictionary(x => new BString(x.Key), x => (IBObject)new BNumber(x.Value)));
            return new BDictionary
            {
                ["m"] = dict,
                ["v"] = new BString(Client),
            };
        }

        public void Deserialize(IReadOnlyList<byte> data)
        {
            var dictParser = new BDictionaryParser(new BencodeParser());
            var dict = dictParser.Parse(data.ToArray());

            if (dict.TryGetValue("m", out IBObject msgTypes))
            {
                MessageIds = ((BDictionary)msgTypes).ToDictionary(x => x.Key.ToString(), x => (byte)((BNumber)x.Value).Value);
            }

            if (dict.TryGetValue("v", out IBObject client))
            {
                Client = ((BString)client).ToString();
            }
        }
    }
}
