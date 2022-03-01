// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Data;
using TorrentCore.Data.Pieces;
using TorrentCore.Transport;

namespace TorrentCore.Application;

public interface IApplicationProtocol
{
    event EventHandler DownloadCompleted;

    Metainfo Metainfo { get; }

    IPieceDataHandler DataHandler { get; }

    IBlockRequests BlockRequests { get; }

    /// <summary>
    /// Gets the peers that are currently connected.
    /// </summary>
    IReadOnlyCollection<IPeer> Peers { get; }

    /// <summary>
    /// Gets the peers that are available to connect to but are not currently connected.
    /// </summary>
    IReadOnlyCollection<ITransportStream> AvailablePeers { get; }

    /// <summary>
    /// Gets the peers that a connection is currently being established to.
    /// </summary>
    IReadOnlyCollection<ITransportStream> ConnectingPeers { get; }

    /// <summary>
    /// Gets the number of bytes uploaded.
    /// </summary>
    long Uploaded { get; }

    /// <summary>
    /// Attempts to connect to the peer addressed by the supplied transport stream.
    /// </summary>
    /// <param name="peerTransport">Transport stream to the peer.</param>
    Task ConnectToPeerAsync(ITransportStream peerTransport);

    /// <summary>
    /// Handles new incoming connection requests.
    /// </summary>
    /// <param name="e">Event args for handling the request.</param>
    void AcceptConnection(AcceptPeerConnectionEventArgs e);

    /// <summary>
    /// Invoked when a pieces has been fully downloaded but fails its hash check.
    /// </summary>
    /// <param name="piece">Details of the corrupted piece.</param>
    void PieceCorrupted(Piece piece);

    /// <summary>
    /// Invoked when a piece has been fully downloaded and passes its hash check.
    /// </summary>
    /// <param name="piece">Details of the completed piece.</param>
    void PieceCompleted(Piece piece);

    /// <summary>
    /// Called when an announce result is received from a tracker.
    /// </summary>
    /// <param name="peerStreams">Peer streams received from the tracker.</param>
    void PeersAvailable(IEnumerable<ITransportStream> peerStreams);
}
