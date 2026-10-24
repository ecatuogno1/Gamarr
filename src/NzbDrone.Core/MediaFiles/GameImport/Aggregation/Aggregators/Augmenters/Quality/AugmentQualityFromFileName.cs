using System.IO;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.GameImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromFileName : IAugmentQuality
    {
        public int Order => 1;
        public string Name => "FileName";

        public AugmentQualityResult AugmentQuality(LocalGame localGame, DownloadClientItem downloadClientItem)
        {
            var quality = localGame.FileGameInfo?.Quality;
            var platform = localGame.Game?.Platform;

            // The file name doesn't say which platform it's for, the game does. That's needed for ROMs in
            // archives and ambiguous extensions.
            if (platform != null && localGame.Path != null && (localGame.FileGameInfo == null || localGame.FileGameInfo.PlatformIgdbId == 0))
            {
                quality = QualityParser.ParseQuality(Path.GetFileName(localGame.Path), platform.IgdbPlatformId);
            }

            return AugmentQualityResult.From(quality);
        }
    }
}
