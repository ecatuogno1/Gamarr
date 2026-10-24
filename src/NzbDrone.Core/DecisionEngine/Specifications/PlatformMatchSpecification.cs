using System.Linq;
using NLog;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Platforms;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    // A release has to be for the game's platform. Releases that don't name their platform are only accepted
    // from searches (they're scoped to the platform's categories), not from RSS, unless the indexer categories of the
    // release name the game's platform. Categories that belong to another platform reject the release.
    public class PlatformMatchSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IPlatformCategoryMapper _platformCategoryMapper;
        private readonly Logger _logger;

        public PlatformMatchSpecification(IPlatformCategoryMapper platformCategoryMapper, Logger logger)
        {
            _platformCategoryMapper = platformCategoryMapper;
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
                var categories = subject.Release?.Categories;

                var categoryPlatforms = _platformCategoryMapper.GetPlatforms(subject.Release?.Categories);

                if (categoryPlatforms.Any())
                {
                    // PC/Games (4050) maps to Windows, but it's used for every PC platform (DOS, Linux, Mac)
                    if (categoryPlatforms.Any(p => p.Id == gamePlatform.Id ||
                                                   PlatformParser.AreCompatible(gamePlatform.IgdbPlatformId, p.IgdbPlatformId) ||
                                                   (p.Family == PlatformFamily.PC && gamePlatform.Family == PlatformFamily.PC)))
                    {
                        _logger.Debug("Release has no platform, its indexer categories match {0}", gamePlatform.Name);
                        return DownloadSpecDecision.Accept();
                    }

                    var categoryPlatformNames = string.Join(", ", categoryPlatforms.Select(p => p.Name));

                    _logger.Debug("Release '{0}' is in the categories of {1}, the game is on {2}", subject.Release?.Title, categoryPlatformNames, gamePlatform.Name);
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongPlatform, "Release is in the indexer categories of {0}, the game is on {1}", categoryPlatformNames, gamePlatform.Name);
                }

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
                var releasePlatformName = PlatformDefinitions.Get(releasePlatform)?.Name ?? releasePlatform.ToString();

                _logger.Debug("Release is for {0}, the game is on {1}", releasePlatformName, gamePlatform.Name);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongPlatform, "Release is for {0}, the game is on {1}", releasePlatformName, gamePlatform.Name);
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
