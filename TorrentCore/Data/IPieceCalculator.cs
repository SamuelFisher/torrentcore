// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TorrentCore.Data
{
    interface IPieceCalculator
    {
        void ComputePieces(List<ContainedFile> files, int pieceSize, IFileHandler fileHandler, List<Piece> pieces);
    }
}
