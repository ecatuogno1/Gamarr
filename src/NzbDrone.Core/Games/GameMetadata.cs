using System;
using System.Collections.Generic;
using Equ;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Games.AlternativeTitles;
using NzbDrone.Core.Games.Translations;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Games
{
    public class GameMetadata : Entity<GameMetadata>
    {
        public GameMetadata()
        {
            AlternativeTitles = new List<AlternativeTitle>();
            Translations = new List<GameTranslation>();
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            Themes = new List<string>();
            GameModes = new List<string>();
            Keywords = new List<string>();
            Developers = new List<string>();
            Publishers = new List<string>();
            AgeRatings = new List<AgeRating>();
            Websites = new List<GameWebsite>();
            PlatformReleaseDates = new List<PlatformReleaseDate>();
            AvailablePlatformIgdbIds = new List<int>();
            OriginalLanguage = Language.English;
            Recommendations = new List<int>();
            Ratings = new Ratings();
        }

        public int IgdbId { get; set; }
        public string IgdbSlug { get; set; }

        public List<MediaCover.MediaCover> Images { get; set; }
        public List<string> Genres { get; set; }
        public List<string> Themes { get; set; }
        public List<string> GameModes { get; set; }
        public List<string> Keywords { get; set; }
        public List<string> Developers { get; set; }
        public List<string> Publishers { get; set; }
        public List<AgeRating> AgeRatings { get; set; }
        public List<GameWebsite> Websites { get; set; }

        // IGDB first_release_date
        public DateTime? ReleaseDate { get; set; }
        public List<PlatformReleaseDate> PlatformReleaseDates { get; set; }
        public List<int> AvailablePlatformIgdbIds { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }

        public int CollectionIgdbId { get; set; }
        public string CollectionTitle { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public string Website { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public GameStatusType Status { get; set; }
        public string Overview { get; set; }
        public string Storyline { get; set; }

        // Get Loaded via a Join Query
        public List<AlternativeTitle> AlternativeTitles { get; set; }
        public List<GameTranslation> Translations { get; set; }

        public int? SecondaryYear { get; set; }
        public string YouTubeTrailerId { get; set; }
        public string OriginalTitle { get; set; }
        public string CleanOriginalTitle { get; set; }
        public Language OriginalLanguage { get; set; }
        public List<int> Recommendations { get; set; }
        public float Popularity { get; set; }

        [MemberwiseEqualityIgnore]
        public bool IsRecentGame => ReleaseDate.HasValue && ReleaseDate.Value >= DateTime.UtcNow.AddDays(-21);

        [MemberwiseEqualityIgnore]
        public string IgdbUrl => IgdbSlug.IsNullOrWhiteSpace() ? null : $"https://www.igdb.com/games/{IgdbSlug}";
    }
}
