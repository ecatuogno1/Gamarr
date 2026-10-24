using System.Collections.Generic;
using NzbDrone.Core.Games;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewGame
    {
        // igdbPlatformId limits the results to games released on that IGDB platform (0 = any platform)
        List<Game> SearchForNewGame(string title, int igdbPlatformId = 0);

        GameMetadata MapGameToIgdbGame(GameMetadata game);
    }
}
