// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleInjector;

namespace TorrentCore.Stage
{
    class PipelineBuilder
    {
        private readonly List<IPipelineStageFactory> stages;

        public PipelineBuilder()
        {
            stages = new List<IPipelineStageFactory>();
        }

        private PipelineBuilder(IEnumerable<IPipelineStageFactory> stages)
        {
            this.stages = stages.ToList();
        }

        public PipelineBuilder AddStage<T>()
            where T : class, ITorrentStage
        {
            return new PipelineBuilder(stages.Concat(new[] { new PipelineStageFactory<T>() }));
        }

        public Pipeline Build()
        {
            return new Pipeline(stages);
        }

        private class PipelineStageFactory<T> : IPipelineStageFactory
            where T : class, ITorrentStage
        {
            public ITorrentStage Construct(Container container)
            {
                return container.GetInstance<T>();
            }
        }
    }
}
