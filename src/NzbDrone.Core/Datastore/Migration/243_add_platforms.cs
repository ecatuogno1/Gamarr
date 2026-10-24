using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Platforms;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(243)]
    public class add_platforms : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Platforms")
                .WithColumn("Name").AsString()
                .WithColumn("ShortName").AsString()
                .WithColumn("IgdbPlatformId").AsInt32().Unique()
                .WithColumn("Family").AsInt32().WithDefaultValue(0)
                .WithColumn("Generation").AsInt32().WithDefaultValue(0)
                .WithColumn("FolderName").AsString()
                .WithColumn("FileExtensions").AsString().WithDefaultValue("[]")
                .WithColumn("NewznabCategories").AsString().WithDefaultValue("[]")
                .WithColumn("Enabled").AsBoolean().WithDefaultValue(true);

            // Seeded in definition order so PC (Microsoft Windows) gets Id 1. PlatformService adds any
            // platforms that are added to PlatformDefinitions later on startup.
            foreach (var platform in PlatformDefinitions.All)
            {
                Insert.IntoTable("Platforms").Row(new
                {
                    Name = platform.Name,
                    ShortName = platform.ShortName,
                    IgdbPlatformId = platform.IgdbPlatformId,
                    Family = (int)platform.Family,
                    Generation = platform.Generation,
                    FolderName = platform.FolderName,
                    FileExtensions = platform.FileExtensions.ToJson(),
                    NewznabCategories = platform.NewznabCategories.ToJson(),
                    Enabled = platform.Enabled
                });
            }
        }
    }
}
