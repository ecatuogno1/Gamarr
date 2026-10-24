using System;
using System.Collections.Generic;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.Collections;
using NzbDrone.Core.Games.Credits;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideGameInfo
    {
        Tuple<GameMetadata, List<Credit>> GetGameInfo(int igdbId);
        GameCollection GetCollectionInfo(int igdbCollectionId);
        List<GameMetadata> GetBulkGameInfo(List<int> igdbIds);
        List<GameMetadata> GetTrendingGames();
        List<GameMetadata> GetPopularGames();

        HashSet<int> GetChangedGames(DateTime startTime, IEnumerable<int> igdbIds);
    }
}
