using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(238)]
    public class parse_title_from_existing_subtitle_files : NzbDroneMigrationBase
    {
        // Gamarr: subtitle extras were removed, so the title parsing of existing subtitle files is no longer performed.
        protected override void MainDbUpgrade()
        {
            Alter.Table("SubtitleFiles").AddColumn("Title").AsString().Nullable();
            Alter.Table("SubtitleFiles").AddColumn("Copy").AsInt32().WithDefaultValue(0);
        }
    }
}
