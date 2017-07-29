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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TorrentCore.Cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            int port = 5000;
            string input = null;
            string output = null;
            bool verbose = false;
            ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineOption("p|port", ref port, "Port to listen for incoming connections on.");
                syntax.DefineOption("i|input", ref input, "Path to torrent file to download.");
                syntax.DefineOption("o|output", ref output, "Path to save downloaded files to.");
                syntax.DefineOption("v|verbose", ref verbose, "Show more detailed information.");
            });

            if (verbose)
                LogManager.Configure(factory => factory.AddConsole(LogLevel.Debug));
            else
                LogManager.Configure(factory => factory.AddConsole(LogLevel.Information));

            var client = new TorrentClient(port);
            var download = client.Add(input, output);
            download.Start().Wait();

            Console.WriteLine("Downloading...");

            while (download.State == DownloadState.Downloading)
            {
                WriteProgressBar(Console.Out, download.Progress);
                Thread.Sleep(1000);
            }

            Console.WriteLine();
            Console.WriteLine("Completed.");
        }

        private static void WriteProgressBar(TextWriter writer, double progress)
        {
            const int totalWidth = 20;
            int width = (int)Math.Floor(progress * totalWidth);
            writer.Write($"\r[{new string('=', width)}{new string(' ', totalWidth - width)}] {progress:P0}");
        }
    }
}
