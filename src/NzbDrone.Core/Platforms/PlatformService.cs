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
            return GetAll().Values.FirstOrDefault(p => p.IgdbPlatformId == igdbPlatformId);
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

        private void InsertMissingPlatforms()
        {
            var existing = _repo.All().ToList();
            var existingIgdbIds = existing.Select(p => p.IgdbPlatformId).ToHashSet();

            var missing = PlatformDefinitions.All
                .Where(p => !existingIgdbIds.Contains(p.IgdbPlatformId))
                .Select(PlatformDefinitions.Copy)
                .ToList();

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
