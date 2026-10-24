using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.GameImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromFolder : IAugmentQuality
    {
        public int Order => 2;
        public string Name => "FolderName";

        public AugmentQualityResult AugmentQuality(LocalGame localGame, DownloadClientItem downloadClientItem)
        {
            return AugmentQualityResult.From(localGame.FolderGameInfo?.Quality);
        }
    }
}
