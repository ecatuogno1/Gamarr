using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.History;
using NzbDrone.Core.MediaFiles.GameImport.Aggregation.Aggregators.Augmenters.Quality;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.GameImport.Aggregation.Aggregators.Augmenters.Quality
{
    [TestFixture]
    public class AugmentQualityFromReleaseNameFixture : CoreTest<AugmentQualityFromReleaseName>
    {
        private LocalGame _localGame;
        private DownloadClientItem _downloadClientItem;
        private ParsedGameInfo _hdtvParsedEpisodeInfo;
        private ParsedGameInfo _webdlParsedEpisodeInfo;

        [SetUp]
        public void Setup()
        {
            _hdtvParsedEpisodeInfo = Builder<ParsedGameInfo>.CreateNew()
                                                               .With(p => p.Quality =
                                                                   new QualityModel(Core.Qualities.Quality.DiscRedump))
                                                               .Build();

            _webdlParsedEpisodeInfo = Builder<ParsedGameInfo>.CreateNew()
                                                                .With(p => p.Quality =
                                                                    new QualityModel(Core.Qualities.Quality.NspXci))
                                                                .Build();

            _localGame = Builder<LocalGame>.CreateNew()
                                                 .Build();

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew()
                                                             .Build();
        }

        [Test]
        public void should_return_null_if_download_client_item_is_null()
        {
            Subject.AugmentQuality(_localGame, null).Should().BeNull();
        }

        [Test]
        public void should_return_null_if_no_grabbed_history()
        {
            Mocker.GetMock<IDownloadHistoryService>()
                  .Setup(s => s.GetLatestGrab(It.IsAny<string>()))
                  .Returns((DownloadHistory)null);

            Subject.AugmentQuality(_localGame, _downloadClientItem).Should().BeNull();
        }

        [TestCase("Game.Name.v1.2-CODEX", 133, Confidence.Tag)]
        [TestCase("Game Name (USA) [Redump]", 113, Confidence.Tag)]
        [TestCase("Game Name [FitGirl Repack]", 132, Confidence.Tag)]
        [TestCase("Game.Name.NSW-VENOM", 120, Confidence.Fallback)]
        public void should_return_augmented_quality(string title, int qualityId, Confidence confidence)
        {
            Mocker.GetMock<IDownloadHistoryService>()
                  .Setup(s => s.GetLatestGrab(It.IsAny<string>()))
                  .Returns(Builder<DownloadHistory>.CreateNew()
                                                   .With(h => h.SourceTitle = title)
                                                   .Build());

            _localGame.Game = null;

            var result = Subject.AugmentQuality(_localGame, _downloadClientItem);

            result.Should().NotBe(null);
            result.Quality.Id.Should().Be(qualityId);
            result.QualityConfidence.Should().Be(confidence);
        }
    }
}
