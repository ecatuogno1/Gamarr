using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class GameSearchCriteria : SearchCriteriaBase
    {
        public override string ToString()
        {
            var platform = Platform?.ShortName ?? Game.Platform?.ShortName;

            return platform.IsNullOrWhiteSpace() ? $"[{Game.Title}]" : $"[{Game.Title}] [{platform}]";
        }
    }
}
