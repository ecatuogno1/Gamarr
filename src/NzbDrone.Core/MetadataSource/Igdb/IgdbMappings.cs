using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Games;
using NzbDrone.Core.MetadataSource.Igdb.Resource;

namespace NzbDrone.Core.MetadataSource.Igdb
{
    // Translates IGDB enums and lookup tables. IGDB replaced several enum fields with references to lookup tables
    // in 2024-2025 (status -> game_status, category -> game_type, websites.category -> websites.type,
    // age_ratings.category/rating -> organization/rating_category). Both forms are handled here.
    public static class IgdbMappings
    {
        public const string ImageUrlFormat = "https://images.igdb.com/igdb/image/upload/t_{0}/{1}.jpg";

        private static readonly Dictionary<int, string> AgeRatingOrganizations = new()
        {
            { 1, "ESRB" },
            { 2, "PEGI" },
            { 3, "CERO" },
            { 4, "USK" },
            { 5, "GRAC" },
            { 6, "CLASS_IND" },
            { 7, "ACB" }
        };

        private static readonly Dictionary<int, string> AgeRatingValues = new()
        {
            { 1, "3" }, { 2, "7" }, { 3, "12" }, { 4, "16" }, { 5, "18" },
            { 6, "RP" }, { 7, "EC" }, { 8, "E" }, { 9, "E10+" }, { 10, "T" }, { 11, "M" }, { 12, "AO" },
            { 13, "A" }, { 14, "B" }, { 15, "C" }, { 16, "D" }, { 17, "Z" },
            { 18, "0" }, { 19, "6" }, { 20, "12" }, { 21, "16" }, { 22, "18" },
            { 23, "All" }, { 24, "12" }, { 25, "15" }, { 26, "18" }, { 27, "Testing" },
            { 28, "L" }, { 29, "10" }, { 30, "12" }, { 31, "14" }, { 32, "16" }, { 33, "18" },
            { 34, "G" }, { 35, "PG" }, { 36, "M" }, { 37, "MA15+" }, { 38, "R18+" }, { 39, "RC" }
        };

        private static readonly Dictionary<int, string> WebsiteCategories = new()
        {
            { 1, "Official" },
            { 2, "Wikia" },
            { 3, "Wikipedia" },
            { 4, "Facebook" },
            { 5, "Twitter" },
            { 6, "Twitch" },
            { 8, "Instagram" },
            { 9, "YouTube" },
            { 10, "iPhone" },
            { 11, "iPad" },
            { 12, "Android" },
            { 13, "Steam" },
            { 14, "Reddit" },
            { 15, "Itch" },
            { 16, "Epic" },
            { 17, "GOG" },
            { 18, "Discord" },
            { 19, "Bluesky" }
        };

        private static readonly Dictionary<int, string> GameStatuses = new()
        {
            { 0, "released" },
            { 2, "alpha" },
            { 3, "beta" },
            { 4, "earlyaccess" },
            { 5, "offline" },
            { 6, "cancelled" },
            { 7, "rumored" },
            { 8, "delisted" }
        };

        public static string ImageUrl(string imageId, string size)
        {
            return imageId.IsNullOrWhiteSpace() ? null : string.Format(ImageUrlFormat, size, imageId);
        }

        public static DateTime? FromUnixTime(long? seconds)
        {
            if (!seconds.HasValue)
            {
                return null;
            }

            return DateTimeOffset.FromUnixTimeSeconds(seconds.Value).UtcDateTime;
        }

        public static long ToUnixTime(DateTime date)
        {
            return new DateTimeOffset(DateTime.SpecifyKind(date, DateTimeKind.Utc)).ToUnixTimeSeconds();
        }

        public static string GetStatusKey(IgdbGameResource resource)
        {
            var status = resource.GameStatus?.Status;

            if (status.IsNotNullOrWhiteSpace())
            {
                return Normalize(status);
            }

            if (resource.Status.HasValue && GameStatuses.TryGetValue(resource.Status.Value, out var oldStatus))
            {
                return oldStatus;
            }

            return null;
        }

        public static GameStatusType MapStatus(IgdbGameResource resource, DateTime now)
        {
            var status = GetStatusKey(resource);

            switch (status)
            {
                case "rumored":
                    return GameStatusType.TBA;
                case "cancelled":
                    return GameStatusType.Announced;
            }

            var releaseDates = new List<DateTime?> { FromUnixTime(resource.FirstReleaseDate) };

            if (resource.ReleaseDates != null)
            {
                releaseDates.AddRange(resource.ReleaseDates.Select(d => FromUnixTime(d.Date)));
            }

            if (releaseDates.Any(d => d.HasValue && d.Value <= now))
            {
                return GameStatusType.Released;
            }

            if ((status is "released" or "earlyaccess" or "offline" or "delisted") && !releaseDates.Any(d => d.HasValue))
            {
                return GameStatusType.Released;
            }

            return GameStatusType.Announced;
        }

        public static AgeRating MapAgeRating(IgdbAgeRating resource)
        {
            var organization = resource.Organization?.Name;

            if (organization.IsNullOrWhiteSpace())
            {
                var organizationId = resource.Category ?? resource.Organization?.Id;

                if (organizationId.HasValue)
                {
                    AgeRatingOrganizations.TryGetValue(organizationId.Value, out organization);
                }
            }

            var rating = resource.RatingCategory?.Rating;

            if (rating.IsNullOrWhiteSpace() && resource.Rating.HasValue)
            {
                AgeRatingValues.TryGetValue(resource.Rating.Value, out rating);
            }

            if (organization.IsNullOrWhiteSpace() || rating.IsNullOrWhiteSpace())
            {
                return null;
            }

            return new AgeRating
            {
                Organization = organization,
                Rating = rating
            };
        }

        public static GameWebsite MapWebsite(IgdbWebsite resource)
        {
            if (resource.Url.IsNullOrWhiteSpace())
            {
                return null;
            }

            var type = resource.Type?.Type;

            if (type.IsNotNullOrWhiteSpace())
            {
                if (type.ContainsIgnoreCase("official"))
                {
                    type = "Official";
                }
            }
            else
            {
                var category = resource.Category ?? resource.Type?.Id;

                if (!category.HasValue || !WebsiteCategories.TryGetValue(category.Value, out type))
                {
                    type = "Other";
                }
            }

            return new GameWebsite
            {
                Url = resource.Url,
                Type = type
            };
        }

        private static string Normalize(string value)
        {
            return new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        }
    }
}
