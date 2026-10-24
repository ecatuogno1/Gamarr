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
                .With(q => q.Quality = Quality.RomUnverified)
                .Build();

            _remoteGame = new RemoteGame
            {
                Game = _game,
                Release = new ReleaseInfo(),
                ParsedGameInfo = new ParsedGameInfo { Quality = new QualityModel(Quality.RomUnverified, new Revision(version: 2)) },
            };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<IQualityDefinitionService>().Setup(s => s.Get(Quality.RomUnverified)).Returns(_qualityType);
        }

        // Sizes are absolute, in GB: between 2 GB and 10 GB
        [TestCase(1024, false)]
        [TestCase(5120, true)]
        [TestCase(11264, false)]
        public void should_check_size_in_gigabytes(int sizeInMegaBytes, bool expectedResult)
        {
            _remoteGame.Game = _game;
            _remoteGame.Release.Size = sizeInMegaBytes.Megabytes();

            Subject.IsSatisfiedBy(_remoteGame, new()).Accepted.Should().Be(expectedResult);
        }

        [Test]
        public void should_allow_fractional_gigabytes_for_roms()
        {
            _qualityType.MinSize = 0;
            _qualityType.MaxSize = 0.01;

            _remoteGame.Release.Size = 4.Megabytes();
            Subject.IsSatisfiedBy(_remoteGame, new()).Accepted.Should().BeTrue();

            _remoteGame.Release.Size = 20.Megabytes();
            Subject.IsSatisfiedBy(_remoteGame, new()).Accepted.Should().BeFalse();
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
        public void should_return_true_if_unlimited_small()
        {
            _remoteGame.Game = _game;
            _remoteGame.Release.Size = 18457280000;
            _qualityType.MaxSize = null;

            Subject.IsSatisfiedBy(_remoteGame, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_unlimited_large()
        {
            _remoteGame.Game = _game;
            _remoteGame.Release.Size = 36857280000;
            _qualityType.MaxSize = null;

            Subject.IsSatisfiedBy(_remoteGame, new()).Accepted.Should().BeTrue();
        }
    }
}
