using System;
using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class game_qualities_and_release_filesFixture : MigrationTest<game_qualities_and_release_files>
    {
        private const string MovieQuality = "{\"quality\":7,\"revision\":{\"version\":1,\"real\":0,\"isRepack\":false}}";

        [Test]
        public void should_replace_quality_definitions_and_file_qualities()
        {
            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("QualityDefinitions").Row(new
                {
                    Quality = 7,
                    Title = "Bluray-1080p",
                    MinSize = 0,
                    MaxSize = 100,
                    PreferredSize = 95
                });

                c.Insert.IntoTable("GameFiles").Row(new
                {
                    GameId = 1,
                    Quality = MovieQuality,
                    Size = 100,
                    DateAdded = DateTime.UtcNow,
                    Languages = "[1]",
                    RelativePath = "Super Metroid (1994).sfc",
                    IndexerFlags = 0,
                    MediaInfo = "{}"
                });
            });

            db.ExecuteScalar<int>("SELECT COUNT(*) FROM \"QualityDefinitions\"").Should().Be(0);

            var files = db.Query<GameFileRow>("SELECT \"Quality\", \"Regions\", \"ReleaseType\" FROM \"GameFiles\"").ToList();

            files.Should().HaveCount(1);
            files.First().Quality.Should().Contain("\"quality\":0");
            files.First().ReleaseType.Should().Be(0);
        }

        [Test]
        public void should_rewrite_existing_quality_profiles_and_add_game_defaults()
        {
            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("QualityProfiles").Row(new
                {
                    Name = "HD-1080p",
                    Cutoff = 7,
                    Items = "[{\"quality\":7,\"items\":[],\"allowed\":true}]",
                    Language = 1,
                    FormatItems = "[]",
                    MinFormatScore = 0,
                    CutoffFormatScore = 0,
                    MinUpgradeFormatScore = 1,
                    UpgradeAllowed = false
                });
            });

            var profiles = db.Query<ProfileRow>("SELECT \"Name\", \"Cutoff\", \"Items\" FROM \"QualityProfiles\"").ToList();

            profiles.Select(p => p.Name).Should().BeEquivalentTo(new[] { "HD-1080p", "Verified ROMs / Redump", "PC – GOG preferred", "PC – Scene/Repack" });

            var existing = profiles.Single(p => p.Name == "HD-1080p");
            existing.Cutoff.Should().Be(101);
            existing.Items.Should().Contain("\"quality\":134");
            existing.Items.Should().NotContain("\"quality\":7,");
        }

        [Test]
        public void should_remove_resolution_conditions_from_custom_formats()
        {
            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("CustomFormats").Row(new
                {
                    Name = "Mixed",
                    IncludeCustomFormatWhenRenaming = false,
                    Specifications = "[{\"type\":\"ResolutionSpecification\",\"body\":{\"value\":1080,\"name\":\"1080p\",\"negate\":false,\"required\":false}},{\"type\":\"ReleaseTitleSpecification\",\"body\":{\"value\":\"GOG\",\"name\":\"GOG\",\"negate\":false,\"required\":false}}]"
                });
            });

            var specifications = db.ExecuteScalar<string>("SELECT \"Specifications\" FROM \"CustomFormats\"");

            specifications.Should().NotContain("ResolutionSpecification");
            specifications.Should().Contain("ReleaseTitleSpecification");
        }

        [Test]
        public void should_replace_movie_naming_format()
        {
            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("NamingConfig").Row(new
                {
                    RenameGames = false,
                    ReplaceIllegalCharacters = true,
                    ColonReplacementFormat = 4,
                    StandardGameFormat = "{Game Title} ({Release Year}) {Quality Full}",
                    GameFolderFormat = "{Platform Folder}/{Game Title} ({Release Year})"
                });
            });

            db.ExecuteScalar<string>("SELECT \"StandardGameFormat\" FROM \"NamingConfig\"")
              .Should().Be("{Game Title} ({Release Year}){ (Region)}{ [Version]}");
        }

        private class GameFileRow
        {
            public string Quality { get; set; }
            public string Regions { get; set; }
            public int ReleaseType { get; set; }
        }

        private class ProfileRow
        {
            public string Name { get; set; }
            public int Cutoff { get; set; }
            public string Items { get; set; }
        }
    }
}
