// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2016 Sam Fisher.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Data;
using TorrentCore.Tracker;
using TorrentCore.Transport;

namespace TorrentCore.Application
{
    public interface IApplicationProtocol<TConnection>
    {
        /// <summary>
        /// Gets the peers that are currently connected.
        /// </summary>
        IReadOnlyCollection<TConnection> Peers { get; }

        /// <summary>
        /// Handles new incoming connection requests.
        /// </summary>
        /// <param name="e">Event args for handling the request.</param>
        void AcceptConnection(AcceptPeerConnectionEventArgs<TConnection> e);

        /// <summary>
        /// Performs actions in each cycle of the main loop.
        /// </summary>
        void Iterate();

        /// <summary>
        /// Invoked when a pieces has been fully downloaded but fails its hash check.
        /// </summary>
        /// <param name="e">Details of the corrupted piece.</param>
        void PieceCorrupted(PieceCompletedEventArgs e);

        /// <summary>
        /// Invoked when a piece has been fully downloaded and passes its hash check.
        /// </summary>
        /// <param name="e">Details of the completed piece.</param>
        void PieceCompleted(PieceCompletedEventArgs e);

        /// <summary>
        /// Called when an announce result is received from a tracker.
        /// </summary>
        /// <param name="result">Result containing information from the tracker.</param>
        void PeersAvailable(IEnumerable<ITransportStream> streams);
    }
}
