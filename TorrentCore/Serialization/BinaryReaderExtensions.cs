// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Net;

namespace TorrentCore.Serialization;

public static class BinaryReaderExtensions
{
    public static IPEndPoint ReadIpV4EndPoint(this BinaryReader reader)
    {
        byte[] ipAddress = reader.ReadBytes(4);
        ushort port = reader.ReadUInt16();
        return new IPEndPoint(new IPAddress(ipAddress), port);
    }
}
