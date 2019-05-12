// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TorrentCore.Application;

namespace TorrentCore.Pipelines
{
    public class PipelineBuilder
    {
        private readonly ImmutableList<Type> _stages;

        public PipelineBuilder()
        {
            _stages = ImmutableList<Type>.Empty;
        }

        private PipelineBuilder(IEnumerable<Type> stages)
        {
            _stages = stages.ToImmutableList();
        }

        public PipelineBuilder AddStage<T>()
            where T : IPipelineStage
        {
            return new PipelineBuilder(_stages.Add(typeof(T)));
        }

        public IPipelineFactory Build()
        {
            return new PipelineStageFactory(_stages);
        }

        private class PipelineStageFactory : IPipelineFactory
        {
            private readonly IReadOnlyList<Type> _stages;

            public PipelineStageFactory(IReadOnlyList<Type> stages)
            {
                _stages = stages;
            }

            public IPipeline CreatePipeline(IServiceProvider pipelineScope, IApplicationProtocol applicationProtocol)
            {
                var stages = _stages.Select(x => (IPipelineStage)ActivatorUtilities.CreateInstance(pipelineScope, x, applicationProtocol)).ToList();
                return ActivatorUtilities.CreateInstance<Pipeline>(pipelineScope, stages);
            }
        }
    }
}
