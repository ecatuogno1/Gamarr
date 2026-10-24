using NzbDrone.Core.Download;
using NzbDrone.Core.Download.History;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.GameImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromReleaseName : IAugmentQuality
    {
        public int Order => 5;
        public string Name => "ReleaseName";

        private readonly IDownloadHistoryService _downloadHistoryService;

        public AugmentQualityFromReleaseName(IDownloadHistoryService downloadHistoryService)
        {
            _downloadHistoryService = downloadHistoryService;
        }

        public AugmentQualityResult AugmentQuality(LocalGame localGame, DownloadClientItem downloadClientItem)
        {
            // Don't try to augment if we can't lookup the grabbed history by downloadId
            if (downloadClientItem == null)
            {
                return null;
            }

            var history = _downloadHistoryService.GetLatestGrab(downloadClientItem.DownloadId);
            if (history == null)
            {
                return null;
            }

            var platform = localGame.Game?.Platform?.IgdbPlatformId ?? 0;
            var historyQuality = platform > 0
                ? QualityParser.ParseQuality(history.SourceTitle, platform)
                : QualityParser.ParseQuality(history.SourceTitle);

            return AugmentQualityResult.From(historyQuality);
        }
    }
}
