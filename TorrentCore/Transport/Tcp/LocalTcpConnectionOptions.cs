// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Net;

namespace TorrentCore.Transport.Tcp;

public class LocalTcpConnectionOptions
{
    /// <summary>
    /// Gets or sets the port on which incoming connections can be made.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the public address that is used to listen for incoming connections.
    /// </summary>
    public IPAddress? PublicAddress { get; set; }

    /// <summary>
    /// Gets or sets the address of the local adapter used for connections.
    /// </summary>
    public IPAddress BindAddress { get; set; } = IPAddress.Any;
}
