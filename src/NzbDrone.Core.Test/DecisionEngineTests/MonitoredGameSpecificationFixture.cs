using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.Games;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class MonitoredGameSpecificationFixture : CoreTest<MonitoredGameSpecification>
    {
        private MonitoredGameSpecification _monitoredEpisodeSpecification;

        private RemoteGame _parseResultMulti;
        private RemoteGame _parseResultSingle;
        private Game _fakeGame;

        [SetUp]
        public void Setup()
        {
            _monitoredEpisodeSpecification = Mocker.Resolve<MonitoredGameSpecification>();

            _fakeGame = Builder<Game>.CreateNew()
                .With(c => c.Monitored = true)
                .Build();

            _parseResultMulti = new RemoteGame
            {
                Game = _fakeGame
            };

            _parseResultSingle = new RemoteGame
            {
                Game = _fakeGame
            };
        }

        private void WithGameUnmonitored()
        {
            _fakeGame.Monitored = false;
        }

        [Test]
        public void setup_should_return_monitored_episode_should_return_true()
        {
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void not_monitored_series_should_be_skipped()
        {
            _fakeGame.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_episode_not_monitored_should_return_false()
        {
            WithGameUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_for_single_episode_search()
        {
            _fakeGame.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new GameSearchCriteria { UserInvokedSearch = true })).Accepted.Should().BeTrue();
        }
    }
}
