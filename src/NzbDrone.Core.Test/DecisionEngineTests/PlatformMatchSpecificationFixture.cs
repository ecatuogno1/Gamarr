using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Games;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class PlatformMatchSpecificationFixture : CoreTest<PlatformMatchSpecification>
    {
        private RemoteGame _remoteGame;

        [SetUp]
        public void Setup()
        {
            _remoteGame = new RemoteGame
            {
                Game = new Game { Platform = new Platform { Id = 5, Name = "Super Nintendo Entertainment System", IgdbPlatformId = PlatformParser.Snes } },
                Release = new ReleaseInfo { Title = "Super Mario World (USA)" },
                ParsedGameInfo = new ParsedGameInfo(),
                ReleaseSource = ReleaseSourceType.Rss
            };
        }

        [Test]
        public void should_accept_release_for_game_platform()
        {
            _remoteGame.ParsedGameInfo.PlatformIgdbId = PlatformParser.Snes;

            Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_release_for_compatible_platform()
        {
            _remoteGame.ParsedGameInfo.PlatformIgdbId = PlatformParser.SuperFamicom;

            Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_release_for_other_platform()
        {
            _remoteGame.ParsedGameInfo.PlatformIgdbId = PlatformParser.GameBoyAdvance;

            var decision = Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation());

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.WrongPlatform);
        }

        [Test]
        public void should_reject_unknown_platform_from_rss()
        {
            var decision = Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation());

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.UnknownPlatform);
        }

        [Test]
        public void should_accept_unknown_platform_from_search()
        {
            Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation(false, new GameSearchCriteria { Game = _remoteGame.Game })).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_unknown_platform_from_interactive_search()
        {
            _remoteGame.ReleaseSource = ReleaseSourceType.InteractiveSearch;

            Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_game_has_no_platform()
        {
            _remoteGame.Game.Platform = null;
            _remoteGame.ParsedGameInfo.PlatformIgdbId = PlatformParser.Ps4;

            Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation()).Accepted.Should().BeTrue();
        }
    }
}
