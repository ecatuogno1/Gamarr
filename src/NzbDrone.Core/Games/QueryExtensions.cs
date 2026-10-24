using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Games
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<Game> AllWithYear(this IEnumerable<Game> query, int? year)
        {
            if (!year.HasValue)
            {
                return query;
            }

            var games = query.ToList();
            var exact = games.Where(game => game.Year == year || game.GameMetadata.Value.SecondaryYear == year).ToList();

            // Ports and regional releases often come out a year apart, a release's year can be off by one
            return exact.Any() ? exact : games.Where(game => System.Math.Abs(game.Year - year.Value) <= 1);
        }
    }
}
