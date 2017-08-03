using System;
using System.Collections.Generic;
using System.Text;
using TorrentCore.ExtensionModule;

namespace TorrentCore.Application.BitTorrent.ExtensionModule
{
    /// <summary>
    /// Holds per-peer custom values that can be set by extension modules.
    /// </summary>
    class PeerCustomValues
    {
        private readonly Dictionary<PeerConnection, Dictionary<IExtensionModule, Dictionary<string, object>>> values =
            new Dictionary<PeerConnection, Dictionary<IExtensionModule, Dictionary<string, object>>>();

        public Dictionary<string, object> Get(PeerConnection peer, IExtensionModule module)
        {
            if (!values.TryGetValue(peer, out Dictionary<IExtensionModule, Dictionary<string, object>> peerValues))
            {
                peerValues = new Dictionary<IExtensionModule, Dictionary<string, object>>();
                values.Add(peer, peerValues);
            }

            if (!peerValues.TryGetValue(module, out Dictionary<string, object> moduleValues))
            {
                moduleValues = new Dictionary<string, object>();
                peerValues.Add(module, moduleValues);
            }

            return moduleValues;
        }

        public void PeerDisconnected(PeerConnection peer)
        {
            values.Remove(peer);
        }
    }
}
