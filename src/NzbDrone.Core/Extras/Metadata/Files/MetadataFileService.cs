using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Games;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Extras.Metadata.Files
{
    public interface IMetadataFileService : IExtraFileService<MetadataFile>
    {
    }

    public class MetadataFileService : ExtraFileService<MetadataFile>, IMetadataFileService
    {
        public MetadataFileService(IExtraFileRepository<MetadataFile> repository, IGameService gameService, IDiskProvider diskProvider, IRecycleBinProvider recycleBinProvider, Logger logger)
            : base(repository, gameService, diskProvider, recycleBinProvider, logger)
        {
        }
    }
}
