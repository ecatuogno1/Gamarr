using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Parser.Model
{
    // Release regions. The values are persisted (custom formats, game files), never renumber them.
    public enum Region
    {
        Unknown = 0,
        World = 1,
        USA = 2,
        Europe = 3,
        Japan = 4,
        Asia = 5,
        Australia = 6,
        Brazil = 7,
        Canada = 8,
        China = 9,
        France = 10,
        Germany = 11,
        HongKong = 12,
        Italy = 13,
        Korea = 14,
        Netherlands = 15,
        Russia = 16,
        Spain = 17,
        Sweden = 18,
        Taiwan = 19,
        UnitedKingdom = 20,
        Scandinavia = 21,
        LatinAmerica = 22,
        Mexico = 23,
        Poland = 24,
        Portugal = 25,
        Denmark = 26,
        Finland = 27,
        Norway = 28,
        Greece = 29,
        India = 30,
        Belgium = 31,
        Switzerland = 32,
        Austria = 33,
        NewZealand = 34,
        SouthAfrica = 35
    }

    public static class RegionExtensions
    {
        private static readonly Dictionary<Region, string> Names = new()
        {
            { Region.Unknown, "Unknown" },
            { Region.World, "World" },
            { Region.USA, "USA" },
            { Region.Europe, "Europe" },
            { Region.Japan, "Japan" },
            { Region.Asia, "Asia" },
            { Region.Australia, "Australia" },
            { Region.Brazil, "Brazil" },
            { Region.Canada, "Canada" },
            { Region.China, "China" },
            { Region.France, "France" },
            { Region.Germany, "Germany" },
            { Region.HongKong, "Hong Kong" },
            { Region.Italy, "Italy" },
            { Region.Korea, "Korea" },
            { Region.Netherlands, "Netherlands" },
            { Region.Russia, "Russia" },
            { Region.Spain, "Spain" },
            { Region.Sweden, "Sweden" },
            { Region.Taiwan, "Taiwan" },
            { Region.UnitedKingdom, "UK" },
            { Region.Scandinavia, "Scandinavia" },
            { Region.LatinAmerica, "Latin America" },
            { Region.Mexico, "Mexico" },
            { Region.Poland, "Poland" },
            { Region.Portugal, "Portugal" },
            { Region.Denmark, "Denmark" },
            { Region.Finland, "Finland" },
            { Region.Norway, "Norway" },
            { Region.Greece, "Greece" },
            { Region.India, "India" },
            { Region.Belgium, "Belgium" },
            { Region.Switzerland, "Switzerland" },
            { Region.Austria, "Austria" },
            { Region.NewZealand, "New Zealand" },
            { Region.SouthAfrica, "South Africa" }
        };

        public static string GetName(this Region region)
        {
            return Names.TryGetValue(region, out var name) ? name : region.ToString();
        }
    }

    public class RegionFieldConverter : ISelectOptionsConverter
    {
        public List<SelectOption> GetSelectOptions()
        {
            return System.Enum.GetValues<Region>()
                .Where(r => r != Region.Unknown)
                .Select(r => new SelectOption { Value = (int)r, Name = r.GetName(), Order = (int)r })
                .ToList();
        }
    }
}
