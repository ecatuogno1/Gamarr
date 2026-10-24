using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Platforms;

namespace NzbDrone.Core.Datastore.Migration
{
    // IGDB slug of each platform, PlatformIgdbSyncService uses it to correct IgdbPlatformId
    [Migration(247)]
    public class add_platform_slug : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Platforms").AddColumn("Slug").AsString().Nullable();

            foreach (var platform in PlatformDefinitions.All)
            {
                Update.Table("Platforms").Set(new { Slug = platform.Slug }).Where(new { IgdbPlatformId = platform.IgdbPlatformId });
            }
        }
    }
}
