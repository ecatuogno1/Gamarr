using NzbDrone.Core.Datastore;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras.Companions
{
    public interface ICompanionFileRepository : IExtraFileRepository<CompanionFile>
    {
    }

    public class CompanionFileRepository : ExtraFileRepository<CompanionFile>, ICompanionFileRepository
    {
        public CompanionFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
