// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2017 Sam Fisher.
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
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TorrentCore.Extensions.ExtensionProtocol;
using TorrentCore.Extensions.PeerExchange;
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

            var client = new TorrentClient(port);
            var extendionProtocolModule = new ExtensionProtocolModule();
            extendionProtocolModule.RegisterMessageHandler(new PeerExchangeMessageHandler(client.AdapterAddress));
            client.Modules.Register(extendionProtocolModule);

            if (runWebUi)
            {
                var uri = client.EnableWebUI(uiPort);
                Console.WriteLine($"Web UI started at {uri}");
            }

            var download = client.Add(input, output);
            download.Start().Wait();

            Console.WriteLine("Downloading...");

            download.WaitForCompletionAsync().Wait();
            Console.ReadKey();
        }
    }
}
