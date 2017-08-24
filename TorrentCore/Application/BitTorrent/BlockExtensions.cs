// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Data;

namespace TorrentCore.Application.BitTorrent
{
    static class BlockExtensions
    {
        public static BlockRequest AsRequest(this Block block)
        {
            return new BlockRequest(block.PieceIndex, block.Offset, block.Data.Length);
        }

        public static Block ToBlock(this BlockRequest request, byte[] data)
        {
            return new Block(request.PieceIndex, request.Offset, data);
        }
    }
}
