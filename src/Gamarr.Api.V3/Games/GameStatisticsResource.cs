using System.Collections.Generic;
using NzbDrone.Core.GameStats;
using NzbDrone.Core.Qualities;

namespace Gamarr.Api.V3.Games
{
    public class GameStatisticsResource
    {
        public int GameFileCount { get; set; }
        public long SizeOnDisk { get; set; }
        public List<string> ReleaseGroups { get; set; }
        public List<Quality> GameFileQualities { get; set; }
    }

    public static class GameStatisticsResourceMapper
    {
        public static GameStatisticsResource ToResource(this GameStatistics model)
        {
            if (model == null)
            {
                return null;
            }

            return new GameStatisticsResource
            {
                GameFileCount = model.GameFileCount,
                SizeOnDisk = model.SizeOnDisk,
                ReleaseGroups = model.ReleaseGroups,
                GameFileQualities = model.GameFileQualities,
            };
        }
    }
}
