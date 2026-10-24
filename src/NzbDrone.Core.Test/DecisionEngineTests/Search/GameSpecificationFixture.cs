using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications.Search;
using NzbDrone.Core.Games;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests.Search
{
    [TestFixture]
    public class GameSpecificationFixture : TestBase<GameSpecification>
    {
        private Game _game1;
        private Game _game2;
        private RemoteGame _remoteGame = new();
        private SearchCriteriaBase _searchCriteria = new GameSearchCriteria();
        private ReleaseDecisionInformation _information;

        [SetUp]
        public void Setup()
        {
            _game1 = Builder<Game>.CreateNew().With(s => s.Id = 1).Build();
            _game2 = Builder<Game>.CreateNew().With(s => s.Id = 2).Build();

            _remoteGame.Game = _game1;
            _information = new ReleaseDecisionInformation(false, _searchCriteria);
        }

        [Test]
        public void should_return_false_if_series_doesnt_match()
        {
            _searchCriteria.Game = _game2;

            Subject.IsSatisfiedBy(_remoteGame, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_series_ids_match()
        {
            _searchCriteria.Game = _game1;

            Subject.IsSatisfiedBy(_remoteGame, _information).Accepted.Should().BeTrue();
        }
    }
}
