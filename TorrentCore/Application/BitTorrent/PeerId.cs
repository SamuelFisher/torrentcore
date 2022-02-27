// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace TorrentCore.Application.BitTorrent;

public sealed class PeerId
{
    private static readonly Dictionary<string, string> ClientIds;

    static PeerId()
    {
        var assembly = typeof(PeerId).GetTypeInfo().Assembly;
        using (var peerIdClientsStream = new StreamReader(assembly.GetManifestResourceStream("TorrentCore.Transport.ClientPeerIds.txt")!))
        {
            ClientIds =
                peerIdClientsStream.ReadToEnd()
                                   .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(x => x.Trim().Split('='))
                                   .ToDictionary(x => x[0], x => x[1]);
        }
    }

    public PeerId(byte[] peerId)
    {
        if (peerId.Length != 20)
            throw new ArgumentException("Peer ID must be 20 bytes long.", nameof(peerId));

        Value = new ReadOnlyCollection<byte>(peerId);

        string str = ToString();
        if (str[0] == '-' && str[7] == '-')
        {
            // Azureus-style peer id
            string clientId = str.Substring(1, 2);
            if (!ClientIds.TryGetValue(clientId, out var clientName))
                clientName = null;
            ClientName = clientName;

            string clientVersion = str.Substring(3, 4);
            int version;
            if (int.TryParse(clientVersion, out version))
                ClientVersion = version;
        }
    }

    /// <summary>
    /// Gets the name of the BitTorrent client the peer is using.
    /// <remarks>Null if the client name cannot be determined.</remarks>
    /// </summary>
    public string? ClientName { get; }

    /// <summary>
    /// Gets the version of the BitTorrent client the peer is using.
    /// <remarks>Null if the client version cannot be determined.</remarks>
    /// </summary>
    public int? ClientVersion { get; }

    public IReadOnlyList<byte> Value { get; }

    public override string ToString()
    {
        return Encoding.UTF8.GetString(Value.ToArray());
    }

    /// <summary>
    /// Creates a new, random PeerId.
    /// </summary>
    public static PeerId CreateNew()
    {
        var rand = new Random();
        byte[] identifier = Encoding.ASCII.GetBytes("-TC0001-");
        byte[] randomData = new byte[20 - identifier.Length];
        rand.NextBytes(randomData);
        var data = identifier.Concat(randomData).ToArray();
        return new PeerId(data);
    }
}
