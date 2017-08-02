using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TorrentCore.Transport.Tcp
{
    public class LocalTcpConnectionDetails
    {
        public LocalTcpConnectionDetails(int port, IPAddress publicAddress, IPAddress bindAddress)
        {
            Port = port;
            PublicAddress = publicAddress;
            BindAddress = bindAddress;
        }

        /// <summary>
        /// Gets the port on which incoming connections can be made.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Gets the public address that is used to listen for incoming connections.
        /// </summary>
        public IPAddress PublicAddress { get; }

        /// <summary>
        /// Gets the address of the local adapter used for connections.
        /// </summary>
        public IPAddress BindAddress { get; }
    }
}
