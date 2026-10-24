using System.IO;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.GameImport.Specifications
{
    // A game file has to be in one of the formats of the game's platform (or a generic disc image/archive),
    // an .nsp isn't a SNES game.
    public class MatchesPlatformSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MatchesPlatformSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalGame localGame, DownloadClientItem downloadClientItem)
        {
            var platform = localGame.Game?.Platform;

            if (platform == null)
            {
                return ImportSpecDecision.Accept();
            }

            var extension = Path.GetExtension(localGame.Path);

            if (MediaFileExtensions.IsAllowedForPlatform(extension, platform))
            {
                return ImportSpecDecision.Accept();
            }

            _logger.Debug("{0} isn't a {1} file", localGame.Path, platform.Name);

            return ImportSpecDecision.Reject(ImportRejectionReason.WrongPlatformFile, $"'{extension}' isn't a {platform.Name} file format");
        }
    }
}
