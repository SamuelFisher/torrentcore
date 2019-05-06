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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TorrentCore.Web.Controllers
{
    public class EmbeddedStaticContentController : Controller
    {
        private readonly Dictionary<string, string> _mediaTypes = new Dictionary<string, string>
        {
            [".html"] = "text/html",
            [".js"] = "text/javascript",
            [".css"] = "text/css",
        };

        public IActionResult Index()
        {
            string fileName = Request.Path.Value.Split('/').Last();
            string extension = Path.GetExtension(fileName);
            string mediaType;
            if (!_mediaTypes.TryGetValue(extension, out mediaType))
                mediaType = "text/plain";
            var content = GetEmbeddedResource(fileName);

            if (content == null)
                return NotFound();

            return new FileStreamResult(content, mediaType);
        }

        public static Stream GetEmbeddedResource(string fileName)
        {
            var asm = typeof(EmbeddedStaticContentController).GetTypeInfo().Assembly;
            return asm.GetManifestResourceStream($"TorrentCore.Web.{fileName}");
        }
    }
}
