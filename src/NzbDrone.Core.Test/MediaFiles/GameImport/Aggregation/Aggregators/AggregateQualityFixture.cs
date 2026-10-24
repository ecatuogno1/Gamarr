using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.GameImport.Aggregation.Aggregators;
using NzbDrone.Core.MediaFiles.GameImport.Aggregation.Aggregators.Augmenters.Quality;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.GameImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateQualityFixture : CoreTest<AggregateQuality>
    {
        private Mock<IAugmentQuality> _fileNameAugmenter;
        private Mock<IAugmentQuality> _folderAugmenter;
        private Mock<IAugmentQuality> _releaseNameAugmenter;

        [SetUp]
        public void Setup()
        {
            _fileNameAugmenter = new Mock<IAugmentQuality>();
            _folderAugmenter = new Mock<IAugmentQuality>();
            _releaseNameAugmenter = new Mock<IAugmentQuality>();

            _fileNameAugmenter.SetupGet(s => s.Order).Returns(1);
            _folderAugmenter.SetupGet(s => s.Order).Returns(2);
            _releaseNameAugmenter.SetupGet(s => s.Order).Returns(5);

            // A .chd file, the folder doesn't say anything and the grabbed release was a Redump set
            _fileNameAugmenter.Setup(s => s.AugmentQuality(It.IsAny<LocalGame>(), It.IsAny<DownloadClientItem>()))
                              .Returns(new AugmentQualityResult(Quality.DiscChdRvz, Confidence.Fallback, new Revision(), Confidence.Fallback));

            _folderAugmenter.Setup(s => s.AugmentQuality(It.IsAny<LocalGame>(), It.IsAny<DownloadClientItem>()))
                            .Returns(new AugmentQualityResult(Quality.Unknown, Confidence.Fallback, new Revision(), Confidence.Fallback));

            _releaseNameAugmenter.Setup(s => s.AugmentQuality(It.IsAny<LocalGame>(), It.IsAny<DownloadClientItem>()))
                                 .Returns(new AugmentQualityResult(Quality.DiscRedump, Confidence.Tag, new Revision(), Confidence.Fallback));
        }

        private void GivenAugmenters(params Mock<IAugmentQuality>[] mocks)
        {
            Mocker.SetConstant<IEnumerable<IAugmentQuality>>(mocks.Select(c => c.Object));
        }

        [Test]
        public void should_return_quality_from_extension_when_other_augments_are_null()
        {
            var nullMock = new Mock<IAugmentQuality>();
            nullMock.Setup(s => s.AugmentQuality(It.IsAny<LocalGame>(), It.IsAny<DownloadClientItem>()))
                    .Returns<LocalGame, DownloadClientItem>((l, d) => null);

            GivenAugmenters(_fileNameAugmenter, nullMock);

            var result = Subject.Aggregate(new LocalGame(), null);

            result.Quality.SourceDetectionSource.Should().Be(QualityDetectionSource.Extension);
            result.Quality.Quality.Should().Be(Quality.DiscChdRvz);
        }

        [Test]
        public void should_ignore_unknown_quality()
        {
            GivenAugmenters(_folderAugmenter, _fileNameAugmenter);

            var result = Subject.Aggregate(new LocalGame(), null);

            result.Quality.Quality.Should().Be(Quality.DiscChdRvz);
        }

        [Test]
        public void should_prefer_tagged_release_name_over_extension()
        {
            GivenAugmenters(_fileNameAugmenter, _folderAugmenter, _releaseNameAugmenter);

            var result = Subject.Aggregate(new LocalGame(), new DownloadClientItem());

            result.Quality.SourceDetectionSource.Should().Be(QualityDetectionSource.Name);
            result.Quality.Quality.Should().Be(Quality.DiscRedump);
        }

        [Test]
        public void should_prefer_first_quality_when_confidence_is_the_same()
        {
            _folderAugmenter.Setup(s => s.AugmentQuality(It.IsAny<LocalGame>(), It.IsAny<DownloadClientItem>()))
                            .Returns(new AugmentQualityResult(Quality.DiscIso, Confidence.Fallback, new Revision(), Confidence.Fallback));

            GivenAugmenters(_fileNameAugmenter, _folderAugmenter);

            var result = Subject.Aggregate(new LocalGame(), null);

            result.Quality.Quality.Should().Be(Quality.DiscChdRvz);
        }

        [Test]
        public void should_return_unknown_when_nothing_is_known()
        {
            GivenAugmenters(_folderAugmenter);

            var result = Subject.Aggregate(new LocalGame(), null);

            result.Quality.Quality.Should().Be(Quality.Unknown);
        }

        [Test]
        public void should_return_version_1_when_no_version_specified()
        {
            GivenAugmenters(_fileNameAugmenter, _releaseNameAugmenter);

            var result = Subject.Aggregate(new LocalGame(), new DownloadClientItem());

            result.Quality.Revision.Version.Should().Be(1);
            result.Quality.RevisionDetectionSource.Should().Be(QualityDetectionSource.Unknown);
        }

        [Test]
        public void should_return_version_2_when_name_indicates_proper()
        {
            _fileNameAugmenter.Setup(s => s.AugmentQuality(It.IsAny<LocalGame>(), It.IsAny<DownloadClientItem>()))
                              .Returns(new AugmentQualityResult(Quality.PcScene, Confidence.Tag, new Revision(2), Confidence.Tag));

            GivenAugmenters(_fileNameAugmenter, _releaseNameAugmenter);

            var result = Subject.Aggregate(new LocalGame(), new DownloadClientItem());

            result.Quality.Revision.Version.Should().Be(2);
            result.Quality.RevisionDetectionSource.Should().Be(QualityDetectionSource.Name);
        }

        [Test]
        public void should_return_version_2_when_file_name_indicates_v0_and_release_name_indicates_v2()
        {
            _fileNameAugmenter.Setup(s => s.AugmentQuality(It.IsAny<LocalGame>(), It.IsAny<DownloadClientItem>()))
                              .Returns(new AugmentQualityResult(Quality.PcScene, Confidence.Tag, new Revision(0), Confidence.Tag));

            _releaseNameAugmenter.Setup(s => s.AugmentQuality(It.IsAny<LocalGame>(), It.IsAny<DownloadClientItem>()))
                                 .Returns(new AugmentQualityResult(Quality.PcScene, Confidence.Tag, new Revision(2), Confidence.Tag));

            GivenAugmenters(_fileNameAugmenter, _releaseNameAugmenter);

            var result = Subject.Aggregate(new LocalGame(), new DownloadClientItem());

            result.Quality.Revision.Version.Should().Be(2);
            result.Quality.RevisionDetectionSource.Should().Be(QualityDetectionSource.Name);
        }
    }
}
