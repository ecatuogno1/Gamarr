using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Platforms
{
    public interface IPlatformRepository : IBasicRepository<Platform>
    {
    }

    public class PlatformRepository : BasicRepository<Platform>, IPlatformRepository
    {
        public PlatformRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
