// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Sockets;

namespace TorrentCore.Transport.Tcp;

sealed class TcpTransportStream : ITransportStream
{
    private readonly ManualResetEvent _connectionEvent = new ManualResetEvent(false);
    private readonly TcpClient _client;

    private Stream? _stream;

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpTransportStream"/> class.
    /// </summary>
    /// <param name="adapterAddress">Local IP address of the adapter to bind to.</param>
    /// <param name="remoteAddress">IP address of remote peer.</param>
    /// <param name="port">Port of remote peer.</param>
    public TcpTransportStream(IPAddress adapterAddress, IPAddress remoteAddress, int port)
    {
        _client = new TcpClient(adapterAddress.AddressFamily);

        // Use the adapter for the IPAddress specified
        _client.Client.Bind(new IPEndPoint(adapterAddress, 0));

        RemoteEndPoint = new IPEndPoint(remoteAddress, port);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpTransportStream"/> class.
    /// </summary>
    /// <param name="client">Existing connection.</param>
    public TcpTransportStream(TcpClient client)
    {
        _client = client;
        _stream = new RateLimitedStream(client.GetStream());
        RemoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint!;
    }

    public Stream Stream => _stream ?? throw new InvalidOperationException();

    public IPEndPoint RemoteEndPoint { get; }

    string ITransportStream.DisplayAddress => RemoteEndPoint.ToString();

    object ITransportStream.Address => RemoteEndPoint;

    /// <summary>
    /// Gets a value indicating whether this connection is active.
    /// </summary>
    public bool IsConnected => _client.Connected;

    /// <summary>
    /// Gets a value indicating whether a connection attempt is in progress for this stream.
    /// </summary>
    public bool IsConnecting { get; private set; }

    /// <summary>
    /// Attempts to initiate this connection.
    /// </summary>
    /// <returns>Task which completes when the connection is made.</returns>
    public async Task Connect()
    {
        if (IsConnected)
            throw new InvalidOperationException("Already connected.");

        if (RemoteEndPoint == null)
            throw new InvalidOperationException("Address and port have not been specified.");

        if (IsConnecting)
        {
            _connectionEvent.WaitOne();
            return;
        }

        IsConnecting = true;

        try
        {
            await _client.ConnectAsync(RemoteEndPoint.Address, RemoteEndPoint.Port);
        }
        finally
        {
            IsConnecting = false;
            _connectionEvent.Set();
        }

        _stream = new RateLimitedStream(_client.GetStream());

        IsConnecting = false;
        _connectionEvent.Set();
    }

    public void Disconnect()
    {
        _client.Dispose();
    }
}
