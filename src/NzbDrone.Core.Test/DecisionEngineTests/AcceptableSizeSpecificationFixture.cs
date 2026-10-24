using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Games;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class AcceptableSizeSpecificationFixture : CoreTest<AcceptableSizeSpecification>
    {
        private Game _game;
        private RemoteGame _remoteGame;
        private QualityDefinition _qualityType;

        [SetUp]
        public void Setup()
        {
            _game = Builder<Game>.CreateNew().Build();

            _qualityType = Builder<QualityDefinition>.CreateNew()
                .With(q => q.MinSize = 2)
                .With(q => q.MaxSize = 10)
                .With(q => q.Quality = Quality.SDTV)
                .Build();

            _remoteGame = new RemoteGame
            {
                Game = _game,
                Release = new ReleaseInfo(),
                ParsedGameInfo = new ParsedGameInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
            };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<IQualityDefinitionService>().Setup(s => s.Get(Quality.SDTV)).Returns(_qualityType);
        }

        // Sizes are still per minute, multiplied by a fixed 110 minutes until stage 3 (2 MB * 110 = 220 MB, 10 MB * 110 = 1100 MB)
        [TestCase(100, false)]
        [TestCase(500, true)]
        [TestCase(1200, false)]
        public void should_check_size_against_fixed_multiplier(int sizeInMegaBytes, bool expectedResult)
        {
            _remoteGame.Game = _game;
            _remoteGame.Release.Size = sizeInMegaBytes.Megabytes();

            Subject.IsSatisfiedBy(_remoteGame, new()).Accepted.Should().Be(expectedResult);
        }

        [Test]
        public void should_return_true_if_size_is_zero()
        {
            _remoteGame.Game = _game;
            _remoteGame.Release.Size = 0;
            _qualityType.MinSize = 10;
            _qualityType.MaxSize = 20;

            Subject.IsSatisfiedBy(_remoteGame, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_unlimited_30_minute()
        {
            _remoteGame.Game = _game;
            _remoteGame.Release.Size = 18457280000;
            _qualityType.MaxSize = null;

            Subject.IsSatisfiedBy(_remoteGame, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_unlimited_60_minute()
        {
            _remoteGame.Game = _game;
            _remoteGame.Release.Size = 36857280000;
            _qualityType.MaxSize = null;

            Subject.IsSatisfiedBy(_remoteGame, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_use_110_minutes()
        {
            _remoteGame.Game = _game;
            _remoteGame.Release.Size = 1095.Megabytes();

            Subject.IsSatisfiedBy(_remoteGame, new()).Accepted.Should().Be(true);
            _remoteGame.Release.Size = 1105.Megabytes();
            Subject.IsSatisfiedBy(_remoteGame, new()).Accepted.Should().Be(false);
        }
    }
}
