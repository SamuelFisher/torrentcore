// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TorrentCore.Data;

namespace TorrentCore.Application
{
    class ApplicationProtocolFactory<TApplicationProtocol> : IApplicationProtocolFactory
        where TApplicationProtocol : IApplicationProtocol
    {
        private readonly IServiceProvider _services;

        public ApplicationProtocolFactory(IServiceProvider services)
        {
            _services = services;
        }

        public IApplicationProtocol Create(Metainfo metainfo, IBlockDataHandler blockDataHandler)
        {
            return ActivatorUtilities.CreateInstance<TApplicationProtocol>(_services, metainfo, blockDataHandler);
        }
    }
}
