using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Games;
using NzbDrone.Core.Platforms;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public abstract class SearchCriteriaBase
    {
        private static readonly Regex SpecialCharacter = new Regex(@"['.\u0060\u00B4\u2018\u2019]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex NonWord = new Regex(@"[\W]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex BeginningThe = new Regex(@"^the\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public Game Game { get; set; }

        // The game's platform, its Newznab categories restrict the search
        public Platform Platform { get; set; }
        public List<string> SceneTitles { get; set; }
        public virtual bool MonitoredEpisodesOnly { get; set; }
        public virtual bool UserInvokedSearch { get; set; }
        public virtual bool InteractiveSearch { get; set; }

        public List<string> CleanSceneTitles => SceneTitles.Select(GetCleanSceneTitle).Distinct().ToList();

        // Query titles with the platform's short name appended ("Super Metroid SNES"), used as a fallback search
        public List<string> GetPlatformQueryTitles(IEnumerable<string> queryTitles, bool clean)
        {
            var shortName = Platform?.ShortName;

            if (shortName.IsNullOrWhiteSpace() || queryTitles == null)
            {
                return new List<string>();
            }

            var platformTerm = clean ? GetCleanSceneTitle(shortName) : shortName;
            var separator = clean ? "+" : " ";

            if (platformTerm.IsNullOrWhiteSpace())
            {
                return new List<string>();
            }

            return queryTitles.Where(t => t.IsNotNullOrWhiteSpace())
                .Select(t => $"{t}{separator}{platformTerm}")
                .Distinct()
                .ToList();
        }

        public static string GetCleanSceneTitle(string title)
        {
            Ensure.That(title, () => title).IsNotNullOrWhiteSpace();

            var cleanTitle = BeginningThe.Replace(title, string.Empty);

            cleanTitle = cleanTitle.Replace("&", "and");
            cleanTitle = SpecialCharacter.Replace(cleanTitle, "");
            cleanTitle = NonWord.Replace(cleanTitle, "+");

            // remove any repeating +s
            cleanTitle = Regex.Replace(cleanTitle, @"\+{2,}", "+");
            cleanTitle = cleanTitle.RemoveAccent();
            return cleanTitle.Trim('+', ' ');
        }
    }
}
