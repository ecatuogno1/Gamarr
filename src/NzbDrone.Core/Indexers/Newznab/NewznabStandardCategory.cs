using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Indexers.Newznab
{
    // The standard Newznab/Torznab categories Gamarr cares about (the same ids Prowlarr and Jackett use)
    public static class NewznabStandardCategory
    {
        public const int Console = 1000;
        public const int ConsoleNds = 1010;
        public const int ConsolePsp = 1020;
        public const int ConsoleWii = 1030;
        public const int ConsoleXbox = 1040;
        public const int ConsoleXbox360 = 1050;
        public const int ConsoleWiiWare = 1060;
        public const int ConsoleXbox360Dlc = 1070;
        public const int ConsolePs3 = 1080;
        public const int ConsoleOther = 1090;
        public const int Console3ds = 1110;
        public const int ConsolePsVita = 1120;
        public const int ConsoleWiiU = 1130;
        public const int ConsoleXboxOne = 1140;
        public const int ConsolePs4 = 1180;

        public const int PC = 4000;
        public const int PC0Day = 4010;
        public const int PCIso = 4020;
        public const int PCMac = 4030;
        public const int PCMobileOther = 4040;
        public const int PCGames = 4050;
        public const int PCMobileIos = 4060;
        public const int PCMobileAndroid = 4070;

        // Default categories for new Newznab/Torznab indexers: all consoles and PC games
        public static readonly int[] DefaultGameCategories =
        {
            Console,
            ConsoleNds,
            ConsolePsp,
            ConsoleWii,
            ConsoleXbox,
            ConsoleXbox360,
            ConsoleWiiWare,
            ConsoleXbox360Dlc,
            ConsolePs3,
            Console3ds,
            ConsolePsVita,
            ConsoleWiiU,
            ConsoleXboxOne,
            ConsolePs4,
            PCGames
        };

        // Used for the category picker when the indexer's caps can't be fetched
        public static List<NewznabCategory> GetDefaultCategoryTree()
        {
            return new List<NewznabCategory>
            {
                new()
                {
                    Id = Console,
                    Name = "Console",
                    Subcategories = new List<NewznabCategory>
                    {
                        new() { Id = ConsoleNds, Name = "NDS" },
                        new() { Id = ConsolePsp, Name = "PSP" },
                        new() { Id = ConsoleWii, Name = "Wii" },
                        new() { Id = ConsoleXbox, Name = "XBox" },
                        new() { Id = ConsoleXbox360, Name = "XBox 360" },
                        new() { Id = ConsoleWiiWare, Name = "Wiiware" },
                        new() { Id = ConsoleXbox360Dlc, Name = "XBox 360 DLC" },
                        new() { Id = ConsolePs3, Name = "PS3" },
                        new() { Id = ConsoleOther, Name = "Other" },
                        new() { Id = Console3ds, Name = "3DS" },
                        new() { Id = ConsolePsVita, Name = "PS Vita" },
                        new() { Id = ConsoleWiiU, Name = "WiiU" },
                        new() { Id = ConsoleXboxOne, Name = "XBox One" },
                        new() { Id = ConsolePs4, Name = "PS4" }
                    }
                },
                new()
                {
                    Id = PC,
                    Name = "PC",
                    Subcategories = new List<NewznabCategory>
                    {
                        new() { Id = PC0Day, Name = "0day" },
                        new() { Id = PCIso, Name = "ISO" },
                        new() { Id = PCMac, Name = "Mac" },
                        new() { Id = PCMobileOther, Name = "Mobile-Other" },
                        new() { Id = PCGames, Name = "Games" },
                        new() { Id = PCMobileIos, Name = "Mobile-iOS" },
                        new() { Id = PCMobileAndroid, Name = "Mobile-Android" }
                    }
                }
            };
        }

        public static bool IsParentCategory(int category)
        {
            return category % 1000 == 0;
        }

        public static bool IsSameGroup(int category, int otherCategory)
        {
            return category / 1000 == otherCategory / 1000;
        }

        // A configured category covers a platform category if it's the same one or its parent (1000 covers 1080)
        public static bool Covers(int configuredCategory, int platformCategory)
        {
            return configuredCategory == platformCategory ||
                   (IsParentCategory(configuredCategory) && IsSameGroup(configuredCategory, platformCategory));
        }

        // The categories to search for a platform: the configured categories that cover one of the platform's
        // categories. If none do (or the platform has none) all configured categories are used.
        public static List<int> GetSearchCategories(IEnumerable<int> configuredCategories, IEnumerable<int> platformCategories)
        {
            var configured = (configuredCategories ?? Enumerable.Empty<int>()).Distinct().ToList();
            var platform = (platformCategories ?? Enumerable.Empty<int>()).Distinct().ToList();

            if (platform.Count == 0)
            {
                return configured;
            }

            var intersection = configured.Where(c => platform.Any(p => Covers(c, p))).ToList();

            return intersection.Count > 0 ? intersection : configured;
        }
    }
}
