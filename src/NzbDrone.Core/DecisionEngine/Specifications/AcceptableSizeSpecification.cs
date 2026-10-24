using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AcceptableSizeSpecification : IDownloadDecisionEngineSpecification
    {
        // Quality sizes are still Radarr's MB-per-minute values, which used to be multiplied by the movie runtime.
        // Games have no runtime, so a fixed 110 minutes (Radarr's fallback) keeps the old limits until stage 3
        // replaces them with absolute sizes.
        private const int SizeMultiplier = 110;

        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly Logger _logger;

        public AcceptableSizeSpecification(IQualityDefinitionService qualityDefinitionService, Logger logger)
        {
            _qualityDefinitionService = qualityDefinitionService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteGame subject, ReleaseDecisionInformation information)
        {
            _logger.Debug("Beginning size check for: {0}", subject);

            var quality = subject.ParsedGameInfo.Quality.Quality;

            if (subject.Release.Size == 0)
            {
                _logger.Debug("Release has unknown size, skipping size check");
                return DownloadSpecDecision.Accept();
            }

            var qualityDefinition = _qualityDefinitionService.Get(quality);

            if (qualityDefinition.MinSize.HasValue)
            {
                var minSize = qualityDefinition.MinSize.Value.Megabytes();

                minSize = minSize * SizeMultiplier;

                // If the parsed size is smaller than minSize we don't want it
                if (subject.Release.Size < minSize)
                {
                    var runtimeMessage = subject.Game.Title;

                    _logger.Debug("Item: {0}, Size: {1} is smaller than minimum allowed size ({2} bytes for {3}), rejecting.", subject, subject.Release.Size, minSize, runtimeMessage);
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.BelowMinimumSize, "{0} is smaller than minimum allowed {1} (for {2})", subject.Release.Size.SizeSuffix(), minSize.SizeSuffix(), runtimeMessage);
                }
            }

            if (!qualityDefinition.MaxSize.HasValue || qualityDefinition.MaxSize.Value == 0)
            {
                _logger.Debug("Max size is unlimited, skipping check");
            }
            else
            {
                var maxSize = qualityDefinition.MaxSize.Value.Megabytes();

                maxSize = maxSize * SizeMultiplier;

                // If the parsed size is greater than maxSize we don't want it
                if (subject.Release.Size > maxSize)
                {
                    _logger.Debug("Item: {0}, Size: {1} is greater than maximum allowed size ({2} for {3}), rejecting", subject, subject.Release.Size, maxSize, subject.Game.Title);
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.AboveMaximumSize, "{0} is larger than maximum allowed {1} (for {2})", subject.Release.Size.SizeSuffix(), maxSize.SizeSuffix(), subject.Game.Title);
                }
            }

            _logger.Debug("Item: {0}, meets size constraints", subject);
            return DownloadSpecDecision.Accept();
        }
    }
}
