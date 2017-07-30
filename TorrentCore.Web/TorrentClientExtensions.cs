using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace TorrentCore.Web
{
    public static class TorrentClientExtensions
    {
        public static Uri EnableWebUI(this TorrentClient client, int port)
        {
            var uri = new Uri($"http://127.0.0.1:{port}");

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls(uri.ToString())
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .ConfigureServices(s =>
                {
                    s.Add(new ServiceDescriptor(typeof(TorrentClient), client));
                })
                .Build();

            host.Start();

            return uri;
        }
    }
}
