// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TorrentCore.Extensions.ExtensionProtocol;
using TorrentCore.Extensions.PeerExchange;
using TorrentCore.Extensions.SendMetadata;
using TorrentCore.Web;

namespace TorrentCore.Cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            int port = 5000;
            int uiPort = 5001;
            bool runWebUi = false;
            string input = null;
            string output = null;
            bool verbose = false;
            ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineOption("p|port", ref port, "Port to listen for incoming connections on.");
                syntax.DefineOption("o|output", ref output, "Path to save downloaded files to.");
                syntax.DefineOption("v|verbose", ref verbose, "Show detailed logging information.");
                var uiPortArgument = syntax.DefineOption("ui", ref uiPort, false, "Run a web UI, optionally specifying the port to listen on (default: 5001).");
                runWebUi = uiPortArgument.IsSpecified;

                syntax.DefineParameter("input", ref input, "Path of torrent file to download.");
            });

            if (verbose)
                LogManager.Configure(factory => factory.AddConsole(LogLevel.Debug));
            else
                LogManager.Configure(factory => factory.AddConsole(LogLevel.Information));

            var client = TorrentClient.Create(new TorrentClientSettings { ListenPort = port });
            var extensionProtocolModule = new ExtensionProtocolModule();
            ////extensionProtocolModule.RegisterMessageHandler(new PeerExchangeMessageHandler(client.AdapterAddress));
            extensionProtocolModule.RegisterMessageHandler(new MetadataMessageHandler());
            client.Modules.Register(extensionProtocolModule);

            if (runWebUi)
            {
                var uri = client.EnableWebUI(uiPort);
                Console.WriteLine($"Web UI started at {uri}");
            }

            var download = client.Add(input, output);
            download.Start();

            Console.WriteLine("Downloading...");

            download.WaitForDownloadCompletionAsync().Wait();
            Console.ReadKey();
        }
    }
}
