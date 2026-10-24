using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(244)]
    public class game_platforms_and_igdb_metadata : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Games: every game belongs to a platform, existing games default to PC (Microsoft Windows)
            Alter.Table("Games").AddColumn("PlatformId").AsInt32().WithDefaultValue(0);
            Execute.Sql("UPDATE \"Games\" SET \"PlatformId\" = (SELECT \"Id\" FROM \"Platforms\" WHERE \"IgdbPlatformId\" = 6)");

            // A game is unique per (GameMetadataId, PlatformId) instead of per GameMetadataId
            IfDatabase(ProcessorIdConstants.PostgreSQL).Execute.Sql("ALTER TABLE \"Games\" DROP CONSTRAINT IF EXISTS \"IX_Games_GameMetadataId\"");
            IfDatabase(ProcessorIdConstants.PostgreSQL).Execute.Sql("ALTER TABLE \"Games\" DROP CONSTRAINT IF EXISTS \"Games_GameMetadataId_key\"");
            IfDatabase(ProcessorIdConstants.PostgreSQL).Execute.Sql("DROP INDEX IF EXISTS \"IX_Games_GameMetadataId\"");
            IfDatabase(ProcessorIdConstants.SQLite).Delete.Index("IX_Games_GameMetadataId").OnTable("Games");
            IfDatabase(ProcessorIdConstants.SQLite).Alter.Table("Games").AlterColumn("GameMetadataId").AsInt32().NotNullable();

            Create.Index("IX_Games_GameMetadataId_PlatformId").OnTable("Games")
                .OnColumn("GameMetadataId").Ascending()
                .OnColumn("PlatformId").Ascending()
                .WithOptions().Unique();

            Create.Index("IX_Games_PlatformId").OnTable("Games").OnColumn("PlatformId");

            // Availability is only Announced or Released, InCinemas (2) becomes Released (3)
            Execute.Sql("UPDATE \"Games\" SET \"MinimumAvailability\" = 3 WHERE \"MinimumAvailability\" = 2");
            Execute.Sql("UPDATE \"Collections\" SET \"MinimumAvailability\" = 3 WHERE \"MinimumAvailability\" = 2");
            Execute.Sql("UPDATE \"ImportLists\" SET \"MinimumAvailability\" = 3 WHERE \"MinimumAvailability\" = 2");

            // GameMetadata: IGDB fields in, movie fields out
            Alter.Table("GameMetadata")
                .AddColumn("IgdbSlug").AsString().Nullable()
                .AddColumn("Storyline").AsString().Nullable()
                .AddColumn("ReleaseDate").AsDateTimeOffset().Nullable()
                .AddColumn("PlatformReleaseDates").AsString().WithDefaultValue("[]")
                .AddColumn("AvailablePlatformIgdbIds").AsString().WithDefaultValue("[]")
                .AddColumn("Themes").AsString().WithDefaultValue("[]")
                .AddColumn("GameModes").AsString().WithDefaultValue("[]")
                .AddColumn("Developers").AsString().WithDefaultValue("[]")
                .AddColumn("Publishers").AsString().WithDefaultValue("[]")
                .AddColumn("AgeRatings").AsString().WithDefaultValue("[]")
                .AddColumn("Websites").AsString().WithDefaultValue("[]");

            Execute.Sql("UPDATE \"GameMetadata\" SET \"Status\" = 3 WHERE \"Status\" = 2");

            // Ratings now only have Igdb (user) and Critic (aggregated) values
            Execute.Sql("UPDATE \"GameMetadata\" SET \"Ratings\" = '{}'");

            Delete.Column("ImdbId")
                .Column("Runtime")
                .Column("Certification")
                .Column("Studio")
                .Column("InCinemas")
                .Column("PhysicalRelease")
                .Column("DigitalRelease")
                .FromTable("GameMetadata");

            // Credits are company credits now (developer/publisher/porting/supporting), drop the old cast and crew
            Delete.FromTable("Credits").AllRows();
            Delete.Column("Character")
                .Column("Department")
                .Column("Job")
                .FromTable("Credits");
            Rename.Column("PersonIgdbId").OnTable("Credits").To("CompanyIgdbId");

            // Collections remember the platform to add missing games for (0 = first enabled platform of each game)
            Alter.Table("Collections").AddColumn("PlatformId").AsInt32().WithDefaultValue(0);

            // Import lists and exclusions: platform aware, 0 = any platform
            Alter.Table("ImportListGames").AddColumn("PlatformId").AsInt32().WithDefaultValue(0);

            Alter.Table("ImportExclusions").AddColumn("PlatformId").AsInt32().WithDefaultValue(0);
            IfDatabase(ProcessorIdConstants.PostgreSQL).Execute.Sql("ALTER TABLE \"ImportExclusions\" DROP CONSTRAINT IF EXISTS \"IX_ImportExclusions_IgdbId\"");
            IfDatabase(ProcessorIdConstants.PostgreSQL).Execute.Sql("ALTER TABLE \"ImportExclusions\" DROP CONSTRAINT IF EXISTS \"ImportExclusions_IgdbId_key\"");
            IfDatabase(ProcessorIdConstants.PostgreSQL).Execute.Sql("DROP INDEX IF EXISTS \"IX_ImportExclusions_IgdbId\"");
            IfDatabase(ProcessorIdConstants.SQLite).Delete.Index("IX_ImportExclusions_IgdbId").OnTable("ImportExclusions");
            IfDatabase(ProcessorIdConstants.SQLite).Alter.Table("ImportExclusions").AlterColumn("IgdbId").AsInt64().NotNullable();

            Create.Index("IX_ImportExclusions_IgdbId_PlatformId").OnTable("ImportExclusions")
                .OnColumn("IgdbId").Ascending()
                .OnColumn("PlatformId").Ascending()
                .WithOptions().Unique();

            // New default folder layout: {Root}/{Platform Folder}/{Game Title} ({Release Year})
            Execute.Sql("UPDATE \"NamingConfig\" SET \"GameFolderFormat\" = '{Platform Folder}/{Game Title} ({Release Year})' WHERE \"GameFolderFormat\" = '{Game Title} ({Release Year})' OR \"GameFolderFormat\" IS NULL");
        }
    }
}
