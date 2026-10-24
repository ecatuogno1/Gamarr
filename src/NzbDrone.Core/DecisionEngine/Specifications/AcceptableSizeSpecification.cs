using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    // Quality size limits are absolute sizes in GB (games have no runtime to scale them by)
    public class AcceptableSizeSpecification : IDownloadDecisionEngineSpecification
    {
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

            if (qualityDefinition.MinSize.HasValue && qualityDefinition.MinSize.Value > 0)
            {
                var minSize = qualityDefinition.MinSize.Value.Gigabytes();

                if (subject.Release.Size < minSize)
                {
                    _logger.Debug("Item: {0}, Size: {1} is smaller than minimum allowed size ({2} bytes for {3}), rejecting.", subject, subject.Release.Size, minSize, quality);
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.BelowMinimumSize, "{0} is smaller than minimum allowed {1} for {2}", subject.Release.Size.SizeSuffix(), minSize.SizeSuffix(), quality);
                }
            }

            if (!qualityDefinition.MaxSize.HasValue || qualityDefinition.MaxSize.Value == 0)
            {
                _logger.Debug("Max size is unlimited, skipping check");
            }
            else
            {
                var maxSize = qualityDefinition.MaxSize.Value.Gigabytes();

                if (subject.Release.Size > maxSize)
                {
                    _logger.Debug("Item: {0}, Size: {1} is greater than maximum allowed size ({2} for {3}), rejecting", subject, subject.Release.Size, maxSize, quality);
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.AboveMaximumSize, "{0} is larger than maximum allowed {1} for {2}", subject.Release.Size.SizeSuffix(), maxSize.SizeSuffix(), quality);
                }
            }

            _logger.Debug("Item: {0}, meets size constraints", subject);
            return DownloadSpecDecision.Accept();
        }
    }
}
