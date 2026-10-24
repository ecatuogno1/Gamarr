using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Games;

namespace NzbDrone.Core.Platforms
{
    public static class DefaultPlatformSelector
    {
        // Japanese editions of a console that Gamarr keeps as their own platform (IGDB platform id -> western
        // counterpart). Games often came out in Japan first, but the western platform is the one to offer.
        private static readonly Dictionary<int, int> RegionalEditions = new Dictionary<int, int>
        {
            { 99, 18 }, // Family Computer -> NES
            { 58, 19 } // Super Famicom -> SNES
        };

        // The platform a search result is offered on first: the platform the search was filtered by, else the
        // platform the game was first released on (Ocarina of Time on N64, not a later Wii port). Platforms that
        // share the first release date prefer PC, then IGDB's platform order. A Japanese edition only wins when its
        // western counterpart isn't available.
        public static Platform Select(IReadOnlyList<Platform> available, GameMetadata metadata, int requestedPlatformId = 0)
        {
            if (available == null || available.Count == 0)
            {
                return null;
            }

            var requested = available.FirstOrDefault(p => p.Id == requestedPlatformId);

            if (requested != null)
            {
                return requested;
            }

            var releaseDates = (metadata?.PlatformReleaseDates ?? new List<PlatformReleaseDate>())
                .Where(d => d.Date.HasValue)
                .GroupBy(d => d.IgdbPlatformId)
                .ToDictionary(g => g.Key, g => g.Min(d => d.Date.Value));

            var igdbOrder = metadata?.AvailablePlatformIgdbIds ?? new List<int>();

            var candidates = available
                .Where(p => !RegionalEditions.TryGetValue(p.IgdbPlatformId, out var western) || available.All(a => a.IgdbPlatformId != western))
                .ToList();

            return candidates
                .Select((platform, index) => new
                {
                    Platform = platform,
                    Index = index,
                    Released = releaseDates.TryGetValue(platform.IgdbPlatformId, out var date) ? date : DateTime.MaxValue,
                    IgdbIndex = igdbOrder.IndexOf(platform.IgdbPlatformId) is var i and >= 0 ? i : int.MaxValue
                })
                .OrderBy(p => p.Released.Date)
                .ThenByDescending(p => p.Platform.Family == PlatformFamily.PC)
                .ThenBy(p => p.IgdbIndex)
                .ThenBy(p => p.Index)
                .First()
                .Platform;
        }
    }
}
