// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TorrentCore.Extensions.ExtensionProtocol;
using TorrentCore.Extensions.PeerExchange;
using TorrentCore.Extensions.SendMetadata;
using TorrentCore.Modularity;

namespace TorrentCore.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        return await BuildCommandLine()
            .UseHost()
            .UseDefaults()
            .Build()
            .InvokeAsync(args);
    }

    private static int Run(Options options)
    {
        var builder = TorrentClientBuilder.CreateDefaultBuilder();

        // Configure logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(options.Verbose ? Serilog.Events.LogEventLevel.Debug : Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        builder.ConfigureServices(services => services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true)));

        // Listen for incoming connections on the specified port
        builder.UsePort(options.Port);

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

        var download = client.Add(options.Input.FullName, options.Output.FullName);
        download.Start();

        Console.WriteLine("Downloading...");

        using (var timer = new System.Timers.Timer(TimeSpan.FromSeconds(1).TotalMilliseconds))
        {
            timer.Elapsed += (o, e) => LogStatus(download);
            timer.Start();

            download.WaitForDownloadCompletionAsync().Wait();
        }
        Console.ReadKey();

        return 0;
    }

    private static CommandLineBuilder BuildCommandLine()
    {
        var root = new RootCommand()
            {
                new Option<int>(new[] { "-p", "--port" }, "Port to listen for incoming connections on.")
                {
                    IsRequired = true,
                },
                new Option<DirectoryInfo>(new[] { "-o", "--output" }, "Path to save downloaded files to.")
                {
                    IsRequired = true,
                },
                new Option<bool>(new[] { "v", "verbose" }, "Show detailed logging information."),
                new Argument<FileInfo>("--input", "Path of torrent file to download.")
                {
                    Arity = ArgumentArity.ExactlyOne,
                },
            };
        root.Handler = CommandHandler.Create<Options>(Run);
        return new CommandLineBuilder(root);
    }

    private static void LogStatus(TorrentDownload download)
    {
        Console.WriteLine($"{download.State} ({download.Progress:P})");
    }

    #nullable disable
    public record Options
    {
        public int Port { get; init; }

        public DirectoryInfo Output { get; init; }

        public bool Verbose { get; init; }

        public FileInfo Input { get; init; }
    }
    #nullable enable
}
