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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace TorrentCore.Web.Controllers
{
    public class EmbeddedStaticContentController : Controller
    {
        private readonly Dictionary<string, string> mediaTypes = new Dictionary<string, string>
        {
            [".html"] = "text/html",
            [".js"] = "text/javascript",
            [".css"] = "text/css"
        };

        public IActionResult Index()
        {
            string fileName = Request.Path.Value.Split('/').Last();
            string extension = Path.GetExtension(fileName);
            string mediaType;
            if (!mediaTypes.TryGetValue(extension, out mediaType))
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
