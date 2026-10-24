using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Games;

namespace Gamarr.Api.V3.Games
{
    public static class ReleaseDateExtensions
    {
        // IGDB release dates are days, stored as midnight UTC. The UI shows them in the browser's time zone, which
        // would move them to the previous day west of UTC, so they're sent as noon UTC (the same day from UTC-12 to UTC+11).
        public static DateTime? ToReleaseDay(this DateTime? date)
        {
            if (!date.HasValue)
            {
                return null;
            }

            var utc = date.Value.ToUniversalTime();

            return utc.TimeOfDay == TimeSpan.Zero ? utc.AddHours(12) : utc;
        }

        public static List<PlatformReleaseDate> ToReleaseDays(this IEnumerable<PlatformReleaseDate> dates)
        {
            return dates?.Select(d => new PlatformReleaseDate { IgdbPlatformId = d.IgdbPlatformId, Date = d.Date.ToReleaseDay() }).ToList();
        }
    }
}
