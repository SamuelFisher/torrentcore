// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Open.Nat;

namespace TorrentCore.Transport.Tcp
{
    class TcpTransportUPnPClient : IUPnPClient
    {
        private bool _useUPnP;

        public TcpTransportUPnPClient(bool useUPnP)
        {
            _useUPnP = useUPnP;
        }

        public async Task TryAddPortMappingAsync(int port)
        {
            if (!_useUPnP)
                return;

            try
            {
                var discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(10000);

                var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
                await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, port, port, "TorrentCore"));
            }
            catch
            {
                // Do nothing
            }
        }
    }
}
