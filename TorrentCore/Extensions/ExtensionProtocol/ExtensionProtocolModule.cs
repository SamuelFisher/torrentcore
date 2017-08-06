// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2017 Samuel Fisher.
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
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using TorrentCore.Modularity;
using TorrentCore.Transport;

namespace TorrentCore.Extensions.ExtensionProtocol
{
    /// <summary>
    /// Provides support for BEP 10 Extension Protocol.
    /// </summary>
    public class ExtensionProtocolModule : IModule
    {
        private const byte ExtensionProtocolMessageId = 20;
        internal const string ExtensionProtocolMessageIds = "EXTENSION_PROTOCOL_MESSAGE_IDS";

        private static readonly ILogger Log = LogManager.GetLogger<ExtensionProtocolModule>();

        private readonly Dictionary<string, byte> supportedMessages = new Dictionary<string, byte>();
        private readonly Dictionary<byte, string> reverseSupportedMessages = new Dictionary<byte, string>();
        private readonly Dictionary<byte, IExtensionProtocolMessageHandler> messageHandlers =
            new Dictionary<byte, IExtensionProtocolMessageHandler>();
        private readonly HashSet<IExtensionProtocolMessageHandler> registeredHandlers =
            new HashSet<IExtensionProtocolMessageHandler>();

        private byte nextMessageTypeId = 1;

        public void RegisterMessageHandler(IExtensionProtocolMessageHandler messageHandler)
        {
            foreach (var messageType in messageHandler.SupportedMessageTypes)
            {
                Log.LogDebug($"Registering {messageHandler.GetType().Name} to receive {messageType.Key} messages using ID {nextMessageTypeId}");
                messageHandlers.Add(nextMessageTypeId, messageHandler);
                supportedMessages.Add(messageType.Key, nextMessageTypeId);
                reverseSupportedMessages.Add(nextMessageTypeId, messageType.Key);
                nextMessageTypeId++;
            }
            registeredHandlers.Add(messageHandler);
        }

        void IModule.OnPrepareHandshake(IPrepareHandshakeContext context)
        {
            // Advertise support for the extension protocol
            context.ReservedBytes[5] |= 0x10;
        }

        void IModule.OnPeerConnected(IPeerContext context)
        {
            // Check for extension protocol support
            bool supportsExtensionProtocol = (context.ReservedBytes[5] & 0x10) != 0;
            if (!supportsExtensionProtocol)
                return;

            // Register to receive extension protocol messages
            context.RegisterMessageHandler(ExtensionProtocolMessageId);

            // Send handshake message
            var handshake = new ExtensionProtocolHandshake
            {
                MessageIds = supportedMessages,
                Client = "TorrentCore 0.1" // todo
            };

            var handshakeDict = handshake.Serialize();
            var prepareHandshakeContext =
                new PrepareExtensionProtocolHandshakeContext(handshakeDict,
                                                             context,
                                                             msg => SendExtensionMessage(context, msg));
            foreach (var handler in registeredHandlers)
                handler.PrepareExtensionProtocolHandshake(prepareHandshakeContext);

            SendMessage(context, writer =>
            {
                writer.Write((byte)0);
                writer.Write(handshakeDict.EncodeAsBytes());
            });
        }

        void IModule.OnMessageReceived(IMessageReceivedContext context)
        {
            // We only registered to receive extension protocol messages
            // so we should only receive messages of this type.
            if (context.MessageId != ExtensionProtocolMessageId)
                throw new InvalidOperationException("Unsupported message type.");
            
            var messageTypeId = context.Reader.ReadByte();

            if (messageTypeId == 0)
            {
                HandshakeMessageReceived(context);

                foreach (var rh in registeredHandlers)
                    rh.PeerConnected(new ExtensionProtocolPeerContext(context, reply => SendExtensionMessage(context, reply)));

                return;
            }

            // Non-handshake message
            var handler = messageHandlers[messageTypeId];
            string messageTypeName = reverseSupportedMessages[messageTypeId];

            // Deserialize
            var message = handler.SupportedMessageTypes[messageTypeName]();
            message.Deserialize(context.Reader.ReadBytes(context.MessageLength - 1));

            var extensionMessageContext =
                new ExtensionProtocolMessageReceivedContext(message,
                                                            context,
                                                            reply => SendExtensionMessage(context, reply));
            handler.MessageReceived(extensionMessageContext);
        }

        private void HandshakeMessageReceived(IMessageReceivedContext context)
        {
            var data = context.Reader.ReadBytes(context.MessageLength - 1);
            var handshake = new ExtensionProtocolHandshake();
            handshake.Deserialize(data);
            context.SetValue(ExtensionProtocolMessageIds, handshake.MessageIds);
        }

        private void SendExtensionMessage(IPeerContext peerContext, IExtensionProtocolMessage message)
        {
            var peerMessageIds = peerContext.GetValue<Dictionary<string, byte>>(ExtensionProtocolMessageIds);

            if (!peerMessageIds.TryGetValue(message.MessageType, out byte messageType))
                throw new InvalidOperationException($"Peer does not support message type {message.MessageType}");

            SendMessage(peerContext, writer =>
            {
                writer.Write(messageType);
                writer.Write(message.Serialize());
            });
        }

        private void SendMessage(IPeerContext peerContext, Action<BinaryWriter> constructMessage)
        {
            using (var ms = new MemoryStream())
            {
                BinaryWriter writer = new BigEndianBinaryWriter(ms);
                constructMessage(writer);
                writer.Flush();
                peerContext.SendMessage(ExtensionProtocolMessageId, ms.ToArray());
            }
        }
    }
}
