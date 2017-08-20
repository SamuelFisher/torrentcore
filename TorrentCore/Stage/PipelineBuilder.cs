// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2017 Samuel Fisher.
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
