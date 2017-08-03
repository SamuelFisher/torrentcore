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

        public byte[] Serialize()
        {
            var dict = new BDictionary(MessageIds.ToDictionary(x => new BString(x.Key), x => (IBObject)new BNumber(x.Value)));
            return new BDictionary
            {
                ["m"] = dict,
                ["v"] = new BString(Client)
            }.EncodeAsBytes();
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
