using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.NewznabTests
{
    public class NewznabRequestGeneratorFixture : CoreTest<NewznabRequestGenerator>
    {
        private GameSearchCriteria _gameSearchCriteria;
        private NewznabCapabilities _capabilities;
        private Platform _snes;
        private Platform _ps3;
        private Platform _pc;

        [SetUp]
        public void SetUp()
        {
            Subject.Settings = new NewznabSettings()
            {
                BaseUrl = "http://127.0.0.1:1234/",
                Categories = new[] { 1, 2 },
                ApiKey = "abcd",
            };

            _snes = new Platform { Id = 10, Name = "Super Nintendo Entertainment System", ShortName = "SNES", NewznabCategories = new List<int> { 1000 } };
            _ps3 = new Platform { Id = 11, Name = "PlayStation 3", ShortName = "PS3", NewznabCategories = new List<int> { 1000, 1080 } };
            _pc = new Platform { Id = 1, Name = "PC (Microsoft Windows)", ShortName = "PC", NewznabCategories = new List<int> { 4050 } };

            _gameSearchCriteria = new GameSearchCriteria
            {
                Game = new Games.Game { Title = "Super Metroid", Year = 1994, IgdbId = 1103 },
                SceneTitles = new List<string> { "Super Metroid" }
            };

            _capabilities = new NewznabCapabilities();

            Mocker.GetMock<INewznabCapabilitiesProvider>()
                .Setup(v => v.GetCapabilities(It.IsAny<NewznabSettings>()))
                .Returns(_capabilities);
        }

        private void GivenGameCategories()
        {
            Subject.Settings.Categories = NewznabStandardCategory.DefaultGameCategories;
        }

        [Test]
        public void should_default_to_game_categories()
        {
            var settings = new NewznabSettings();

            settings.Categories.Should().Contain(new[] { 1000, 1080, 1180, 4050 });
            settings.Categories.Should().NotContain(c => c >= 2000 && c < 3000);
        }

        [Test]
        public void should_use_all_categories_for_feed()
        {
            var results = Subject.GetRecentRequests();

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("t=search");
            page.Url.Query.Should().Contain("&cat=1,2&");
        }

        [Test]
        public void should_not_have_duplicate_categories()
        {
            Subject.Settings.Categories = new[] { 1, 2, 2, 3 };

            var results = Subject.GetRecentRequests();

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.FullUri.Should().Contain("&cat=1,2,3&");
        }

        [Test]
        public void should_return_subsequent_pages()
        {
            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            results.GetAllTiers().Should().HaveCount(1);

            var pages = results.GetAllTiers().First().Take(3).ToList();

            pages[0].Url.FullUri.Should().Contain("&offset=0&");
            pages[1].Url.FullUri.Should().Contain("&offset=100&");
            pages[2].Url.FullUri.Should().Contain("&offset=200&");
        }

        [Test]
        public void should_not_get_unlimited_pages()
        {
            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            results.GetAllTiers().Should().HaveCount(1);

            var pages = results.GetAllTiers().First().Take(500).ToList();

            pages.Count.Should().BeLessThan(500);
        }

        [Test]
        public void should_search_by_title_without_year_or_ids()
        {
            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("t=search");
            page.Url.Query.Should().EndWith("q=Super%20Metroid");
            page.Url.Query.Should().NotContain("1994");
            page.Url.Query.Should().NotContain("imdbid");
            page.Url.Query.Should().NotContain("igdbid");
            page.Url.Query.Should().NotContain("tmdbid");
        }

        [Test]
        public void should_not_search_if_basic_search_not_supported()
        {
            _capabilities.SupportedSearchParameters = null;

            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            results.GetAllTiers().Should().BeEmpty();
        }

        [Test]
        public void should_search_each_title()
        {
            _gameSearchCriteria.SceneTitles = new List<string> { "Super Metroid", "Metroid 3" };

            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            results.GetTier(0).Should().HaveCount(2);
            results.GetTier(0).Last().First().Url.Query.Should().EndWith("q=Metroid%203");
        }

        [Test]
        public void should_restrict_categories_to_platform()
        {
            GivenGameCategories();
            _gameSearchCriteria.Platform = _ps3;

            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("&cat=1000,1080&");
        }

        [Test]
        public void should_use_pc_games_category_for_pc()
        {
            GivenGameCategories();
            _gameSearchCriteria.Platform = _pc;

            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("&cat=4050&");
        }

        [Test]
        public void should_use_configured_parent_category_covering_platform_category()
        {
            Subject.Settings.Categories = new[] { 1000, 4000 };
            _gameSearchCriteria.Platform = _pc;

            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("&cat=4000&");
        }

        [Test]
        public void should_use_configured_categories_if_platform_categories_not_configured()
        {
            Subject.Settings.Categories = new[] { 1080, 1180 };
            _gameSearchCriteria.Platform = _snes;

            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("&cat=1080,1180&");
        }

        [Test]
        public void should_use_configured_categories_without_platform()
        {
            GivenGameCategories();

            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain($"&cat={string.Join(",", NewznabStandardCategory.DefaultGameCategories)}&");
        }

        [Test]
        public void should_fall_back_to_title_and_platform()
        {
            GivenGameCategories();
            _gameSearchCriteria.Platform = _snes;

            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            results.Tiers.Should().Be(2);

            var tier1 = results.GetTier(0).First().First();
            var tier2 = results.GetTier(1).First().First();

            tier1.Url.Query.Should().EndWith("q=Super%20Metroid");
            tier1.Url.Query.Should().Contain("&cat=1000&");
            tier2.Url.Query.Should().EndWith("q=Super%20Metroid%20SNES");
            tier2.Url.Query.Should().Contain("&cat=1000&");
        }

        [Test]
        public void should_not_add_platform_tier_without_platform()
        {
            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            results.Tiers.Should().Be(1);
        }

        [Test]
        public void should_encode_raw_title()
        {
            _capabilities.TextSearchEngine = "raw";

            var gameRawSearchCriteria = new GameSearchCriteria
            {
                Game = new Games.Game { Title = "Some Game & Title: Words", Year = 2021, IgdbId = 123 },
                SceneTitles = new List<string> { "Some Game & Title: Words" }
            };

            var results = Subject.GetSearchRequests(gameRawSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("q=Some%20Game%20%26%20Title%3A%20Words");
            page.Url.Query.Should().NotContain(" & ");
            page.Url.Query.Should().Contain("%26");
        }

        [Test]
        public void should_use_clean_title_and_encode()
        {
            _capabilities.TextSearchEngine = "sphinx";

            var gameRawSearchCriteria = new GameSearchCriteria
            {
                Game = new Games.Game { Title = "Some Game & Title: Words", Year = 2021, IgdbId = 123 },
                SceneTitles = new List<string> { "Some Game & Title: Words" }
            };

            var results = Subject.GetSearchRequests(gameRawSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().EndWith("q=Some%20Game%20and%20Title%20Words");
            page.Url.Query.Should().NotContain(" & ");
            page.Url.Query.Should().NotContain("%26");
        }

        [Test]
        public void should_clean_platform_short_name()
        {
            _gameSearchCriteria.Platform = new Platform { Id = 12, ShortName = "Wii U", NewznabCategories = new List<int> { 1000, 1130 } };

            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            var page = results.GetTier(1).First().First();

            page.Url.Query.Should().EndWith("q=Super%20Metroid%20Wii%20U");
        }
    }
}
