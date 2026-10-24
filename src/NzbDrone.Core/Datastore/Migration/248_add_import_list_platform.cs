using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    // Default platform of an import list for items without one, 0 = the game's first IGDB platform
    [Migration(248)]
    public class add_import_list_platform : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ImportLists").AddColumn("PlatformId").AsInt32().WithDefaultValue(0);
        }
    }
}
