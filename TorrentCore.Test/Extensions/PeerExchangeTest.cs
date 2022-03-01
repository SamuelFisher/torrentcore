// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using TorrentCore.Application.BitTorrent;
using TorrentCore.Data;
using TorrentCore.Extensions.ExtensionProtocol;
using TorrentCore.Extensions.PeerExchange;
using TorrentCore.Transport;
using TorrentCore.Transport.Tcp;

namespace TorrentCore.Test.Extension;

[TestFixture]
public class PeerExchangeTest
{
    [Test]
    public void TestWithWrongMessageType()
    {
        var messageHandler = new PeerExchangeMessageHandler(Mock.Of<ILogger<PeerExchangeMessageHandler>>(), Mock.Of<ITcpTransportProtocol>());

        var context = new Mock<IExtensionProtocolMessageReceivedContext>();
        context.Setup(x => x.Message).Returns(new WrongTypeMessage());

        Assert.Throws<InvalidOperationException>(() => messageHandler.MessageReceived(context.Object));
    }

    [Test]
    public void TestWithoutMetadata()
    {
        PeerExchangeMessage sentMessage = new PeerExchangeMessage();
        var messageHandler = new PeerExchangeMessageHandler(Mock.Of<ILogger<PeerExchangeMessageHandler>>(), Mock.Of<ITcpTransportProtocol>());

        var context = new Mock<IExtensionProtocolMessageReceivedContext>();
        context.Setup(x => x.Message).Returns(MockMessage());
        context.Setup(x => x.Peers).Returns(MockPeers());
        context.Setup(x => x.GetValue<PeerExchangeMetadata>(PeerExchangeMetadata.Key)).Returns(default(PeerExchangeMetadata));
        context.Setup(x => x.SendMessage(It.IsAny<IExtensionProtocolMessage>())).Callback<IExtensionProtocolMessage>(message => sentMessage = (PeerExchangeMessage)message);

        messageHandler.MessageReceived(context.Object);

        context.Verify(x => x.Message, Times.Once);
        context.Verify(x => x.PeersAvailable(It.IsAny<IEnumerable<ITransportStream>>()), Times.Once);
        context.Verify(x => x.GetValue<PeerExchangeMetadata>(PeerExchangeMetadata.Key), Times.Once);
        context.Verify(x => x.Peers, Times.Once);
        context.Verify(x => x.SendMessage(It.IsAny<PeerExchangeMessage>()), Times.Once);
        context.Verify(x => x.SetValue(PeerExchangeMetadata.Key, It.IsAny<PeerExchangeMetadata>()), Times.Once);
        context.VerifyNoOtherCalls();

        var expectedSentMessage = new PeerExchangeMessage
        {
            Added = new List<IPEndPoint>
                {
                    new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 20 }), 8080),
                    new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 21 }), 8080),
                    new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 22 }), 8080),
                    new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 23 }), 8080),
                },
            Dropped = new List<IPEndPoint>(),
        };

        sentMessage.Should().BeEquivalentTo(expectedSentMessage);
    }

    [Test]
    public void TestWithSomePeersToSend()
    {
        PeerExchangeMessage sentMessage = new PeerExchangeMessage();
        var messageHandler = new PeerExchangeMessageHandler(Mock.Of<ILogger<PeerExchangeMessageHandler>>(), Mock.Of<ITcpTransportProtocol>());

        var context = new Mock<IExtensionProtocolMessageReceivedContext>();
        context.Setup(x => x.Message).Returns(MockMessage());
        context.Setup(x => x.Peers).Returns(MockPeers());
        context.Setup(x => x.SendMessage(It.IsAny<IExtensionProtocolMessage>())).Callback<IExtensionProtocolMessage>(message => sentMessage = (PeerExchangeMessage)message);

        var metadata = new PeerExchangeMetadata
        {
            ConnectedPeersSnapshot = new List<string>
                {
                    "192.168.1.20:8080",
                    "192.168.1.21:8080",
                    "192.168.1.25:8080",
                },
        };
        context.Setup(x => x.GetValue<PeerExchangeMetadata>(PeerExchangeMetadata.Key)).Returns(metadata);

        messageHandler.MessageReceived(context.Object);

        context.Verify(x => x.Message, Times.Once);
        context.Verify(x => x.PeersAvailable(It.IsAny<IEnumerable<ITransportStream>>()), Times.Once);
        context.Verify(x => x.GetValue<PeerExchangeMetadata>(PeerExchangeMetadata.Key), Times.Once);
        context.Verify(x => x.Peers, Times.Once);
        context.Verify(x => x.SendMessage(It.IsAny<PeerExchangeMessage>()), Times.Once);
        context.Verify(x => x.SetValue(PeerExchangeMetadata.Key, It.IsAny<PeerExchangeMetadata>()), Times.Once);
        context.VerifyNoOtherCalls();

        var expectedSentMessage = new PeerExchangeMessage
        {
            Added = new List<IPEndPoint>
                {
                    new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 22 }), 8080),
                    new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 23 }), 8080),
                },
            Dropped = new List<IPEndPoint>
                {
                    new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 25 }), 8080),
                },
        };

        sentMessage.Should().BeEquivalentTo(expectedSentMessage);
    }

    [Test]
    public void TestWithNoChangesFromSnapshot()
    {
        var messageHandler = new PeerExchangeMessageHandler(Mock.Of<ILogger<PeerExchangeMessageHandler>>(), Mock.Of<ITcpTransportProtocol>());

        var context = new Mock<IExtensionProtocolMessageReceivedContext>();
        context.Setup(x => x.Message).Returns(MockMessage());
        context.Setup(x => x.Peers).Returns(MockPeers());

        var metadata = new PeerExchangeMetadata
        {
            ConnectedPeersSnapshot = new List<string>
                {
                    "192.168.1.20:8080",
                    "192.168.1.21:8080",
                    "192.168.1.22:8080",
                    "192.168.1.23:8080",
                },
        };
        context.Setup(x => x.GetValue<PeerExchangeMetadata>(PeerExchangeMetadata.Key)).Returns(metadata);

        messageHandler.MessageReceived(context.Object);

        context.Verify(x => x.Message, Times.Once);
        context.Verify(x => x.PeersAvailable(It.IsAny<IEnumerable<ITransportStream>>()), Times.Once);
        context.Verify(x => x.GetValue<PeerExchangeMetadata>(PeerExchangeMetadata.Key), Times.Once);
        context.Verify(x => x.Peers, Times.Once);
        context.VerifyNoOtherCalls();
    }

    [Test]
    public void TestWithNotEnoughTimeSinceLastMessageIsSent()
    {
        var messageHandler = new PeerExchangeMessageHandler(Mock.Of<ILogger<PeerExchangeMessageHandler>>(), Mock.Of<ITcpTransportProtocol>());

        var context = new Mock<IExtensionProtocolMessageReceivedContext>();
        context.Setup(x => x.Message).Returns(MockMessage());
        context.Setup(x => x.Peers).Returns(MockPeers());

        var metadata = new PeerExchangeMetadata
        {
            LastMessageDate = DateTime.UtcNow.AddSeconds(-10),
            ConnectedPeersSnapshot = new List<string>
                {
                    "192.168.1.20:8080",
                    "192.168.1.21:8080",
                },
        };
        context.Setup(x => x.GetValue<PeerExchangeMetadata>(PeerExchangeMetadata.Key)).Returns(metadata);

        messageHandler.MessageReceived(context.Object);

        context.Verify(x => x.Message, Times.Once);
        context.Verify(x => x.PeersAvailable(It.IsAny<IEnumerable<ITransportStream>>()), Times.Once);
        context.Verify(x => x.GetValue<PeerExchangeMetadata>(PeerExchangeMetadata.Key), Times.Once);
        context.VerifyNoOtherCalls();
    }

    private PeerExchangeMessage MockMessage()
    {
        return new PeerExchangeMessage
        {
            Added = new List<IPEndPoint>
                {
                    new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 10 }), 8081),
                    new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 11 }), 8080),
                },
            Dropped = new List<IPEndPoint>
                {
                    new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 12 }), 8080),
                },
        };
    }

    private List<BitTorrentPeer> MockPeers()
    {
        return new List<BitTorrentPeer>
            {
                new BitTorrentPeer(NullLogger<BitTorrentPeer>.Instance, MockMetaInfo(), null, null, ProtocolExtension.None, null, new FakeTransportStream("192.168.1.20:8080")),
                new BitTorrentPeer(NullLogger<BitTorrentPeer>.Instance, MockMetaInfo(), null, null, ProtocolExtension.None, null, new FakeTransportStream("192.168.1.21:8080")),
                new BitTorrentPeer(NullLogger<BitTorrentPeer>.Instance, MockMetaInfo(), null, null, ProtocolExtension.None, null, new FakeTransportStream("192.168.1.22:8080")),
                new BitTorrentPeer(NullLogger<BitTorrentPeer>.Instance, MockMetaInfo(), null, null, ProtocolExtension.None, null, new FakeTransportStream("192.168.1.23:8080")),
            };
    }

    private Metainfo MockMetaInfo()
    {
        var fakeHash = new Sha1Hash(new byte[20]);
        var fakePieces = new List<Piece>
            {
                new Piece(0, 100, fakeHash),
            };

        return new Metainfo("Mock", fakeHash, new List<ContainedFile>(), fakePieces, new List<List<Uri>>(), new List<byte>());
    }
}
