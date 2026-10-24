using System.Collections.Generic;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Model
{
    public class ParsedGameInfo
    {
        public ParsedGameInfo()
        {
            GameTitles = new List<string>();
            Languages = new List<Language>();
            Regions = new List<Region>();
        }

        public List<string> GameTitles { get; set; }
        public string OriginalTitle { get; set; }
        public string ReleaseTitle { get; set; }
        public string SimpleReleaseTitle { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public string Edition { get; set; }
        public int Year { get; set; }
        public int IgdbId { get; set; }

        // The platform as detected from the release (IGDB platform id, stable across databases) and the
        // matching Gamarr platform id once it's been resolved against the platform table (0 = unknown).
        public int PlatformIgdbId { get; set; }
        public int PlatformId { get; set; }
        public List<Region> Regions { get; set; }
        public string Version { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public int DiscNumber { get; set; }
        public int DiscCount { get; set; }

        // Switch title id (16 hex digits) when present
        public string TitleId { get; set; }

        public string GameTitle => PrimaryGameTitle;

        public string PrimaryGameTitle
        {
            get
            {
                if (GameTitles.Count > 0)
                {
                    return GameTitles[0];
                }

                return null;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} {2}", PrimaryGameTitle, Year, Quality);
        }
    }
}
