using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Games;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.GameTests.GameServiceTests
{
    [TestFixture]
    public class AllGamePlatformsByIgdbIdFixture : CoreTest<GameService>
    {
        private void GivenGames(params (int IgdbId, int PlatformId)[] games)
        {
            var pairs = new List<KeyValuePair<int, int>>();

            foreach (var (igdbId, platformId) in games)
            {
                pairs.Add(new KeyValuePair<int, int>(igdbId, platformId));
            }

            Mocker.GetMock<IGameRepository>()
                  .Setup(s => s.AllGameIgdbIdsWithPlatform())
                  .Returns(pairs);
        }

        [Test]
        public void should_group_platforms_by_title()
        {
            GivenGames((1029, 42), (1029, 29), (1029, 31), (1070, 26));

            var result = Subject.AllGamePlatformsByIgdbId();

            result.Should().HaveCount(2);
            result[1029].Should().Equal(29, 31, 42);
            result[1070].Should().Equal(26);
        }

        [Test]
        public void should_return_empty_dictionary_for_empty_library()
        {
            GivenGames();

            Subject.AllGamePlatformsByIgdbId().Should().BeEmpty();
        }

        [Test]
        public void should_not_list_a_platform_twice()
        {
            GivenGames((1029, 29), (1029, 29));

            Subject.AllGamePlatformsByIgdbId()[1029].Should().Equal(29);
        }
    }
}
