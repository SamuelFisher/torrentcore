// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Application.BitTorrent.Messages;

/// <summary>
/// A message used to inform a peer that it is being chocked.
/// </summary>
public class ChokeMessage : CommonPeerMessage
{
    public const byte MessageID = 0;

    /// <summary>
    /// Gets the ID of the message.
    /// </summary>
    public override byte ID => MessageID;
}
