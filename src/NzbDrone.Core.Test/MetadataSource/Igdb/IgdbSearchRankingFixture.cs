using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.AlternativeTitles;
using NzbDrone.Core.MetadataSource.Igdb;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MetadataSource.Igdb
{
    [TestFixture]
    public class IgdbSearchRankingFixture : CoreTest
    {
        private static Game GivenGame(int igdbId, string title, float popularity, int year = 1994, params string[] alternativeTitles)
        {
            return new Game
            {
                GameMetadata = new GameMetadata
                {
                    IgdbId = igdbId,
                    Title = title,
                    CleanTitle = title.CleanGameTitle(),
                    Year = year,
                    Popularity = popularity,
                    AlternativeTitles = alternativeTitles.Select(t => new AlternativeTitle { Title = t, CleanTitle = t.CleanGameTitle() }).ToList()
                }
            };
        }

        private static List<int> Ids(List<Game> games) => games.Select(g => g.GameMetadata.Value.IgdbId).ToList();

        [Test]
        public void should_put_the_exact_title_ahead_of_hacks_containing_it()
        {
            var results = new List<Game>
            {
                GivenGame(330214, "Super Metroid Arcade", 2, 2017),
                GivenGame(42524, "Super Metroid: Phazon", 10, 2011),
                GivenGame(9999, "Metroid Fusion Super Edition", 50, 2005),
                GivenGame(1103, "Super Metroid", 1085)
            };

            Ids(IgdbProxy.RankSearchResults(results, "Super Metroid")).Should().Equal(1103, 42524, 330214, 9999);
        }

        [Test]
        public void should_prefer_the_most_rated_of_several_exact_matches()
        {
            var results = new List<Game>
            {
                GivenGame(202256, "Sonic the Hedgehog 2", 0, 1992),
                GivenGame(1997, "Sonic the Hedgehog 2", 600, 1992),
                GivenGame(4438, "Sonic the Hedgehog 2", 40, 1992)
            };

            Ids(IgdbProxy.RankSearchResults(results, "sonic the hedgehog 2")).Should().Equal(1997, 4438, 202256);
        }

        [Test]
        public void should_match_alternative_titles_exactly()
        {
            var results = new List<Game>
            {
                GivenGame(1, "Final Fantasy III Advance", 30),
                GivenGame(2, "Final Fantasy VI", 900, 1994, "Final Fantasy III")
            };

            Ids(IgdbProxy.RankSearchResults(results, "Final Fantasy III")).Should().Equal(2, 1);
        }

        [Test]
        public void should_keep_igdb_order_for_titles_that_do_not_start_with_the_term()
        {
            var results = new List<Game>
            {
                GivenGame(1, "The Legend of Zelda: A Link to the Past", 10),
                GivenGame(2, "BS Zelda no Densetsu", 500),
                GivenGame(3, "Another Zelda", 900)
            };

            Ids(IgdbProxy.RankSearchResults(results, "Link to the Past")).Should().Equal(1, 2, 3);
        }

        [Test]
        public void should_still_prefer_the_requested_year()
        {
            var results = new List<Game>
            {
                GivenGame(1, "Resident Evil", 900, 1996),
                GivenGame(2, "Resident Evil", 400, 2002)
            };

            Ids(IgdbProxy.RankSearchResults(results, "Resident Evil", 2002)).Should().Equal(2, 1);
        }
    }
}
