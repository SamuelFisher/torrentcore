// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using SimpleInjector;

namespace TorrentCore.Stage
{
    class Pipeline
    {
        private readonly IList<IPipelineStageFactory> _stageFactory;

        public Pipeline(IList<IPipelineStageFactory> stageFactory)
        {
            _stageFactory = stageFactory;
        }

        public void Run(Container container, IStageInterrupt interrupt, IProgress<StatusUpdate> progress)
        {
            foreach (var stage in _stageFactory)
            {
                var instance = stage.Construct(container);
                instance.Run(interrupt, progress);

                if (interrupt.IsStopRequested)
                    return;
            }
        }
    }
}
