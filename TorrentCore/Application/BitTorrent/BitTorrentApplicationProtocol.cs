// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using TorrentCore.Application.BitTorrent.ExtensionModule;
using TorrentCore.Application.BitTorrent.Messages;
using TorrentCore.Data;
using TorrentCore.Data.Pieces;
using TorrentCore.Engine;
using TorrentCore.Modularity;
using TorrentCore.Transport;

namespace TorrentCore.Application.BitTorrent;

/// <summary>
/// Manages the connections to peers.
/// </summary>
/// <remarks>This class is not thread-safe. All methods should be called from the <see cref="MainLoop"/> thread.</remarks>
class BitTorrentApplicationProtocol :
    IApplicationProtocol,
    IPeerMessageHandler,
    ITorrentContext
{
    private readonly ILogger<BitTorrentApplicationProtocol> _logger;
    private readonly PeerId _localPeerId;
    private readonly IApplicationProtocolPeerInitiator _peerInitiator;
    private readonly IReadOnlyCollection<IModule> _modules;
    private readonly HashSet<BitTorrentPeer> _peers = new HashSet<BitTorrentPeer>();
    private readonly List<ITransportStream> _connectingPeers = new List<ITransportStream>();
    private readonly HashSet<ITransportStream> _availablePeers = new HashSet<ITransportStream>(new TransportStreamAddressEqualityComparer());
    private readonly Dictionary<Tuple<BitTorrentPeer, byte>, IModule> _messageHandlerRegistrations = new Dictionary<Tuple<BitTorrentPeer, byte>, IModule>();
    private readonly BlockRequestManager _blockRequests;

    private readonly object _peersLock = new object();

    public BitTorrentApplicationProtocol(
        ILogger<BitTorrentApplicationProtocol> logger,
        PeerId localPeerId,
        Metainfo metainfo,
        IApplicationProtocolPeerInitiator peerInitiator,
        IEnumerable<IModule> modules,
        IPieceDataHandler dataHandler)
    {
        _logger = logger;
        _localPeerId = localPeerId;
        _peerInitiator = peerInitiator;
        _modules = modules.ToList().AsReadOnly();
        DataHandler = dataHandler;
        dataHandler.PieceCompleted += PieceCompleted;
        dataHandler.PieceCorrupted += PieceCorrupted;
        Metainfo = metainfo;
        _blockRequests = new BlockRequestManager();
    }

    public event EventHandler? DownloadCompleted;

    public Metainfo Metainfo { get; }

    public IReadOnlyCollection<BitTorrentPeer> Peers => _peers;

    IReadOnlyCollection<IPeer> IApplicationProtocol.Peers => _peers;

    public IReadOnlyCollection<ITransportStream> AvailablePeers => _availablePeers;

    public IReadOnlyCollection<ITransportStream> ConnectingPeers => _connectingPeers;

    public IBlockRequests BlockRequests => _blockRequests;

    public IPieceDataHandler DataHandler { get; }

    public long Uploaded { get; private set; }

    public void PeersAvailable(IEnumerable<ITransportStream> newPeers)
    {
        foreach (var peer in newPeers)
        {
            lock (_peersLock)
            {
                bool added = _availablePeers.Add(peer);
                if (!added)
                    _logger.LogInformation($"Discarded duplicate peer {peer.DisplayAddress}");
            }
        }
    }

    /// <summary>
    /// Handles new incoming connection requests.
    /// </summary>
    /// <param name="e">Event args for handling the request.</param>
    public void AcceptConnection(AcceptPeerConnectionEventArgs e)
    {
        var peer = e.Accept();
        PeerConnected(peer);
    }

    public void PieceCompleted(Piece piece)
    {
        _blockRequests.ClearBlocksForPiece(piece);

        foreach (var peer in _peers)
            peer.SendMessage(new HaveMessage(piece));

        if (!DataHandler.IncompletePieces().Any())
        {
            DataHandler.Flush();
            DownloadCompleted?.Invoke(this, new EventArgs());
        }
    }

    public void PieceCorrupted(Piece piece)
    {
        _blockRequests.ClearBlocksForPiece(piece);
    }

    /// <summary>
    /// Invoked when a message is received from a connected peer.
    /// </summary>
    /// <param name="peer">Peer that sent the message.</param>
    /// <param name="data">Received message data.</param>
    public void MessageReceived(BitTorrentPeer peer, byte[] data)
    {
        if (data.Length == 0)
            return;

        var reader = new BigEndianBinaryReader(new MemoryStream(data));

        byte messageId = reader.ReadByte();

        _logger.LogTrace($"Message received: {messageId}");

        if (_messageHandlerRegistrations.TryGetValue(Tuple.Create(peer, messageId), out var module))
        {
            var customValues = peer.GetCustomValues(module);
            var messageReceivedContext = new MessageReceivedContext(
                peer,
                this,
                messageId,
                data.Length - 1,
                reader,
                customValues,
                rMessageId => RegisterModuleForMessageId(peer, module, rMessageId));

            module.OnMessageReceived(messageReceivedContext);
            _logger.LogTrace($"Message of type {messageId} handled by module {module.GetType().Name}");
        }
        else
        {
            // Unknown message type
            _logger.LogWarning($"Received unknown message type {messageId} from {peer.Address}");
            peer.Disconnect();
        }
    }

    public async Task ConnectToPeerAsync(ITransportStream peerTransport)
    {
        try
        {
            lock (_peersLock)
            {
                _availablePeers.Remove(peerTransport);
                _connectingPeers.Add(peerTransport);
            }

            _logger.LogInformation($"Connecting to peer at {peerTransport.DisplayAddress}");
            await peerTransport.ConnectAsync();

            // TODO: run on main loop thread
            if (!peerTransport.IsConnected)
            {
                _logger.LogInformation($"Failed to connect to peer at {peerTransport.DisplayAddress}");

                // Connection failed
                lock (_peersLock)
                {
                    _connectingPeers.Remove(peerTransport);
                }

                // TODO: keep a record of failed connection peers
                return;
            }

            var peer = _peerInitiator.InitiateOutgoingConnection(peerTransport, this);

            lock (_peersLock)
            {
                _connectingPeers.Remove(peerTransport);
            }

            PeerConnected(peer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to peer");

            lock (_peersLock)
            {
                if (_connectingPeers.Contains(peerTransport))
                    _connectingPeers.Remove(peerTransport);
            }
        }
    }

    public void UploadedData(byte[] data)
    {
        Uploaded += data.Length;
    }

    private void RegisterModuleForMessageId(BitTorrentPeer peer, IModule module, byte messageId)
    {
        lock (_peersLock)
            _messageHandlerRegistrations[Tuple.Create(peer, messageId)] = module;
    }

    private void PeerConnected(IPeer e)
    {
        if (!(e is BitTorrentPeer peer))
            throw new ArgumentException($"Expected peer of type {typeof(BitTorrentPeer)} but was {typeof(IPeer)}");

        _logger.LogInformation($"Connected to peer at {peer.Address}");

        lock (_peersLock)
            _peers.Add(peer);

        foreach (var module in _modules)
        {
            var context = new PeerContext(
                peer,
                peer.GetCustomValues(module),
                this,
                messageId => RegisterModuleForMessageId(peer, module, messageId));
            module.OnPeerConnected(context);
        }

        peer.ReceiveData();
    }

    public void PeerDisconnected(BitTorrentPeer peer)
    {
        lock (_peersLock)
        {
            _logger.LogInformation($"Disconnected from peer at {peer.Address}");
            _peers.Remove(peer);

            // TODO: optimise this
            foreach (var r in _messageHandlerRegistrations.Where(x => x.Key.Item1 == peer).ToList())
                _messageHandlerRegistrations.Remove(r.Key);
        }
    }

    public void Dispose()
    {
        DataHandler.Dispose();
    }
}
