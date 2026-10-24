using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Platforms
{
    public interface IPlatformService
    {
        List<Platform> All();
        List<Platform> Enabled();
        Platform Get(int id);
        Platform Find(int id);
        Platform FindByIgdbPlatformId(int igdbPlatformId);
        List<Platform> FindByIgdbPlatformIds(IEnumerable<int> igdbPlatformIds);
        List<Platform> FindByFolderName(string folderName);
        Platform Update(Platform platform);
        void ClearCache();
    }

    public class PlatformService : IPlatformService, IHandle<ApplicationStartedEvent>
    {
        private readonly IPlatformRepository _repo;
        private readonly ICached<Dictionary<int, Platform>> _cache;
        private readonly Logger _logger;

        public PlatformService(IPlatformRepository repo, ICacheManager cacheManager, Logger logger)
        {
            _repo = repo;
            _cache = cacheManager.GetCache<Dictionary<int, Platform>>(GetType());
            _logger = logger;
        }

        private Dictionary<int, Platform> GetAll()
        {
            return _cache.Get("all", () => _repo.All().ToDictionary(p => p.Id), TimeSpan.FromMinutes(5));
        }

        public List<Platform> All()
        {
            return GetAll().Values.OrderBy(p => p.Name, StringComparer.InvariantCultureIgnoreCase).ToList();
        }

        public List<Platform> Enabled()
        {
            return All().Where(p => p.Enabled).ToList();
        }

        public Platform Get(int id)
        {
            var platform = Find(id);

            if (platform == null)
            {
                throw new ModelNotFoundException(typeof(Platform), id);
            }

            return platform;
        }

        public Platform Find(int id)
        {
            return GetAll().TryGetValue(id, out var platform) ? platform : null;
        }

        public Platform FindByIgdbPlatformId(int igdbPlatformId)
        {
            var all = GetAll().Values.ToList();

            // The parser knows platforms by the ids in PlatformDefinitions, an id corrected from IGDB is found by slug
            return all.FirstOrDefault(p => p.IgdbPlatformId == igdbPlatformId) ??
                   FindBySlug(all, PlatformDefinitions.Get(igdbPlatformId)?.Slug);
        }

        public List<Platform> FindByIgdbPlatformIds(IEnumerable<int> igdbPlatformIds)
        {
            if (igdbPlatformIds == null)
            {
                return new List<Platform>();
            }

            var all = GetAll().Values.ToList();

            // Keep the order IGDB returned the platforms in
            return igdbPlatformIds.Distinct()
                .Select(id => all.FirstOrDefault(p => p.IgdbPlatformId == id))
                .Where(p => p != null)
                .ToList();
        }

        public List<Platform> FindByFolderName(string folderName)
        {
            if (folderName.IsNullOrWhiteSpace())
            {
                return new List<Platform>();
            }

            return GetAll().Values.Where(p => p.FolderName.Equals(folderName, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public Platform Update(Platform platform)
        {
            var existing = _repo.Get(platform.Id);

            // Only the enabled flag is user editable, the rest is reference data
            existing.Enabled = platform.Enabled;

            _repo.SetFields(existing, p => p.Enabled);
            _cache.Clear();

            return existing;
        }

        // Clears the cache after PlatformIgdbSyncService changed platforms
        public void ClearCache()
        {
            _cache.Clear();
        }

        private static Platform FindBySlug(IEnumerable<Platform> platforms, string slug)
        {
            return slug.IsNullOrWhiteSpace() ? null : platforms.FirstOrDefault(p => slug.Equals(p.Slug, StringComparison.OrdinalIgnoreCase));
        }

        // A definition is the same platform as a row with its slug, or with its IGDB id when the row has no slug yet
        private static Platform FindExisting(List<Platform> existing, Platform definition)
        {
            return FindBySlug(existing, definition.Slug) ??
                   existing.FirstOrDefault(p => p.IgdbPlatformId == definition.IgdbPlatformId && p.Slug.IsNullOrWhiteSpace());
        }

        private void InsertMissingPlatforms()
        {
            var existing = _repo.All().ToList();
            var missing = new List<Platform>();

            foreach (var definition in PlatformDefinitions.All)
            {
                var platform = FindExisting(existing, definition);

                if (platform == null)
                {
                    // Another row may hold the IGDB id (unique) after a correction, the definition is outdated then
                    if (existing.All(p => p.IgdbPlatformId != definition.IgdbPlatformId))
                    {
                        missing.Add(PlatformDefinitions.Copy(definition));
                    }
                }
                else if (platform.Slug.IsNullOrWhiteSpace())
                {
                    platform.Slug = definition.Slug;
                    _repo.SetFields(platform, p => p.Slug);
                }
            }

            if (missing.Any())
            {
                _logger.Debug("Adding {0} missing platforms", missing.Count);
                _repo.InsertMany(missing);
            }

            _cache.Clear();
        }

        public void Handle(ApplicationStartedEvent message)
        {
            InsertMissingPlatforms();
        }
    }
}
