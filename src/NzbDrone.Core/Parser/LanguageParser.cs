using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Parser
{
    // Parses languages of game releases: No-Intro/Redump lists "(En,Fr,De)", TOSEC "(en-de)",
    // translation tags "[T+Eng]", and scene/repack words ("ENGLISH", "GER", "Multi5" carries no list).
    public static class LanguageParser
    {
        // "(En,Fr,De,Es,It)", "(En)", "(En-US,Fr)", TOSEC "(en)" "(de-fr)"
        private static readonly Regex LanguageListRegex = new(@"[\(\[](?<langs>(?:[A-Z][a-z]|[a-z]{2})(?:-[A-Za-z]{2})?(?:[,+](?:[A-Z][a-z]|[a-z]{2})(?:-[A-Za-z]{2})?)*)[\)\]]",
            RegexOptions.Compiled);

        private static readonly Regex TosecLanguageListRegex = new(@"\((?<langs>[a-z]{2}(?:-[a-z]{2})+)\)", RegexOptions.Compiled);

        // GoodTools/fan translations: "[T+Eng]", "[T-Ger1.00_Author]", "[T+Spa100%]"
        private static readonly Regex TranslationRegex = new(@"\[T[+-]?(?<lang>[A-Z][a-z]{2})[^\]]*\]", RegexOptions.Compiled);

        private static readonly Regex LanguageWordRegex = new(@"(?<=^|[^a-z0-9])(?:
                                                                (?<english>english|eng)|
                                                                (?<french>french|fra|fre|vf|truefrench)|
                                                                (?<german>german|deutsch|ger)|
                                                                (?<spanish>spanish|espanol|español|castellano|spa|esp)|
                                                                (?<spanishlatino>latino|spanish[._ -]latin)|
                                                                (?<italian>italian|ita)|
                                                                (?<portuguesebr>brazilian|pt-?br|portugues[._ -]brasil)|
                                                                (?<portuguese>portuguese)|
                                                                (?<dutch>dutch)|
                                                                (?<russian>russian|rus)|
                                                                (?<polish>polish|pol)|
                                                                (?<japanese>japanese)|
                                                                (?<korean>korean)|
                                                                (?<chinese>chinese|chs|cht)|
                                                                (?<swedish>swedish)|
                                                                (?<danish>danish)|
                                                                (?<norwegian>norwegian)|
                                                                (?<finnish>finnish)|
                                                                (?<czech>czech)|
                                                                (?<hungarian>hungarian)|
                                                                (?<turkish>turkish)|
                                                                (?<greek>greek)|
                                                                (?<arabic>arabic)|
                                                                (?<ukrainian>ukrainian|ukr)
                                                              )(?=$|[^a-z0-9])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Dictionary<string, Language> WordGroups = new()
        {
            { "english", Language.English },
            { "french", Language.French },
            { "german", Language.German },
            { "spanish", Language.Spanish },
            { "spanishlatino", Language.SpanishLatino },
            { "italian", Language.Italian },
            { "portuguesebr", Language.PortugueseBR },
            { "portuguese", Language.Portuguese },
            { "dutch", Language.Dutch },
            { "russian", Language.Russian },
            { "polish", Language.Polish },
            { "japanese", Language.Japanese },
            { "korean", Language.Korean },
            { "chinese", Language.Chinese },
            { "swedish", Language.Swedish },
            { "danish", Language.Danish },
            { "norwegian", Language.Norwegian },
            { "finnish", Language.Finnish },
            { "czech", Language.Czech },
            { "hungarian", Language.Hungarian },
            { "turkish", Language.Turkish },
            { "greek", Language.Greek },
            { "arabic", Language.Arabic },
            { "ukrainian", Language.Ukrainian }
        };

        public static List<Language> ParseLanguages(string title)
        {
            var languages = new List<Language>();

            if (string.IsNullOrWhiteSpace(title))
            {
                return new List<Language> { Language.Unknown };
            }

            foreach (Match match in LanguageListRegex.Matches(title))
            {
                var codes = match.Groups["langs"].Value.Split(',', '+');
                var parsed = codes.Select(c => IsoLanguages.Find(c)?.Language).ToList();

                // Only a list where every code is a language counts, "(Fr)" is fine, "(Uk)" isn't a language list
                if (parsed.All(l => l != null))
                {
                    languages.AddRange(parsed);
                }
            }

            foreach (Match match in TosecLanguageListRegex.Matches(title))
            {
                var parsed = match.Groups["langs"].Value.Split('-').Select(c => IsoLanguages.Find(c)?.Language).ToList();

                if (parsed.All(l => l != null))
                {
                    languages.AddRange(parsed);
                }
            }

            foreach (Match match in TranslationRegex.Matches(title))
            {
                var language = IsoLanguages.Find(match.Groups["lang"].Value.ToLowerInvariant())?.Language;

                if (language != null)
                {
                    languages.Add(language);
                }
            }

            foreach (Match match in LanguageWordRegex.Matches(title))
            {
                foreach (var group in WordGroups)
                {
                    if (match.Groups[group.Key].Success)
                    {
                        languages.Add(group.Value);
                    }
                }
            }

            if (!languages.Any())
            {
                languages.Add(Language.Unknown);
            }

            return languages.DistinctBy(l => (int)l).ToList();
        }
    }
}
