// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

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
            var listenUri = new Uri($"http://localhost:{port}");
            return client.EnableWebUI(listenUri);
        }

        public static Uri EnableWebUI(this TorrentClient client, Uri listenUri)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls(listenUri.ToString())
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .ConfigureServices(s => { s.Add(new ServiceDescriptor(typeof(TorrentClient), client)); })
                .Build();

            host.Start();

            return listenUri;
        }
    }
}
