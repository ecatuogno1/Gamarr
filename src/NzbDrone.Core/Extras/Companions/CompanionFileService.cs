using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Games;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Extras.Companions
{
    public interface ICompanionFileService : IExtraFileService<CompanionFile>
    {
    }

    public class CompanionFileService : ExtraFileService<CompanionFile>, ICompanionFileService
    {
        public CompanionFileService(IExtraFileRepository<CompanionFile> repository, IGameService gameService, IDiskProvider diskProvider, IRecycleBinProvider recycleBinProvider, Logger logger)
            : base(repository, gameService, diskProvider, recycleBinProvider, logger)
        {
        }
    }
}
