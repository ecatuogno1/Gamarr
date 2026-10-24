using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers.FileList;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.FileListTests
{
    public class FileListRequestGeneratorFixture : CoreTest<FileListRequestGenerator>
    {
        private GameSearchCriteria _gameSearchCriteria;

        [SetUp]
        public void Setup()
        {
            Subject.Settings = new FileListSettings()
            {
                BaseUrl = "http://127.0.0.1:1234/",
                Categories = new[] { 9, 10 },
                Passkey = "abcd",
                Username = "somename"
            };

            _gameSearchCriteria = new GameSearchCriteria
            {
                Game = new Games.Game { Title = "Doom Eternal", Year = 2020 },
                SceneTitles = new List<string> { "Doom Eternal" }
            };
        }

        [Test]
        public void should_use_categories_for_feed()
        {
            var results = Subject.GetRecentRequests();

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("&category=9,10");
        }

        [Test]
        public void should_search_by_name_without_year()
        {
            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("type=name");
            page.Url.Query.Should().Contain("query=Doom+Eternal");
            page.Url.Query.Should().NotContain("2020");
        }

        [Test]
        public void should_use_pc_games_category_for_pc_platform()
        {
            _gameSearchCriteria.Platform = new Platform { Id = 1, ShortName = "PC", NewznabCategories = new List<int> { 4050 } };

            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("&category=9&");
        }

        [Test]
        public void should_use_console_games_category_for_console_platform()
        {
            _gameSearchCriteria.Platform = new Platform { Id = 2, ShortName = "PS3", NewznabCategories = new List<int> { 1000, 1080 } };

            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("&category=10&");
        }

        [Test]
        public void should_fall_back_to_title_and_platform()
        {
            _gameSearchCriteria.Platform = new Platform { Id = 2, ShortName = "PS3", NewznabCategories = new List<int> { 1000, 1080 } };

            var results = Subject.GetSearchRequests(_gameSearchCriteria);

            results.Tiers.Should().Be(2);
            results.GetTier(1).First().First().Url.Query.Should().Contain("query=Doom+Eternal+PS3");
        }

        [Test]
        public void should_default_to_game_categories()
        {
            new FileListSettings().Categories.Should().BeEquivalentTo(new[] { 9, 10 });
        }

        [TestCase("Jocuri PC", 4050)]
        [TestCase("Jocuri Console", 1000)]
        public void should_map_category_name_to_newznab_category(string name, int category)
        {
            FileListCategoryMapping.ToNewznabCategories(name).Should().BeEquivalentTo(new[] { category });
        }

        [Test]
        public void should_not_map_unknown_category_name()
        {
            FileListCategoryMapping.ToNewznabCategories("Filme HD").Should().BeEmpty();
        }
    }
}
