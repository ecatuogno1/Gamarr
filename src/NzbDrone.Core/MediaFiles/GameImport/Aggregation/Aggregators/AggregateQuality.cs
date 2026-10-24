using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.GameImport.Aggregation.Aggregators.Augmenters.Quality;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.GameImport.Aggregation.Aggregators
{
    public class AggregateQuality : IAggregateLocalGame
    {
        public int Order => 1;

        private readonly List<IAugmentQuality> _augmentQualities;
        private readonly Logger _logger;

        public AggregateQuality(IEnumerable<IAugmentQuality> augmentQualities,
                                Logger logger)
        {
            _augmentQualities = augmentQualities.OrderBy(a => a.Order).ToList();
            _logger = logger;
        }

        public LocalGame Aggregate(LocalGame localGame, DownloadClientItem downloadClientItem)
        {
            var quality = Quality.Unknown;
            var qualityConfidence = Confidence.Default;
            var revision = new Revision(1);
            var revisionConfidence = Confidence.Default;

            foreach (var augmentQuality in _augmentQualities)
            {
                var augmentedQuality = augmentQuality.AugmentQuality(localGame, downloadClientItem);

                if (augmentedQuality == null)
                {
                    continue;
                }

                _logger.Trace("Considering Quality {0} ({1}) Revision {2} from {3}", augmentedQuality.Quality, augmentedQuality.QualityConfidence, augmentedQuality.Revision, augmentQuality.Name);

                // The first known quality wins unless a later one was detected with more confidence (a tag in the
                // release name beats an extension)
                if (augmentedQuality.Quality != null && augmentedQuality.Quality != Quality.Unknown &&
                    (quality == Quality.Unknown || augmentedQuality.QualityConfidence > qualityConfidence))
                {
                    quality = augmentedQuality.Quality;
                    qualityConfidence = augmentedQuality.QualityConfidence;
                }

                if (augmentedQuality.Revision != null)
                {
                    // Update the revision and confidence if it is higher than the current confidence,
                    // this will allow explicitly detected v0 to override the v1 default.
                    if (augmentedQuality.RevisionConfidence > revisionConfidence)
                    {
                        revision = augmentedQuality.Revision;
                        revisionConfidence = augmentedQuality.RevisionConfidence;
                    }

                    // Update the revision and confidence if it is the same confidence and the revision is higher,
                    // this will allow the best revision to be used in the event there is a disagreement.
                    else if (augmentedQuality.RevisionConfidence == revisionConfidence &&
                             augmentedQuality.Revision > revision)
                    {
                        revision = augmentedQuality.Revision;
                        revisionConfidence = augmentedQuality.RevisionConfidence;
                    }
                }
            }

            _logger.Trace("Selected Quality {0} ({1}) Revision {2}", quality, qualityConfidence, revision);

            var result = new QualityModel(quality, revision)
            {
                SourceDetectionSource = qualityConfidence == Confidence.Tag ? QualityDetectionSource.Name : QualityDetectionSource.Extension,
                RevisionDetectionSource = revisionConfidence == Confidence.Tag ? QualityDetectionSource.Name : QualityDetectionSource.Unknown
            };

            _logger.Debug("Using quality: {0}", result);

            localGame.Quality = result;

            return localGame;
        }
    }
}
