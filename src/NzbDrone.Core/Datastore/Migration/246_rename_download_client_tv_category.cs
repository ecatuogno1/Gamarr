using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    // Download Station and NZBVortex kept Sonarr's TvCategory/TvDirectory setting names
    [Migration(246)]
    public class rename_download_client_tv_category : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"DownloadClients\" SET \"Settings\" = REPLACE(REPLACE(\"Settings\", '\"tvCategory\"', '\"gameCategory\"'), '\"tvDirectory\"', '\"gameDirectory\"') " +
                        "WHERE \"Implementation\" IN ('UsenetDownloadStation', 'TorrentDownloadStation', 'NzbVortex')");
        }
    }
}
