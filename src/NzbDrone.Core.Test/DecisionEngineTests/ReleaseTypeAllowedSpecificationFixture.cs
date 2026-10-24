using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Games;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class ReleaseTypeAllowedSpecificationFixture : CoreTest<ReleaseTypeAllowedSpecification>
    {
        private RemoteGame _remoteGame;
        private CustomFormat _updateFormat;

        [SetUp]
        public void Setup()
        {
            _updateFormat = new CustomFormat("Updates", new ReleaseTypeSpecification { Value = (int)ReleaseType.Update }) { Id = 1 };

            _remoteGame = new RemoteGame
            {
                Game = new Game
                {
                    QualityProfile = new QualityProfile
                    {
                        FormatItems = new List<ProfileFormatItem> { new ProfileFormatItem { Format = _updateFormat, Score = 0 } }
                    }
                },
                Release = new ReleaseInfo { Title = "Game.Update.v1.2-CODEX" },
                ParsedGameInfo = new ParsedGameInfo { ReleaseType = ReleaseType.Update }
            };
        }

        [TestCase(ReleaseType.Base)]
        [TestCase(ReleaseType.Bundle)]
        [TestCase(ReleaseType.Homebrew)]
        public void should_accept_game_releases(ReleaseType releaseType)
        {
            _remoteGame.ParsedGameInfo.ReleaseType = releaseType;

            Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation()).Accepted.Should().BeTrue();
        }

        [TestCase(ReleaseType.Update)]
        [TestCase(ReleaseType.Dlc)]
        [TestCase(ReleaseType.Demo)]
        [TestCase(ReleaseType.Hack)]
        [TestCase(ReleaseType.Translation)]
        [TestCase(ReleaseType.Prototype)]
        public void should_reject_other_release_types_by_default(ReleaseType releaseType)
        {
            _remoteGame.ParsedGameInfo.ReleaseType = releaseType;

            var decision = Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation());

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.ReleaseTypeNotWanted);
        }

        [Test]
        public void should_accept_release_type_with_positive_custom_format_score()
        {
            _remoteGame.Game.QualityProfile.FormatItems[0].Score = 10;
            _remoteGame.CustomFormats = new List<CustomFormat> { _updateFormat };

            Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_release_type_with_matching_custom_format_without_score()
        {
            _remoteGame.CustomFormats = new List<CustomFormat> { _updateFormat };

            Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_reject_dlc_when_only_updates_are_wanted()
        {
            _remoteGame.Game.QualityProfile.FormatItems[0].Score = 10;
            _remoteGame.CustomFormats = new List<CustomFormat> { _updateFormat };
            _remoteGame.ParsedGameInfo.ReleaseType = ReleaseType.Dlc;

            Subject.IsSatisfiedBy(_remoteGame, new ReleaseDecisionInformation()).Accepted.Should().BeFalse();
        }
    }
}
