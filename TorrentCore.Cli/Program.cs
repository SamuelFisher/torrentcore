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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TorrentCore.Extensions.ExtensionProtocol;
using TorrentCore.Extensions.PeerExchange;
using TorrentCore.Extensions.SendMetadata;
using TorrentCore.Modularity;
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

            var builder = TorrentClientBuilder.CreateDefaultBuilder();

            // Configure logging
            builder.ConfigureServices(services => services.AddLogging(loggingBuilder =>
                loggingBuilder.AddConsole().SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information)));

            // Listen for incoming connections on the specified port
            builder.UsePort(port);

            // Add extension protocol
            builder.ConfigureServices(services =>
            {
                services.AddScoped<IModule>(s =>
                {
                    // TODO: Handle construction of message handlers inside ExtensionProtocolModule
                    var extensionProtocolModule = ActivatorUtilities.CreateInstance<ExtensionProtocolModule>(s);
                    extensionProtocolModule.RegisterMessageHandler(ActivatorUtilities.CreateInstance<PeerExchangeMessageHandler>(s));
                    extensionProtocolModule.RegisterMessageHandler(ActivatorUtilities.CreateInstance<MetadataMessageHandler>(s));
                    return extensionProtocolModule;
                });
            });

            var client = builder.Build();

            ////if (runWebUi)
            ////{
            ////    var uri = client.EnableWebUI(uiPort);
            ////    Console.WriteLine($"Web UI started at {uri}");
            ////}

            var download = client.Add(input, output);
            download.Start();

            Console.WriteLine("Downloading...");

            using (var timer = new System.Timers.Timer(TimeSpan.FromSeconds(1).TotalMilliseconds))
            {
                timer.Elapsed += (o, e) => LogStatus(download);
                timer.Start();

                download.WaitForDownloadCompletionAsync().Wait();
            }
            Console.ReadKey();
        }

        private static void LogStatus(TorrentDownload download)
        {
            Console.WriteLine($"{download.State} ({download.Progress:P})");
        }
    }
}
