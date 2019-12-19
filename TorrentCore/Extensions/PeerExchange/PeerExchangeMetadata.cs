using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentCore.Extensions.PeerExchange
{
    public class PeerExchangeMetadata
    {
        public static string Key => "PeerExchange::Metadata";

        public DateTime LastMessageDate { get; set; }

        public IEnumerable<string> ConnectedPeersSnapshot { get; set; } = new List<string>();
    }
}
