// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2017 Sam Fisher.
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

        private readonly LocalTcpConnectionDetails tcpConnectionDetails;
        private readonly Uri trackerUri;
        private readonly Random rand;
        private readonly UdpClient client;

        public UdpTracker(LocalTcpConnectionDetails tcpConnectionDetails, Uri trackerUri)
        {
            this.tcpConnectionDetails = tcpConnectionDetails;
            this.trackerUri = trackerUri;
            rand = new Random();
            // TODO don't listen until needed
            client = new UdpClient(0);
        }

        public string Type => "udp";

        public async Task<AnnounceResult> Announce(AnnounceRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            int transactionId = rand.Next();
            var connectionResponse = await SendAndWaitForResponse<ConnectionResponseMessage>(new ConnectionRequestMessage
            {
                ConnectionId = ConnectionProtocolId, // Identifies tracker UDP protocol
                TransactionId = transactionId
            });

            if (connectionResponse.TransactionId != transactionId)
                throw new InvalidDataException("Mismatching transaction ID");

            long connectionId = connectionResponse.ConnectionId;

            transactionId = rand.Next();
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
                IPAddress = tcpConnectionDetails.PublicAddress,
                Key = rand.Next(),
                NumWant = -1, // default
                Port = (ushort)tcpConnectionDetails.Port
            });
            
            if (announceResponse.TransactionId != transactionId)
                throw new InvalidDataException("Mismatching transaction ID");

            return new AnnounceResult(announceResponse.Peers.Select(x => new TcpTransportStream(tcpConnectionDetails.BindAddress, x.IPAddress, x.Port)));
        }

        protected async Task<T> SendAndWaitForResponse<T>(UdpTrackerRequestMessage request) where T : UdpTrackerResponseMessage, new()
        {
            await Send(request);
            return await Receive<T>();
        }

        protected Task Send(UdpTrackerRequestMessage message)
        {
            var ms = new MemoryStream();
            var writer = new BigEndianBinaryWriter(ms);
            message.WriteTo(writer);
            writer.Flush();

            return client.SendAsync(ms.ToArray(), (int)ms.Length, trackerUri.Host, trackerUri.Port);
        }

        protected async Task<T> Receive<T>() where T : UdpTrackerResponseMessage, new()
        {
            var result = await client.ReceiveAsync();
            var message = new T();
            var ms = new MemoryStream(result.Buffer);
            var reader = new BigEndianBinaryReader(ms);
            message.ReadFrom(reader);
            return message;
        }
    }
}
