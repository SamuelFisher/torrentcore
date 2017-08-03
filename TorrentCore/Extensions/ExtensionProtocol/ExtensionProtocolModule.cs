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
using System.Text;
using TorrentCore.ExtensionModule;

namespace TorrentCore.Extensions.ExtensionProtocol
{
    /// <summary>
    /// Provides support for BEP 10 Extension Protocol.
    /// </summary>
    public class ExtensionProtocolModule : IExtensionModule
    {
        private const int ExtensionProtocolMessageId = 20;
        private const string SupportsExtensionProtocol = "EXTENSION_PROTOCOL";

        public void OnPrepareHandshake(IPrepareHandshakeContext context)
        {
            // Advertise support for the extension protocol
            context.ReservedBytes[5] |= 0x10;
        }

        public void OnPeerConnected(IPeerContext context)
        {
            // Check for extension protocol support
            context.SetValue(SupportsExtensionProtocol, (context.ReservedBytes[5] & 0x10) != 0);
        }

        public void OnMessageReceived(IMessageReceivedContext context)
        {
            if (context.MessageId != ExtensionProtocolMessageId)
                return;

            bool supportsExtensionProtocol = context.GetValue<bool>(SupportsExtensionProtocol);

            context.Handle();
        }
    }
}
