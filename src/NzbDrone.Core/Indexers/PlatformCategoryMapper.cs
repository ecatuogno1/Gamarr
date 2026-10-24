using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Platforms;

namespace NzbDrone.Core.Indexers
{
    // Maps the Newznab/Torznab categories of a release (ReleaseInfo.Categories) to Gamarr platforms
    public interface IPlatformCategoryMapper
    {
        // The platforms a release in these categories is for. Empty if the categories don't identify a platform,
        // e.g. only the generic Console (1000) category.
        List<Platform> GetPlatforms(IEnumerable<int> categories);
        List<int> GetPlatformIds(IEnumerable<int> categories);

        // False if the categories clearly belong to another platform (a PS3 release for an SNES game, a console
        // release for a PC game). Releases without categories are always compatible.
        bool IsCompatible(Platform platform, IEnumerable<int> categories);
    }

    public class PlatformCategoryMapper : IPlatformCategoryMapper
    {
        // IGDB platform id of PC (Microsoft Windows), PC/Games (4050) releases are assumed to be Windows games
        public const int PcWindowsIgdbPlatformId = 6;

        private readonly IPlatformService _platformService;

        public PlatformCategoryMapper(IPlatformService platformService)
        {
            _platformService = platformService;
        }

        public List<Platform> GetPlatforms(IEnumerable<int> categories)
        {
            var releaseCategories = categories?.Distinct().ToList() ?? new List<int>();

            if (releaseCategories.Count == 0)
            {
                return new List<Platform>();
            }

            var platforms = _platformService.All();
            var result = GetSpecificConsolePlatforms(platforms, releaseCategories);

            if (releaseCategories.Any(c => c == NewznabStandardCategory.PC || c == NewznabStandardCategory.PCGames))
            {
                var pc = platforms.FirstOrDefault(p => p.IgdbPlatformId == PcWindowsIgdbPlatformId);

                if (pc != null)
                {
                    result.Add(pc);
                }
            }

            return result.DistinctBy(p => p.Id).ToList();
        }

        public List<int> GetPlatformIds(IEnumerable<int> categories)
        {
            return GetPlatforms(categories).Select(p => p.Id).ToList();
        }

        public bool IsCompatible(Platform platform, IEnumerable<int> categories)
        {
            var releaseCategories = categories?.Distinct().ToList() ?? new List<int>();

            if (platform == null || releaseCategories.Count == 0 || platform.NewznabCategories == null || platform.NewznabCategories.Count == 0)
            {
                return true;
            }

            // A console subcategory (1080 PS3) names the platform(s) exactly
            var specific = GetSpecificConsolePlatforms(_platformService.All(), releaseCategories);

            if (specific.Any())
            {
                return specific.Any(p => p.Id == platform.Id);
            }

            // Otherwise only the category group (Console 1xxx, PC 4xxx) has to match
            return releaseCategories.Any(c => platform.NewznabCategories.Any(p => NewznabStandardCategory.IsSameGroup(c, p)));
        }

        private static List<Platform> GetSpecificConsolePlatforms(List<Platform> platforms, List<int> releaseCategories)
        {
            var subcategories = releaseCategories
                .Where(c => !NewznabStandardCategory.IsParentCategory(c) &&
                            c != NewznabStandardCategory.ConsoleOther &&
                            NewznabStandardCategory.IsSameGroup(c, NewznabStandardCategory.Console))
                .ToList();

            if (subcategories.Count == 0)
            {
                return new List<Platform>();
            }

            return platforms.Where(p => p.NewznabCategories != null && p.NewznabCategories.Any(subcategories.Contains)).ToList();
        }
    }
}
