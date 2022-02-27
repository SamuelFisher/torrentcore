// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Net;

namespace TorrentCore.Transport;

class BigEndianBinaryWriter : BinaryWriter
{
    public BigEndianBinaryWriter(Stream stream)
        : base(stream)
    {
    }

    public override void Write(short value)
    {
        value = IPAddress.HostToNetworkOrder(value);
        base.Write(value);
    }

    public override void Write(ushort value)
    {
        int networkOrderInt = IPAddress.HostToNetworkOrder((int)value);
        byte[] bytes = BitConverter.GetBytes(networkOrderInt);
        base.Write(bytes, 2, 2);
    }

    public override void Write(int value)
    {
        value = IPAddress.HostToNetworkOrder(value);
        base.Write(value);
    }

    public override void Write(long value)
    {
        value = IPAddress.HostToNetworkOrder(value);
        base.Write(value);
    }
}
