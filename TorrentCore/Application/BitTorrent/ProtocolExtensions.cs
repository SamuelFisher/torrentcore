// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Application.BitTorrent;

[Flags]
public enum ProtocolExtension
{
    None = 0,

    /// <summary>
    /// BEP 6 Fast Extension.
    /// </summary>
    FastPeers = 1,

    /// <summary>
    /// BEP 5 Distributed Hash Table.
    /// </summary>
    Dht = 2,

    /// <summary>
    /// BEP 10 Extension Protocol.
    /// </summary>
    ExtensionProtocol = 3,
}

static class ProtocolExtensions
{
    public static ProtocolExtension DetermineSupportedProcotolExtensions(byte[] reserved)
    {
        var protocols = ProtocolExtension.None;

        if ((reserved[7] & 0x04) != 0)
            protocols |= ProtocolExtension.FastPeers;

        if ((reserved[7] & 0x01) != 0)
            protocols |= ProtocolExtension.Dht;

        if ((reserved[5] & 0x10) != 0)
            protocols |= ProtocolExtension.ExtensionProtocol;

        return protocols;
    }
}
