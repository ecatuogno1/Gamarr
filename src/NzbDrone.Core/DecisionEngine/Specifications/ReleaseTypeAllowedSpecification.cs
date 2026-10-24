using System.Linq;
using NLog;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    // Updates, DLC, demos, hacks, translations and prototypes aren't the game, they're rejected unless the
    // quality profile gives a custom format with a matching release type condition a positive score.
    public class ReleaseTypeAllowedSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public ReleaseTypeAllowedSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteGame subject, ReleaseDecisionInformation information)
        {
            var releaseType = subject.ParsedGameInfo?.ReleaseType ?? ReleaseType.Base;

            if (!releaseType.IsRejectedByDefault())
            {
                return DownloadSpecDecision.Accept();
            }

            var profile = subject.Game?.QualityProfile;

            var wanted = profile != null && subject.CustomFormats.Any(format =>
                format.Specifications.OfType<ReleaseTypeSpecification>().Any(s => !s.Negate && s.Value == (int)releaseType) &&
                profile.FormatItems.Any(i => i.Format.Id == format.Id && i.Score > 0));

            if (wanted)
            {
                _logger.Debug("{0} release is wanted by a custom format", releaseType.GetName());
                return DownloadSpecDecision.Accept();
            }

            _logger.Debug("{0} release '{1}' isn't wanted", releaseType.GetName(), subject.Release?.Title);
            return DownloadSpecDecision.Reject(DownloadRejectionReason.ReleaseTypeNotWanted, "{0} releases aren't wanted (add a custom format with a positive score to allow them)", releaseType.GetName());
        }
    }
}
