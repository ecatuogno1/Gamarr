using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Games.Events;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ImportLists.ImportExclusions
{
    public interface IImportListExclusionService
    {
        ImportListExclusion Add(ImportListExclusion importListExclusion);
        List<ImportListExclusion> Add(List<ImportListExclusion> importListExclusions);
        List<ImportListExclusion> All();
        PagingSpec<ImportListExclusion> Paged(PagingSpec<ImportListExclusion> pagingSpec);
        bool IsGameExcluded(int igdbId, int platformId);
        void Delete(int id);
        void Delete(List<int> ids);
        ImportListExclusion Get(int id);
        ImportListExclusion Update(ImportListExclusion importListExclusion);
        List<int> AllExcludedIgdbIds();
    }

    public class ImportListExclusionService : IImportListExclusionService, IHandleAsync<GamesDeletedEvent>
    {
        private readonly IImportListExclusionRepository _repo;
        private readonly Logger _logger;

        public ImportListExclusionService(IImportListExclusionRepository repo,
                             Logger logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public ImportListExclusion Add(ImportListExclusion importListExclusion)
        {
            var existing = _repo.FindByIgdbIdAndPlatform(importListExclusion.IgdbId, importListExclusion.PlatformId);

            if (existing != null)
            {
                return existing;
            }

            return _repo.Insert(importListExclusion);
        }

        public List<ImportListExclusion> Add(List<ImportListExclusion> importListExclusions)
        {
            _repo.InsertMany(DeDupeExclusions(importListExclusions));

            return importListExclusions;
        }

        public bool IsGameExcluded(int igdbId, int platformId)
        {
            return _repo.IsGameExcluded(igdbId, platformId);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }

        public void Delete(List<int> ids)
        {
            _repo.DeleteMany(ids);
        }

        public ImportListExclusion Get(int id)
        {
            return _repo.Get(id);
        }

        public ImportListExclusion Update(ImportListExclusion importListExclusion)
        {
            return _repo.Update(importListExclusion);
        }

        public List<ImportListExclusion> All()
        {
            return _repo.All().ToList();
        }

        public PagingSpec<ImportListExclusion> Paged(PagingSpec<ImportListExclusion> pagingSpec)
        {
            return _repo.GetPaged(pagingSpec);
        }

        public List<int> AllExcludedIgdbIds()
        {
            return _repo.AllExcludedIgdbIds();
        }

        public void HandleAsync(GamesDeletedEvent message)
        {
            if (!message.AddImportListExclusion)
            {
                return;
            }

            _logger.Debug("Adding {0} deleted games to import list exclusions.", message.Games.Count);

            var exclusionsToAdd = DeDupeExclusions(message.Games.Select(m => new ImportListExclusion
            {
                IgdbId = m.IgdbId,
                PlatformId = m.PlatformId,
                GameTitle = m.Title,
                GameYear = m.Year
            }).ToList());

            _repo.InsertMany(exclusionsToAdd);
        }

        private List<ImportListExclusion> DeDupeExclusions(List<ImportListExclusion> exclusions)
        {
            var existingExclusions = _repo.All().Select(x => (x.IgdbId, x.PlatformId)).ToHashSet();

            return exclusions
                .DistinctBy(x => (x.IgdbId, x.PlatformId))
                .Where(x => !existingExclusions.Contains((x.IgdbId, x.PlatformId)))
                .ToList();
        }
    }
}
