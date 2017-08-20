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
using BencodeNET.Objects;
using BencodeNET.Parsing;
using TorrentCore.Extensions.ExtensionProtocol;

namespace TorrentCore.Extensions.SendMetadata
{
    class MetadataMessage : IExtensionProtocolMessage
    {
        public const string MessageType = "ut_metadata";

        public enum Type
        {
            Request = 0,
            Data = 1,
            Reject = 2
        }

        string IExtensionProtocolMessage.MessageType => MessageType;
        
        public Type RequestType { get; set; }

        public int PieceIndex { get; set; }

        public int TotalSize { get; set; }

        public byte[] PieceData { get; set; }
        
        public byte[] Serialize()
        {
            var dict = new BDictionary
            {
                ["msg_type"] = new BNumber((int)RequestType),
                ["piece"] = new BNumber(PieceIndex)
            };

            if (RequestType == Type.Data)
                dict["total_size"] = new BNumber(TotalSize);

            using (var ms = new MemoryStream())
            {
                dict.EncodeTo(ms);

                if (RequestType == Type.Data)
                    ms.Write(PieceData, 0, PieceData.Length);

                return ms.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var dictParser = new BDictionaryParser(new BencodeParser());
                var dict = dictParser.Parse(ms);

                var requestType = (Type)((BNumber)dict["msg_type"]).Value;
                if (!Enum.IsDefined(typeof(Type), requestType))
                    return; // Unsupported message type

                RequestType = requestType;
            
                PieceIndex = (int)((BNumber)dict["piece"]).Value;

                if (RequestType == Type.Data)
                {
                    TotalSize = (int)((BNumber)dict["total_size"]).Value;
                    PieceData = new byte[ms.Length - ms.Position];
                    ms.Read(PieceData, 0, (int)(ms.Length - ms.Position));
                }
            }
        }
    }
}
