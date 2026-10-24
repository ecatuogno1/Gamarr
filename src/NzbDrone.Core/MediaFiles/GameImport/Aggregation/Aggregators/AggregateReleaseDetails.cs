using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.GameImport.Aggregation.Aggregators
{
    // Regions, version, release type and disc of a file, preferring the release name (download client item),
    // then the folder and then the file name.
    public class AggregateReleaseDetails : IAggregateLocalGame
    {
        public int Order => 1;

        public LocalGame Aggregate(LocalGame localGame, DownloadClientItem downloadClientItem)
        {
            var sources = new List<ParsedGameInfo>
            {
                localGame.DownloadClientGameInfo,
                localGame.FolderGameInfo,
                localGame.FileGameInfo
            }.Where(i => i != null).ToList();

            localGame.Regions = sources.Select(s => s.Regions).FirstOrDefault(r => r != null && r.Any()) ?? new List<Region>();
            localGame.Version = sources.Select(s => s.Version).FirstOrDefault(v => v.IsNotNullOrWhiteSpace());
            localGame.ReleaseType = sources.Select(s => s.ReleaseType).FirstOrDefault(t => t != ReleaseType.Base);

            // The disc of the file itself, not the release
            var discSource = localGame.FileGameInfo?.DiscNumber > 0 ? localGame.FileGameInfo : sources.FirstOrDefault(s => s.DiscNumber > 0);

            localGame.DiscNumber = discSource?.DiscNumber ?? 0;
            localGame.DiscCount = sources.Select(s => s.DiscCount).FirstOrDefault(c => c > 0);

            return localGame;
        }
    }
}
