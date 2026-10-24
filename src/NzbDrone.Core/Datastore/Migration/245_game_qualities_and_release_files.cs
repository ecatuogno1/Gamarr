using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    // Replaces Radarr's video qualities with the game qualities, rewrites quality profiles and custom formats
    // that used video-only conditions, adds release details to game files and the companion files table.
    [Migration(245)]
    public class game_qualities_and_release_files : NzbDroneMigrationBase
    {
        private const string UnknownQuality = "{\"quality\":0,\"revision\":{\"version\":1,\"real\":0,\"isRepack\":false}}";

        // Game quality ids in weight order (see Quality.DefaultQualityDefinitions)
        private static readonly int[] QualityIds = { 0, 100, 101, 110, 111, 112, 113, 120, 121, 122, 123, 124, 130, 131, 132, 133, 134 };

        protected override void MainDbUpgrade()
        {
            // QualityDefinitionService inserts the game definitions on startup
            Execute.Sql("DELETE FROM \"QualityDefinitions\"");

            // Old video qualities can't be mapped to game qualities
            Execute.Sql($"UPDATE \"GameFiles\" SET \"Quality\" = '{UnknownQuality}'");
            Execute.Sql($"UPDATE \"History\" SET \"Quality\" = '{UnknownQuality}'");
            Execute.Sql($"UPDATE \"Blocklist\" SET \"Quality\" = '{UnknownQuality}'");
            Execute.Sql("DELETE FROM \"PendingReleases\"");

            Execute.WithConnection(UpdateQualityProfiles);
            Execute.WithConnection(UpdateCustomFormats);

            Alter.Table("GameFiles")
                .AddColumn("Regions").AsString().Nullable()
                .AddColumn("Version").AsString().Nullable()
                .AddColumn("ReleaseType").AsInt32().WithDefaultValue(0)
                .AddColumn("DiscNumber").AsInt32().WithDefaultValue(0)
                .AddColumn("DiscCount").AsInt32().WithDefaultValue(0);

            Delete.Column("MediaInfo").FromTable("GameFiles");

            Create.TableForModel("CompanionFiles")
                .WithColumn("GameId").AsInt32().NotNullable()
                .WithColumn("GameFileId").AsInt32().Nullable()
                .WithColumn("RelativePath").AsString().NotNullable()
                .WithColumn("Extension").AsString().Nullable()
                .WithColumn("Added").AsDateTimeOffset().NotNullable()
                .WithColumn("LastUpdated").AsDateTimeOffset().NotNullable();

            Create.Index("IX_CompanionFiles_GameId").OnTable("CompanionFiles").OnColumn("GameId");
            Create.Index("IX_CompanionFiles_GameFileId").OnTable("CompanionFiles").OnColumn("GameFileId");

            // Removed movie settings
            Execute.Sql("DELETE FROM \"Config\" WHERE \"Key\" IN ('enablemediainfo', 'allowhardcodedsubs', 'whitelistedhardcodedsubs')");
            Execute.Sql("UPDATE \"Config\" SET \"Value\" = 'none' WHERE \"Key\" = 'filedate' AND LOWER(\"Value\") = 'cinemas'");

            // Movie naming tokens are gone, the old default file format becomes the new default
            Execute.Sql("UPDATE \"NamingConfig\" SET \"StandardGameFormat\" = '{Game Title} ({Release Year}){ (Region)}{ [Version]}' WHERE \"StandardGameFormat\" = '{Game Title} ({Release Year}) {Quality Full}' OR \"StandardGameFormat\" LIKE '%{MediaInfo%' OR \"StandardGameFormat\" LIKE '%{ImdbId}%' OR \"StandardGameFormat\" LIKE '%{IgdbId}%'");
        }

        private void UpdateQualityProfiles(IDbConnection conn, IDbTransaction tran)
        {
            var profiles = conn.Query<QualityProfile245>("SELECT \"Id\", \"Name\" FROM \"QualityProfiles\"", transaction: tran).ToList();

            if (!profiles.Any())
            {
                // Fresh database, QualityProfileService adds the default profiles on startup
                return;
            }

            // Existing profiles allow every game quality, like the "Any" default
            var anyItems = BuildItems(QualityIds);

            conn.Execute("UPDATE \"QualityProfiles\" SET \"Items\" = @Items, \"Cutoff\" = 101",
                new { Items = anyItems },
                transaction: tran);

            var formatIds = conn.Query<int>("SELECT \"Id\" FROM \"CustomFormats\"", transaction: tran).ToList();
            var formatItems = JsonSerializer.Serialize(formatIds.Select(id => new { format = id, score = 0 }));

            var defaults = new List<(string Name, int Cutoff, int[] Allowed)>
            {
                ("Verified ROMs / Redump", 101, new[] { 101, 113 }),
                ("PC – GOG preferred", 134, new[] { 130, 131, 132, 133, 134 }),
                ("PC – Scene/Repack", 133, new[] { 132, 133 })
            };

            foreach (var profile in defaults.Where(d => profiles.All(p => p.Name != d.Name)))
            {
                conn.Execute("INSERT INTO \"QualityProfiles\" (\"Name\", \"Cutoff\", \"Items\", \"Language\", \"FormatItems\", \"MinFormatScore\", \"CutoffFormatScore\", \"MinUpgradeFormatScore\", \"UpgradeAllowed\") " +
                             "VALUES (@Name, @Cutoff, @Items, -1, @FormatItems, 0, 0, 1, @UpgradeAllowed)",
                    new
                    {
                        profile.Name,
                        profile.Cutoff,
                        Items = BuildItems(profile.Allowed),
                        FormatItems = formatItems,
                        UpgradeAllowed = true
                    },
                    transaction: tran);
            }
        }

        private static string BuildItems(int[] allowed)
        {
            var items = QualityIds.Select(id => new
            {
                quality = id,
                items = System.Array.Empty<object>(),
                allowed = allowed.Contains(id)
            });

            return JsonSerializer.Serialize(items);
        }

        private void UpdateCustomFormats(IDbConnection conn, IDbTransaction tran)
        {
            var formats = conn.Query<CustomFormat245>("SELECT \"Id\", \"Specifications\" FROM \"CustomFormats\"", transaction: tran).ToList();
            var removedTypes = new[] { "ResolutionSpecification", "QualityModifierSpecification" };

            foreach (var format in formats)
            {
                if (string.IsNullOrWhiteSpace(format.Specifications))
                {
                    continue;
                }

                var specifications = JsonNode.Parse(format.Specifications)?.AsArray();

                if (specifications == null)
                {
                    continue;
                }

                var kept = new JsonArray();
                var changed = false;

                foreach (var specification in specifications)
                {
                    var type = specification?["type"]?.GetValue<string>();

                    if (removedTypes.Contains(type))
                    {
                        changed = true;
                        continue;
                    }

                    // Video sources don't exist anymore, the source condition now means cartridge/disc/digital/PC
                    if (type == "SourceSpecification")
                    {
                        changed = true;
                        continue;
                    }

                    kept.Add(specification?.DeepClone());
                }

                if (changed)
                {
                    conn.Execute("UPDATE \"CustomFormats\" SET \"Specifications\" = @Specifications WHERE \"Id\" = @Id",
                        new { Specifications = kept.ToJsonString(), format.Id },
                        transaction: tran);
                }
            }
        }

        private class QualityProfile245
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class CustomFormat245
        {
            public int Id { get; set; }
            public string Specifications { get; set; }
        }
    }
}
