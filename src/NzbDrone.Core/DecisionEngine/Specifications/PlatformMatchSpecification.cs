using NLog;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    // A release has to be for the game's platform. Releases that don't name their platform are only accepted
    // from searches (they're scoped to the platform's categories), not from RSS.
    public class PlatformMatchSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public PlatformMatchSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteGame subject, ReleaseDecisionInformation information)
        {
            var gamePlatform = subject.Game?.Platform;

            if (gamePlatform == null)
            {
                return DownloadSpecDecision.Accept();
            }

            var releasePlatform = subject.ParsedGameInfo?.PlatformIgdbId ?? 0;

            if (releasePlatform == 0)
            {
                if (information?.SearchCriteria != null || subject.ReleaseSource is ReleaseSourceType.InteractiveSearch or ReleaseSourceType.UserInvokedSearch or ReleaseSourceType.Search)
                {
                    _logger.Debug("Release has no platform, accepting it from a platform search");
                    return DownloadSpecDecision.Accept();
                }

                _logger.Debug("Release '{0}' doesn't name its platform, rejecting", subject.Release?.Title);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.UnknownPlatform, "Platform couldn't be determined from the release, only searches accept it");
            }

            if (!PlatformParser.AreCompatible(gamePlatform.IgdbPlatformId, releasePlatform))
            {
                var releasePlatformName = Platforms.PlatformDefinitions.Get(releasePlatform)?.Name ?? releasePlatform.ToString();

                _logger.Debug("Release is for {0}, the game is on {1}", releasePlatformName, gamePlatform.Name);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongPlatform, "Release is for {0}, the game is on {1}", releasePlatformName, gamePlatform.Name);
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
