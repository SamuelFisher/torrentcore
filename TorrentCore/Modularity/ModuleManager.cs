// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentCore.Modularity
{
    class ModuleManager : IModuleManager
    {
        private readonly List<IModule> modules = new List<IModule>();

        public IEnumerable<IModule> Modules => modules;

        public void Register(IModule module)
        {
            modules.Add(module);
        }
    }
}
