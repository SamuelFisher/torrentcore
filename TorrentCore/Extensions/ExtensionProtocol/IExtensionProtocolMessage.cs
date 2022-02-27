// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Extensions.ExtensionProtocol;

public interface IExtensionProtocolMessage
{
    string MessageType { get; }

    byte[] Serialize();

    void Deserialize(byte[] data);
}
