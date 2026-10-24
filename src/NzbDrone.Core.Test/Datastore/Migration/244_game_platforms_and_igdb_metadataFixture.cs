using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class game_platforms_and_igdb_metadataFixture : MigrationTest<game_platforms_and_igdb_metadata>
    {
        private static void GivenGame(FluentMigrator.Migration c, int minimumAvailability = 2)
        {
            c.Insert.IntoTable("GameMetadata").Row(new
            {
                IgdbId = 1103,
                ImdbId = "tt0000001",
                Title = "Super Metroid",
                CleanTitle = "supermetroid",
                Status = 2,
                Images = "[]",
                Recommendations = "[]",
                Runtime = 90,
                Certification = "E",
                Studio = "Nintendo",
                InCinemas = DateTime.UtcNow,
                OriginalTitle = "Super Metroid",
                CleanOriginalTitle = "supermetroid",
                OriginalLanguage = 1,
                Ratings = "{\"imdb\":{\"votes\":1,\"value\":5}}",
                LastInfoSync = DateTime.UtcNow
            });

            c.Insert.IntoTable("Games").Row(new
            {
                Monitored = true,
                MinimumAvailability = minimumAvailability,
                QualityProfileId = 1,
                GameFileId = 0,
                GameMetadataId = 1,
                Path = "/Games/Super Metroid (1994)",
                Added = DateTime.UtcNow
            });
        }

        [Test]
        public void should_assign_existing_games_to_pc_and_map_in_cinemas_availability()
        {
            var db = WithDapperMigrationTestDb(c => GivenGame(c));

            var games = db.Query<GameRow>("SELECT \"Id\", \"PlatformId\", \"MinimumAvailability\" FROM \"Games\"");
            var pcId = db.ExecuteScalar<int>("SELECT \"Id\" FROM \"Platforms\" WHERE \"IgdbPlatformId\" = 6");

            games.Should().HaveCount(1);
            games.First().PlatformId.Should().Be(pcId);
            games.First().MinimumAvailability.Should().Be(3);

            db.ExecuteScalar<int>("SELECT \"Status\" FROM \"GameMetadata\"").Should().Be(3);
        }

        [Test]
        public void should_allow_same_game_on_another_platform_but_not_twice_on_the_same()
        {
            var db = WithDapperMigrationTestDb(c => GivenGame(c));

            var snesId = db.ExecuteScalar<int>("SELECT \"Id\" FROM \"Platforms\" WHERE \"IgdbPlatformId\" = 19");
            var pcId = db.ExecuteScalar<int>("SELECT \"Id\" FROM \"Platforms\" WHERE \"IgdbPlatformId\" = 6");

            db.Execute("INSERT INTO \"Games\" (\"Monitored\", \"MinimumAvailability\", \"QualityProfileId\", \"GameFileId\", \"GameMetadataId\", \"Path\", \"PlatformId\") VALUES (1, 3, 1, 0, 1, '/Games/snes/Super Metroid (1994)', @snesId)", new { snesId });

            Action duplicate = () => db.Execute("INSERT INTO \"Games\" (\"Monitored\", \"MinimumAvailability\", \"QualityProfileId\", \"GameFileId\", \"GameMetadataId\", \"Path\", \"PlatformId\") VALUES (1, 3, 1, 0, 1, '/Games/pc/Super Metroid (1994)', @pcId)", new { pcId });

            duplicate.Should().Throw<Exception>();

            db.Query<GameRow>("SELECT \"Id\", \"PlatformId\", \"MinimumAvailability\" FROM \"Games\"").Should().HaveCount(2);
        }

        [Test]
        public void should_drop_movie_columns_and_add_igdb_columns()
        {
            var db = WithDapperMigrationTestDb(c => GivenGame(c));

            var metadata = (IDictionary<string, object>)db.Query("SELECT * FROM \"GameMetadata\"").Single();

            metadata.Should().NotContainKeys("ImdbId", "Runtime", "Certification", "Studio", "InCinemas", "PhysicalRelease", "DigitalRelease");
            metadata.Should().ContainKeys("IgdbSlug", "Storyline", "ReleaseDate", "PlatformReleaseDates", "AvailablePlatformIgdbIds", "Themes", "GameModes", "Developers", "Publishers", "AgeRatings", "Websites");
            metadata["Developers"].Should().Be("[]");
            metadata["Ratings"].Should().Be("{}");
        }

        [Test]
        public void should_make_exclusions_unique_per_platform()
        {
            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ImportExclusions").Row(new { IgdbId = 1103, GameTitle = "Super Metroid", GameYear = 1994 });
            });

            db.Execute("INSERT INTO \"ImportExclusions\" (\"IgdbId\", \"GameTitle\", \"GameYear\", \"PlatformId\") VALUES (1103, 'Super Metroid', 1994, 19)");

            Action duplicate = () => db.Execute("INSERT INTO \"ImportExclusions\" (\"IgdbId\", \"GameTitle\", \"GameYear\", \"PlatformId\") VALUES (1103, 'Super Metroid', 1994, 19)");
            duplicate.Should().Throw<Exception>();

            db.Query<int>("SELECT \"PlatformId\" FROM \"ImportExclusions\" ORDER BY \"Id\"").Should().Equal(0, 19);
        }

        [Test]
        public void should_update_default_game_folder_format()
        {
            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("NamingConfig").Row(new
                {
                    RenameGames = false,
                    ReplaceIllegalCharacters = true,
                    ColonReplacementFormat = 4,
                    StandardGameFormat = "{Game Title} ({Release Year}) {Quality Full}",
                    GameFolderFormat = "{Game Title} ({Release Year})"
                });
            });

            db.ExecuteScalar<string>("SELECT \"GameFolderFormat\" FROM \"NamingConfig\"").Should().Be("{Platform Folder}/{Game Title} ({Release Year})");
        }

        [Test]
        public void should_clear_movie_credits_and_rename_person_column()
        {
            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Credits").Row(new
                {
                    CreditIgdbId = "abc",
                    PersonIgdbId = 1,
                    Name = "Actor",
                    Images = "[]",
                    Character = "Hero",
                    Order = 0,
                    Type = 0,
                    GameMetadataId = 1
                });
            });

            db.ExecuteScalar<int>("SELECT COUNT(*) FROM \"Credits\"").Should().Be(0);

            db.Execute("INSERT INTO \"Credits\" (\"CreditIgdbId\", \"CompanyIgdbId\", \"Name\", \"Images\", \"Order\", \"Type\", \"GameMetadataId\") VALUES ('3001-publisher', 70, 'Nintendo', '[]', 0, 1, 1)");

            var credit = (IDictionary<string, object>)db.Query("SELECT * FROM \"Credits\"").Single();
            credit.Should().ContainKey("CompanyIgdbId");
            credit.Should().NotContainKeys("PersonIgdbId", "Character", "Department", "Job");
        }

        private class GameRow
        {
            public int Id { get; set; }
            public int PlatformId { get; set; }
            public int MinimumAvailability { get; set; }
        }
    }
}
