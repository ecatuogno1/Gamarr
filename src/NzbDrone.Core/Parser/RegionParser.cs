using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser
{
    // Parses release regions from No-Intro/Redump ("(USA, Europe)"), GoodTools ("(U)", "(JUE)"),
    // TOSEC ("(US)", "(US-EU)"), Switch ("[US]") and scene ("EUR", "PAL", "NTSC-J") notations.
    public static class RegionParser
    {
        private static readonly Dictionary<string, Region> RegionNames = new(StringComparer.OrdinalIgnoreCase)
        {
            { "World", Region.World },
            { "USA", Region.USA },
            { "America", Region.USA },
            { "United States", Region.USA },
            { "Europe", Region.Europe },
            { "Japan", Region.Japan },
            { "Asia", Region.Asia },
            { "Australia", Region.Australia },
            { "Brazil", Region.Brazil },
            { "Canada", Region.Canada },
            { "China", Region.China },
            { "France", Region.France },
            { "Germany", Region.Germany },
            { "Hong Kong", Region.HongKong },
            { "Italy", Region.Italy },
            { "Korea", Region.Korea },
            { "South Korea", Region.Korea },
            { "Netherlands", Region.Netherlands },
            { "Holland", Region.Netherlands },
            { "Russia", Region.Russia },
            { "Spain", Region.Spain },
            { "Sweden", Region.Sweden },
            { "Taiwan", Region.Taiwan },
            { "UK", Region.UnitedKingdom },
            { "United Kingdom", Region.UnitedKingdom },
            { "Scandinavia", Region.Scandinavia },
            { "Latin America", Region.LatinAmerica },
            { "Mexico", Region.Mexico },
            { "Poland", Region.Poland },
            { "Portugal", Region.Portugal },
            { "Denmark", Region.Denmark },
            { "Finland", Region.Finland },
            { "Norway", Region.Norway },
            { "Greece", Region.Greece },
            { "India", Region.India },
            { "Belgium", Region.Belgium },
            { "Switzerland", Region.Switzerland },
            { "Austria", Region.Austria },
            { "New Zealand", Region.NewZealand },
            { "South Africa", Region.SouthAfrica },
            { "PAL", Region.Europe },
            { "NTSC", Region.USA },
            { "NTSC-U", Region.USA },
            { "NTSC-US", Region.USA },
            { "NTSC-J", Region.Japan },
            { "NTSC-JP", Region.Japan },
            { "EUR", Region.Europe },
            { "JPN", Region.Japan },
            { "JAP", Region.Japan },
            { "KOR", Region.Korea },
            { "CHN", Region.China }
        };

        // TOSEC / Switch / ISO 3166 two letter codes, upper case only (No-Intro uses "En", "De" for languages)
        private static readonly Dictionary<string, Region> CountryCodes = new(StringComparer.Ordinal)
        {
            { "US", Region.USA },
            { "EU", Region.Europe },
            { "JP", Region.Japan },
            { "AS", Region.Asia },
            { "AU", Region.Australia },
            { "BR", Region.Brazil },
            { "CA", Region.Canada },
            { "CN", Region.China },
            { "FR", Region.France },
            { "DE", Region.Germany },
            { "HK", Region.HongKong },
            { "IT", Region.Italy },
            { "KR", Region.Korea },
            { "NL", Region.Netherlands },
            { "RU", Region.Russia },
            { "ES", Region.Spain },
            { "SE", Region.Sweden },
            { "TW", Region.Taiwan },
            { "GB", Region.UnitedKingdom },
            { "UK", Region.UnitedKingdom },
            { "MX", Region.Mexico },
            { "PL", Region.Poland },
            { "PT", Region.Portugal },
            { "DK", Region.Denmark },
            { "FI", Region.Finland },
            { "NO", Region.Norway },
            { "GR", Region.Greece },
            { "IN", Region.India },
            { "BE", Region.Belgium },
            { "CH", Region.Switzerland },
            { "AT", Region.Austria },
            { "NZ", Region.NewZealand },
            { "ZA", Region.SouthAfrica },
            { "WW", Region.World },
            { "GLOBAL", Region.World }
        };

        // GoodTools single letter codes, a group can combine them: "(JUE)"
        private static readonly Dictionary<char, Region> GoodToolsCodes = new()
        {
            { 'U', Region.USA },
            { 'E', Region.Europe },
            { 'J', Region.Japan },
            { 'W', Region.World },
            { 'A', Region.Australia },
            { 'B', Region.Brazil },
            { 'K', Region.Korea },
            { 'C', Region.China },
            { 'F', Region.France },
            { 'G', Region.Germany },
            { 'H', Region.Netherlands },
            { 'I', Region.Italy },
            { 'S', Region.Spain }
        };

        // Scene region tokens, matched case sensitively as whole tokens ("Game.Name.EUR.NSW-GROUP")
        private static readonly Regex SceneRegionRegex = new(@"(?<=^|[._ \-\[(])(?<region>USA|EUR|JPN|JAP|PAL|NTSC(?:-[UJ])?|ASIA|KOR|CHN)(?=$|[._ \-\])])", RegexOptions.Compiled);

        private static readonly Regex GoodToolsGroupRegex = new(@"^(?:[UEJWAKBCFGHIS]{1,4})$", RegexOptions.Compiled);

        public static List<Region> ParseBracketGroup(string content)
        {
            var regions = new List<Region>();

            if (string.IsNullOrWhiteSpace(content))
            {
                return regions;
            }

            content = content.Trim();

            if (RegionNames.TryGetValue(content, out var namedRegion))
            {
                regions.Add(namedRegion);
                return regions;
            }

            if (content.Length <= 4 && GoodToolsGroupRegex.IsMatch(content) && content != "SC")
            {
                // A two letter group like "UK" or "US" is a country code, not GoodTools letters
                if (content.Length == 2 && CountryCodes.TryGetValue(content, out var country))
                {
                    regions.Add(country);
                    return regions;
                }

                regions.AddRange(content.Select(c => GoodToolsCodes[c]).Distinct());
                return regions;
            }

            var items = content.Split(new[] { ',', '+' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var item in items)
            {
                if (RegionNames.TryGetValue(item, out var region))
                {
                    regions.Add(region);
                    continue;
                }

                // TOSEC combines countries with dashes: "US-EU"
                var codes = item.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (codes.Length > 0 && codes.All(c => CountryCodes.ContainsKey(c)))
                {
                    regions.AddRange(codes.Select(c => CountryCodes[c]));
                    continue;
                }

                // Every item has to be a region, otherwise it's some other tag ("(Konami)", "(En,Fr)")
                return new List<Region>();
            }

            return regions.Distinct().ToList();
        }

        public static List<Region> ParseSceneTokens(string title)
        {
            return SceneRegionRegex.Matches(title)
                .Select(m => RegionNames[m.Groups["region"].Value])
                .Distinct()
                .ToList();
        }

        public static Region ParseRegionName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Region.Unknown;
            }

            if (RegionNames.TryGetValue(name.Trim(), out var region))
            {
                return region;
            }

            return CountryCodes.TryGetValue(name.Trim().ToUpperInvariant(), out region) ? region : Region.Unknown;
        }
    }
}
