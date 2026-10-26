using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.Credits;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.Igdb;
using NzbDrone.Core.MetadataSource.Igdb.Resource;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MetadataSource.Igdb
{
    [TestFixture]
    public class IgdbProxyFixture : CoreTest<IgdbProxy>
    {
        private List<string> _queries;

        [SetUp]
        public void Setup()
        {
            _queries = new List<string>();
            GivenGames("game_legacy_fields.json");
        }

        private void GivenGames(string file)
        {
            var json = ReadAllText($"Files/Igdb/{file}");

            Mocker.GetMock<IIgdbClient>()
                  .Setup(s => s.Query<IgdbGameResource>("games", It.IsAny<string>()))
                  .Returns<string, string>((endpoint, query) =>
                  {
                      _queries.Add(query);
                      return JsonConvert.DeserializeObject<List<IgdbGameResource>>(json);
                  });
        }

        private void GivenNoGames()
        {
            Mocker.GetMock<IIgdbClient>()
                  .Setup(s => s.Query<IgdbGameResource>("games", It.IsAny<string>()))
                  .Returns<string, string>((endpoint, query) =>
                  {
                      _queries.Add(query);
                      return new List<IgdbGameResource>();
                  });
        }

        [Test]
        public void should_map_game_with_legacy_fields()
        {
            var (game, credits) = Subject.GetGameInfo(1103);

            game.IgdbId.Should().Be(1103);
            game.IgdbSlug.Should().Be("super-metroid");
            game.IgdbUrl.Should().Be("https://www.igdb.com/games/super-metroid");
            game.Title.Should().Be("Super Metroid");
            game.CleanTitle.Should().Be("supermetroid");
            game.Overview.Should().Be("The third game in the Metroid series.");
            game.Storyline.Should().StartWith("Samus Aran");

            game.ReleaseDate.Should().Be(new DateTime(1994, 4, 1, 0, 0, 0, DateTimeKind.Utc));
            game.Year.Should().Be(1994);
            game.Status.Should().Be(GameStatusType.Released);

            game.Genres.Should().BeEquivalentTo("Platform", "Adventure");
            game.Themes.Should().BeEquivalentTo("Science fiction");
            game.GameModes.Should().BeEquivalentTo("Single player");
            game.Keywords.Should().BeEquivalentTo("metroidvania");
            game.Developers.Should().BeEquivalentTo("Nintendo R&D1", "Intelligent Systems");
            game.Publishers.Should().BeEquivalentTo("Nintendo");

            game.CollectionIgdbId.Should().Be(104);
            game.CollectionTitle.Should().Be("Metroid");

            game.AvailablePlatformIgdbIds.Should().BeEquivalentTo(new[] { 19, 58, 5, 41 });
            game.PlatformReleaseDates.Should().HaveCount(4);
            game.PlatformReleaseDates.Single(d => d.IgdbPlatformId == 19).Date.Should().Be(new DateTime(1994, 4, 22, 0, 0, 0, DateTimeKind.Utc));
            game.PlatformReleaseDates.Single(d => d.IgdbPlatformId == 41).Date.Should().BeNull();

            game.AlternativeTitles.Should().ContainSingle(t => t.Title == "Metroid 3");
            game.Recommendations.Should().BeEquivalentTo(new[] { 1104, 1105 });
            game.YouTubeTrailerId.Should().Be("trailer456");
            game.Popularity.Should().Be(818);

            game.Ratings.Igdb.Value.Should().Be(92.1m);
            game.Ratings.Igdb.Votes.Should().Be(812);
            game.Ratings.Igdb.Type.Should().Be(RatingType.User);
            game.Ratings.Critic.Value.Should().Be(94.5m);
            game.Ratings.Critic.Votes.Should().Be(6);
            game.Ratings.Critic.Type.Should().Be(RatingType.Critic);

            game.AgeRatings.Should().BeEquivalentTo(new[]
            {
                new AgeRating { Organization = "ESRB", Rating = "E" },
                new AgeRating { Organization = "PEGI", Rating = "3" }
            });

            game.Websites.Should().BeEquivalentTo(new[]
            {
                new GameWebsite { Type = "Official", Url = "https://www.nintendo.com/super-metroid" },
                new GameWebsite { Type = "Wikipedia", Url = "https://en.wikipedia.org/wiki/Super_Metroid" }
            });
            game.Website.Should().Be("https://www.nintendo.com/super-metroid");

            credits.Should().HaveCount(4);
            credits.Should().ContainSingle(c => c.Name == "Nintendo" && c.Type == CreditType.Publisher && c.CompanyIgdbId == 70);
            credits.Should().ContainSingle(c => c.Name == "Intelligent Systems" && c.Type == CreditType.Developer);
            credits.Should().ContainSingle(c => c.Name == "Intelligent Systems" && c.Type == CreditType.Supporting);
            credits.Select(c => c.CreditIgdbId).Should().OnlyHaveUniqueItems();
            credits.Single(c => c.Name == "Nintendo").Images.Single().RemoteUrl.Should().Be("https://images.igdb.com/igdb/image/upload/t_logo_med/cl1abc.jpg");
        }

        [Test]
        public void should_map_images()
        {
            var game = Subject.GetGameInfo(1103).Item1;

            game.Images.Should().ContainSingle(i => i.CoverType == MediaCoverTypes.Poster)
                .Which.RemoteUrl.Should().Be("https://images.igdb.com/igdb/image/upload/t_original/co1x5c.jpg");

            game.Images.Should().ContainSingle(i => i.CoverType == MediaCoverTypes.Fanart)
                .Which.RemoteUrl.Should().Be("https://images.igdb.com/igdb/image/upload/t_1080p/ar1abc.jpg");

            game.Images.Where(i => i.CoverType == MediaCoverTypes.Screenshot).Select(i => i.RemoteUrl)
                .Should().BeEquivalentTo(
                    "https://images.igdb.com/igdb/image/upload/t_1080p/sc1aaa.jpg",
                    "https://images.igdb.com/igdb/image/upload/t_1080p/sc1bbb.jpg");
        }

        [Test]
        public void should_use_screenshot_as_fanart_without_artwork()
        {
            GivenGames("game_modern_fields.json");

            var game = Subject.GetGameInfo(119133).Item1;

            game.Images.Should().ContainSingle(i => i.CoverType == MediaCoverTypes.Fanart)
                .Which.RemoteUrl.Should().Be("https://images.igdb.com/igdb/image/upload/t_1080p/sc2aaa.jpg");
        }

        [Test]
        public void should_map_game_with_current_field_names()
        {
            GivenGames("game_modern_fields.json");

            var game = Subject.GetGameInfo(119133).Item1;

            game.Title.Should().Be("The Legend of Zelda: Tears of the Kingdom");
            game.Status.Should().Be(GameStatusType.Released);
            game.CollectionIgdbId.Should().Be(106);
            game.CollectionTitle.Should().Be("The Legend of Zelda");
            game.AvailablePlatformIgdbIds.Should().BeEquivalentTo(new[] { 130, 508 });

            game.AgeRatings.Should().BeEquivalentTo(new[]
            {
                new AgeRating { Organization = "ESRB", Rating = "E10+" },
                new AgeRating { Organization = "PEGI", Rating = "12" }
            });

            game.Websites.Should().BeEquivalentTo(new[]
            {
                new GameWebsite { Type = "Official", Url = "https://zelda.nintendo.com/tears-of-the-kingdom/" },
                new GameWebsite { Type = "Steam", Url = "https://store.steampowered.com/app/0" }
            });

            game.YouTubeTrailerId.Should().BeNull();
            game.Developers.Should().BeEquivalentTo("Nintendo EPD");
        }

        [Test]
        public void should_map_status_of_unreleased_games()
        {
            GivenGames("game_upcoming.json");

            var games = Subject.GetBulkGameInfo(new List<int> { 250000, 250001, 250002 });

            games.Single(g => g.IgdbId == 250000).Status.Should().Be(GameStatusType.Announced);
            games.Single(g => g.IgdbId == 250000).Year.Should().Be(2100);
            games.Single(g => g.IgdbId == 250001).Status.Should().Be(GameStatusType.Announced);
            games.Single(g => g.IgdbId == 250002).Status.Should().Be(GameStatusType.TBA);
        }

        [Test]
        public void should_throw_not_found_for_unknown_game()
        {
            GivenNoGames();

            Assert.Throws<GameNotFoundException>(() => Subject.GetGameInfo(1));
        }

        [Test]
        public void should_request_expanded_fields_including_deprecated_and_current_names()
        {
            Subject.GetGameInfo(1103);

            var query = _queries.Single();
            query.Should().StartWith("fields ");
            query.Should().Contain("where id = 1103;");

            foreach (var field in new[] { "cover.image_id", "involved_companies.company.name", "status", "game_status.status", "category", "game_type.type", "collection.name", "collections.name", "age_ratings.category", "age_ratings.organization.name", "websites.category", "websites.type.type" })
            {
                query.Should().Contain(field);
            }
        }

        [Test]
        public void should_fall_back_to_current_field_names_when_igdb_rejects_deprecated_fields()
        {
            var json = ReadAllText("Files/Igdb/game_modern_fields.json");

            Mocker.GetMock<IIgdbClient>()
                  .Setup(s => s.Query<IgdbGameResource>("games", It.IsAny<string>()))
                  .Returns<string, string>((endpoint, query) =>
                  {
                      _queries.Add(query);

                      if (query.StartsWith(IgdbProxy.GameFields))
                      {
                          throw new IgdbException("IGDB request to 'games' failed with HTTP 400: [{\"title\":\"Invalid Field\",\"status\":400}]");
                      }

                      return JsonConvert.DeserializeObject<List<IgdbGameResource>>(json);
                  });

            Subject.GetGameInfo(119133).Item1.Title.Should().Be("The Legend of Zelda: Tears of the Kingdom");
            Subject.GetGameInfo(119133);

            _queries.Should().HaveCount(3);
            _queries[1].Should().StartWith(IgdbProxy.ModernGameFields);
            _queries[2].Should().StartWith(IgdbProxy.ModernGameFields);
        }

        [TestCase("igdb:1103")]
        [TestCase("igdbid:1103")]
        [TestCase("IGDB: 1103")]
        public void should_look_up_by_igdb_id(string term)
        {
            var results = Subject.SearchForNewGame(term);

            results.Should().HaveCount(1);
            results.First().IgdbId.Should().Be(1103);
            _queries.Single().Should().Contain("where id = 1103;");
            _queries.Single().Should().NotContain("search");
        }

        [Test]
        public void should_look_up_by_igdb_url()
        {
            Subject.SearchForNewGame("https://www.igdb.com/games/super-metroid");

            _queries.Single().Should().Contain("where slug = \"super-metroid\";");
        }

        [Test]
        public void should_search_by_name_excluding_versions()
        {
            var results = Subject.SearchForNewGame("Super Metroid");

            results.Should().HaveCount(1);
            results.First().Id.Should().Be(0);

            var query = _queries.Single();
            query.Should().Contain("search \"Super Metroid\";");
            query.Should().Contain("where version_parent = null;");
            query.Should().Contain("limit 30;");
        }

        [Test]
        public void should_filter_search_by_platform()
        {
            Subject.SearchForNewGame("Super Metroid", 19);

            _queries.Single().Should().Contain("where version_parent = null & platforms = (19);");
        }

        [Test]
        public void should_strip_year_in_parentheses_from_search()
        {
            Subject.SearchForNewGame("Super Metroid (1994)");

            _queries.Single().Should().Contain("search \"Super Metroid\";");
        }

        [Test]
        public void should_escape_quotes_in_search_term()
        {
            Subject.SearchForNewGame("Say \"Hi\"");

            _queries.Single().Should().Contain("search \"Say \\\"Hi\\\"\";");
        }

        [Test]
        public void should_get_collection_with_games()
        {
            Mocker.GetMock<IIgdbClient>()
                  .Setup(s => s.Query<IgdbCollectionResource>("collections", It.IsAny<string>()))
                  .Returns(JsonConvert.DeserializeObject<List<IgdbCollectionResource>>(ReadAllText("Files/Igdb/collection.json")));

            var collection = Subject.GetCollectionInfo(104);

            collection.IgdbId.Should().Be(104);
            collection.Title.Should().Be("Metroid");
            collection.Games.Should().HaveCount(1);
            collection.Images.Should().ContainSingle(i => i.CoverType == MediaCoverTypes.Poster);

            _queries.Single().Should().Contain("where id = (1103,1102);");
        }

        [Test]
        public void should_query_changed_games_for_library_ids()
        {
            Mocker.GetMock<IIgdbClient>()
                  .Setup(s => s.Query<IgdbReference>("games", It.IsAny<string>()))
                  .Returns<string, string>((endpoint, query) =>
                  {
                      _queries.Add(query);
                      return new List<IgdbReference> { new IgdbReference { Id = 1103 } };
                  });

            var changed = Subject.GetChangedGames(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), new[] { 1103, 1104, 1103 });

            changed.Should().BeEquivalentTo(new[] { 1103 });
            _queries.Single().Should().Be("fields id; where id = (1103,1104) & updated_at > 1704066300; limit 500;");
        }

        [Test]
        public void should_search_when_videos_have_no_name()
        {
            GivenGames("game_sparse.json");

            var results = Subject.SearchForNewGame("Action Boy");

            results.Should().HaveCount(3);
            results.First().Title.Should().Be("Action Boy");
            results.First().GameMetadata.Value.YouTubeTrailerId.Should().Be("trailer333");
        }

        [Test]
        public void should_use_first_video_when_no_video_is_named_trailer()
        {
            var resource = new IgdbGameResource
            {
                Id = 1,
                Name = "Action Boy",
                Videos = new List<IgdbVideo> { new() { Id = 1, VideoId = "first" }, new() { Id = 2, Name = "Gameplay", VideoId = "second" } }
            };

            Subject.MapGame(resource).YouTubeTrailerId.Should().Be("first");
        }

        [Test]
        public void should_map_game_with_missing_and_null_fields()
        {
            GivenGames("game_sparse.json");

            var game = Subject.GetBulkGameInfo(new List<int> { 300001 }).Single(g => g.IgdbId == 300001);

            game.Title.Should().Be("Action Boy");
            game.ReleaseDate.Should().BeNull();
            game.Year.Should().Be(0);
            game.Genres.Should().BeEquivalentTo("Adventure");
            game.Themes.Should().BeEmpty();
            game.Keywords.Should().BeEmpty();
            game.Developers.Should().BeEmpty();
            game.Publishers.Should().BeEmpty();
            game.CollectionIgdbId.Should().Be(0);
            game.CollectionTitle.Should().BeNull();
            game.AlternativeTitles.Should().BeEmpty();
            game.AgeRatings.Should().BeEmpty();
            game.Websites.Should().ContainSingle().Which.Type.Should().Be("Other");
            game.Website.Should().BeNull();
            game.Recommendations.Should().BeEquivalentTo(new[] { 300002 });
            game.AvailablePlatformIgdbIds.Should().BeEquivalentTo(new[] { 24 });
            game.PlatformReleaseDates.Should().ContainSingle().Which.Date.Should().BeNull();

            // The release date without a platform still counts
            game.Status.Should().Be(GameStatusType.Released);
            game.Ratings.Igdb.Should().BeNull();
            game.Ratings.Critic.Should().BeNull();

            game.Images.Should().NotContain(i => i.CoverType == MediaCoverTypes.Poster);
            game.Images.Should().ContainSingle(i => i.CoverType == MediaCoverTypes.Fanart)
                .Which.RemoteUrl.Should().Be("https://images.igdb.com/igdb/image/upload/t_1080p/scab.jpg");
        }

        [Test]
        public void should_name_games_without_a_name_after_slug_or_id()
        {
            GivenGames("game_sparse.json");

            var games = Subject.GetBulkGameInfo(new List<int> { 300002, 300003 });

            games.Should().HaveCount(3);
            games.Single(g => g.IgdbId == 300002).Title.Should().Be("nameless-game");
            games.Single(g => g.IgdbId == 300003).Title.Should().Be("IGDB 300003");
            games.Single(g => g.IgdbId == 300003).SortTitle.Should().NotBeNull();
        }

        [Test]
        public void should_map_game_with_only_an_id()
        {
            var minimal = Subject.MapGame(new IgdbGameResource { Id = 42 });

            minimal.Title.Should().Be("IGDB 42");
            minimal.Images.Should().BeEmpty();
            minimal.YouTubeTrailerId.Should().BeNull();
            Subject.MapCredits(new IgdbGameResource { Id = 42 }).Should().BeEmpty();
        }

        [Test]
        public void should_tolerate_null_and_out_of_range_values_in_mappings()
        {
            IgdbMappings.MapAgeRating(null).Should().BeNull();
            IgdbMappings.MapWebsite(null).Should().BeNull();
            IgdbMappings.FromUnixTime(long.MaxValue).Should().BeNull();
            IgdbMappings.FromUnixTime(long.MinValue).Should().BeNull();
            IgdbMappings.GetStatusKey(new IgdbGameResource { GameStatus = new IgdbReference() }).Should().BeNull();
            IgdbMappings.MapStatus(new IgdbGameResource { ReleaseDates = new List<IgdbReleaseDate> { null } }, DateTime.UtcNow)
                .Should().Be(GameStatusType.Announced);
        }

        [Test]
        public void should_skip_credits_with_missing_company()
        {
            GivenGames("game_sparse.json");

            Subject.GetGameInfo(300001).Item2.Should().BeEmpty();
        }

        [Test]
        public void should_get_collection_without_name_or_games()
        {
            Mocker.GetMock<IIgdbClient>()
                  .Setup(s => s.Query<IgdbCollectionResource>("collections", It.IsAny<string>()))
                  .Returns(new List<IgdbCollectionResource> { new() { Id = 7, Slug = "series", Games = new List<IgdbReference> { null } } });

            var collection = Subject.GetCollectionInfo(7);

            collection.Title.Should().Be("series");
            collection.Games.Should().BeEmpty();
            collection.Images.Should().BeEmpty();
        }

        [Test]
        public void should_return_existing_metadata_when_mapping_known_igdb_id()
        {
            var existing = new GameMetadata { IgdbId = 1103, Title = "Existing" };

            Mocker.GetMock<IGameMetadataService>()
                  .Setup(s => s.FindByIgdbId(1103))
                  .Returns(existing);

            Subject.MapGameToIgdbGame(new GameMetadata { IgdbId = 1103 }).Should().BeSameAs(existing);
            _queries.Should().BeEmpty();
        }

        [Test]
        public void should_map_list_game_by_title_and_year()
        {
            var result = Subject.MapGameToIgdbGame(new GameMetadata { Title = "Super Metroid", Year = 1994 });

            result.IgdbId.Should().Be(1103);
            _queries.Single().Should().Contain("search \"Super Metroid\";");
        }
    }
}
