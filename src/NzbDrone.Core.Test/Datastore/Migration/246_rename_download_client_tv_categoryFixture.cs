using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class rename_download_client_tv_categoryFixture : MigrationTest<rename_download_client_tv_category>
    {
        [Test]
        public void should_rename_download_station_and_nzbvortex_settings_only()
        {
            var db = WithMigrationTestDb(c =>
            {
                foreach (var implementation in new[] { "TorrentDownloadStation", "NzbVortex", "Transmission" })
                {
                    c.Insert.IntoTable("DownloadClients").Row(new
                    {
                        Enable = true,
                        Name = implementation,
                        Implementation = implementation,
                        Priority = 1,
                        Settings = "{\"host\":\"127.0.0.1\",\"tvCategory\":\"gamarr\",\"tvDirectory\":\"games\"}",
                        ConfigContract = implementation + "Settings"
                    });
                }
            });

            var settings = db.Query<DownloadClientSettings246>("SELECT \"Implementation\", \"Settings\" FROM \"DownloadClients\"").ToDictionary(c => c.Implementation, c => c.Settings);

            settings["TorrentDownloadStation"].Should().Be("{\"host\":\"127.0.0.1\",\"gameCategory\":\"gamarr\",\"gameDirectory\":\"games\"}");
            settings["NzbVortex"].Should().Contain("\"gameCategory\":\"gamarr\"");
            settings["Transmission"].Should().Contain("\"tvCategory\":\"gamarr\"");
        }
    }

    public class DownloadClientSettings246
    {
        public string Implementation { get; set; }
        public string Settings { get; set; }
    }
}
