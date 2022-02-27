// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace TorrentCore.Application.Pipelines;

class Pipeline : IPipeline
{
    private readonly ILogger<Pipeline> _logger;
    private readonly IList<IPipelineStage> _stages;

    public Pipeline(ILogger<Pipeline> logger, IList<IPipelineStage> stages)
    {
        _logger = logger;
        _stages = stages;
    }

    public void Run(IStageInterrupt interrupt, IProgress<StatusUpdate> progress)
    {
        foreach (var stage in _stages)
        {
            _logger.LogInformation($"Starting pipeline stage {stage.GetType().Name}");
            try
            {
                stage.Run(interrupt, progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception in pipeline stage {stage.GetType().Name}");
                progress.Report(new StatusUpdate(DownloadState.Error, 0.0));
                return;
            }
            _logger.LogInformation($"Finished pipeline stage {stage.GetType().Name}");

            if (interrupt.IsStopRequested)
            {
                _logger.LogInformation("Stopping pipeline due to stop request");
                return;
            }
        }
    }
}
