using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TorrentCore.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            LogManager.Configure(factory => factory.AddConsole(LogLevel.Information));
            var client = new TorrentClient(0);

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls("http://localhost:5050")
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .ConfigureServices(s =>
                {
                    s.Add(new ServiceDescriptor(typeof(TorrentClient), client));
                })
                .Build();

            var download = client.Add(@"C:\Users\Sam\Downloads\Torrent\ubuntu-17.04-desktop-amd64.iso.torrent",
                                      @"C:\Users\Sam\Downloads\Torrent\DL");
            download.Start().Wait();

            Console.WriteLine("Downloading...");

            host.Start();

            download.WaitForCompletionAsync().Wait();
        }
    }
}
