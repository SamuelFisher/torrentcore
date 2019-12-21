// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using TorrentCore.Extensions.ExtensionProtocol;

namespace TorrentCore.Test.Extension
{
    public class WrongTypeMessage : IExtensionProtocolMessage
    {
        public string MessageType => "Wrong";

        public void Deserialize(byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
