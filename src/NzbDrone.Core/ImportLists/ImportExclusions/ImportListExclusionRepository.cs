using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ImportLists.ImportExclusions
{
    public interface IImportListExclusionRepository : IBasicRepository<ImportListExclusion>
    {
        bool IsGameExcluded(int igdbId, int platformId);
        ImportListExclusion FindByIgdbIdAndPlatform(int igdbId, int platformId);
        List<int> AllExcludedIgdbIds();
    }

    public class ImportListListExclusionRepository : BasicRepository<ImportListExclusion>, IImportListExclusionRepository
    {
        public ImportListListExclusionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool IsGameExcluded(int igdbId, int platformId)
        {
            return Query(x => x.IgdbId == igdbId).Any(x => x.Matches(igdbId, platformId));
        }

        public ImportListExclusion FindByIgdbIdAndPlatform(int igdbId, int platformId)
        {
            return Query(x => x.IgdbId == igdbId && x.PlatformId == platformId).SingleOrDefault();
        }

        public List<int> AllExcludedIgdbIds()
        {
            using var conn = _database.OpenConnection();

            return conn.Query<int>("SELECT \"IgdbId\" FROM \"ImportExclusions\"").ToList();
        }
    }
}
