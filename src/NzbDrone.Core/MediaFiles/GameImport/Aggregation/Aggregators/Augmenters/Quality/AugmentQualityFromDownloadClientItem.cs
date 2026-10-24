using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.GameImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromDownloadClientItem : IAugmentQuality
    {
        public int Order => 3;
        public string Name => "DownloadClientItem";

        public AugmentQualityResult AugmentQuality(LocalGame localGame, DownloadClientItem downloadClientItem)
        {
            return AugmentQualityResult.From(localGame.DownloadClientGameInfo?.Quality);
        }
    }
}
