using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Games;
using NzbDrone.Core.Indexers;
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
        private List<Platform> _platforms;

        [SetUp]
        public void Setup()
        {
            var id = 1;

            _platforms = PlatformDefinitions.All.Select(PlatformDefinitions.Copy).ToList();
            _platforms.ForEach(p => p.Id = id++);

            Mocker.GetMock<IPlatformService>()
                .Setup(s => s.All())
                .Returns(_platforms);

            Mocker.SetConstant<IPlatformCategoryMapper>(Mocker.Resolve<PlatformCategoryMapper>());

            _remoteGame = new RemoteGame
            {
                Game = new Game { Platform = Platform("SNES") },
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

        private Platform Platform(string shortName)
        {
            return _platforms.Single(p => p.ShortName == shortName);
        }

        [Test]
        public void should_accept_unknown_platform_from_rss_when_categories_match_game_platform()
        {
            _remoteGame.Game.Platform = Platform("PS3");
            _remoteGame.Release.Categories = new List<int> { 1000, 1080 };

            Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_unknown_platform_when_categories_are_for_another_platform()
        {
            _remoteGame.Release.Categories = new List<int> { 1000, 1080 };

            var decision = Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation(false, new GameSearchCriteria { Game = _remoteGame.Game }));

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.WrongPlatform);
        }

        [Test]
        public void should_reject_pc_category_release_for_console_game()
        {
            _remoteGame.Release.Categories = new List<int> { 4000, 4050 };

            var decision = Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation(false, new GameSearchCriteria { Game = _remoteGame.Game }));

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.WrongPlatform);
        }

        [Test]
        public void should_accept_pc_category_release_for_pc_game_from_rss()
        {
            _remoteGame.Game.Platform = _platforms.Single(p => p.IgdbPlatformId == PlatformCategoryMapper.PcWindowsIgdbPlatformId);
            _remoteGame.Release.Categories = new List<int> { 4050 };

            Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_use_search_rules_when_categories_dont_name_a_platform()
        {
            _remoteGame.Release.Categories = new List<int> { 1000 };

            Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation()).Accepted.Should().BeFalse();
            Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation(false, new GameSearchCriteria { Game = _remoteGame.Game })).Accepted.Should().BeTrue();
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
