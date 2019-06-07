// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TorrentCore.Transport;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Tracker.Udp
{
    class UdpTracker : ITracker
    {
        private const long ConnectionProtocolId = 0x41727101980L;

        private readonly LocalTcpConnectionOptions _tcpConnectionDetails;
        private readonly Uri _trackerUri;
        private readonly Random _rand;
        private readonly UdpClient _client;

        public UdpTracker(LocalTcpConnectionOptions tcpConnectionDetails, Uri trackerUri)
        {
            _tcpConnectionDetails = tcpConnectionDetails;
            _trackerUri = trackerUri;
            _rand = new Random();

            // TODO don't listen until needed
            _client = new UdpClient(0);
        }

        public string Type => "udp";

        public async Task<AnnounceResult> Announce(AnnounceRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            int transactionId = GenerateTransactionId();
            var connectionResponse = await SendAndWaitForResponse<ConnectionResponseMessage>(new ConnectionRequestMessage
            {
                ConnectionId = ConnectionProtocolId, // Identifies tracker UDP protocol
                TransactionId = transactionId,
            });

            if (connectionResponse.TransactionId != transactionId)
                throw new InvalidDataException("Mismatching transaction ID");

            long connectionId = connectionResponse.ConnectionId;

            transactionId = GenerateTransactionId();
            var announceResponse = await SendAndWaitForResponse<AnnounceResponseMessage>(new AnnounceRequestMessage
            {
                ConnectionId = connectionId,
                TransactionId = transactionId,
                InfoHash = request.InfoHash,
                PeerId = new byte[20], // todo
                Downloaded = 0, // todo
                LeftToDownload = request.Remaining,
                Uploaded = 0, // todo
                Event = AnnounceRequestMessage.EventType.Started,
                IPAddress = _tcpConnectionDetails.PublicAddress,
                Key = _rand.Next(),
                NumWant = -1, // default
                Port = (ushort)_tcpConnectionDetails.Port,
            });

            if (announceResponse.TransactionId != transactionId)
                throw new InvalidDataException("Mismatching transaction ID");

            return new AnnounceResult(announceResponse.Peers.Select(x => new TcpTransportStream(_tcpConnectionDetails.BindAddress, x.IPAddress, x.Port)));
        }

        protected async Task<T> SendAndWaitForResponse<T>(UdpTrackerRequestMessage request)
            where T : UdpTrackerResponseMessage, new()
        {
            await Send(request);
            return await Receive<T>();
        }

        protected virtual Task Send(UdpTrackerRequestMessage message)
        {
            var ms = new MemoryStream();
            var writer = new BigEndianBinaryWriter(ms);
            message.WriteTo(writer);
            writer.Flush();

            return _client.SendAsync(ms.ToArray(), (int)ms.Length, _trackerUri.Host, _trackerUri.Port);
        }

        protected async Task<T> Receive<T>()
            where T : UdpTrackerResponseMessage, new()
        {
            var message = new T();
            var ms = await Receive();
            var reader = new BigEndianBinaryReader(ms);
            message.ReadFrom(reader);
            return message;
        }

        protected virtual async Task<MemoryStream> Receive()
        {
            var result = await _client.ReceiveAsync();
            return new MemoryStream(result.Buffer);
        }

        protected virtual int GenerateTransactionId()
        {
            return _rand.Next();
        }
    }
}
