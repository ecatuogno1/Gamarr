using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.GameImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityResult
    {
        public string Name { get; set; }
        public Qualities.Quality Quality { get; set; }
        public Confidence QualityConfidence { get; set; }
        public Revision Revision { get; set; }
        public Confidence RevisionConfidence { get; set; }

        public AugmentQualityResult(Qualities.Quality quality,
                                    Confidence qualityConfidence,
                                    Revision revision,
                                    Confidence revisionConfidence)
        {
            Quality = quality;
            QualityConfidence = qualityConfidence;
            Revision = revision;
            RevisionConfidence = revisionConfidence;
        }

        public static AugmentQualityResult From(QualityModel quality)
        {
            if (quality == null)
            {
                return null;
            }

            var qualityConfidence = quality.SourceDetectionSource == QualityDetectionSource.Name
                ? Confidence.Tag
                : Confidence.Fallback;

            var revisionConfidence = quality.RevisionDetectionSource == QualityDetectionSource.Name
                ? Confidence.Tag
                : Confidence.Fallback;

            return new AugmentQualityResult(quality.Quality, qualityConfidence, quality.Revision, revisionConfidence);
        }
    }
}
