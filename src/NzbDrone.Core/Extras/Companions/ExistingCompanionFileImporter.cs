using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Games;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Platforms;

namespace NzbDrone.Core.Extras.Companions
{
    // Picks up companion files on a rescan: other game files (tracks, discs, updates) in the game folder,
    // everything in sub folders, and for PC games every other file (installers and installed games).
    public class ExistingCompanionFileImporter : ImportExistingExtraFilesBase<CompanionFile>
    {
        private readonly IExtraFileService<CompanionFile> _companionFileService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IGameReleaseFileService _releaseFileService;
        private readonly Logger _logger;

        public ExistingCompanionFileImporter(IExtraFileService<CompanionFile> companionFileService,
                                             IMediaFileService mediaFileService,
                                             IGameReleaseFileService releaseFileService,
                                             Logger logger)
            : base(companionFileService)
        {
            _companionFileService = companionFileService;
            _mediaFileService = mediaFileService;
            _releaseFileService = releaseFileService;
            _logger = logger;
        }

        // After metadata (so artwork isn't taken for a companion), before other extras
        public override int Order => 1;

        public override IEnumerable<ExtraFile> ProcessFiles(Game game, List<string> filesOnDisk, List<string> importedFiles, string fileNameBeforeRename)
        {
            var filterResult = FilterAndClean(game, filesOnDisk, importedFiles, fileNameBeforeRename is not null);
            var gameFile = _mediaFileService.GetFilesByGame(game.Id).FirstOrDefault();

            if (gameFile == null)
            {
                return filterResult.PreviouslyImported;
            }

            var gameFilePath = Path.Combine(game.Path, gameFile.RelativePath);
            var isPc = game.Platform?.Family == PlatformFamily.PC;
            var companions = new List<CompanionFile>();

            foreach (var file in filterResult.FilesOnDisk)
            {
                if (file.PathEquals(gameFilePath) || _releaseFileService.IsJunkFile(file))
                {
                    continue;
                }

                var relativePath = game.Path.GetRelativePath(file);
                var inSubFolder = relativePath.IndexOfAny(new[] { '/', '\\' }) >= 0;

                if (!isPc && !inSubFolder && !MediaFileExtensions.IsGameFile(Path.GetExtension(file)))
                {
                    continue;
                }

                companions.Add(new CompanionFile
                {
                    GameId = game.Id,
                    GameFileId = gameFile.Id,
                    RelativePath = relativePath,
                    Extension = Path.GetExtension(file)
                });
            }

            _logger.Debug("Found {0} existing companion files", companions.Count);
            _companionFileService.Upsert(companions);

            return companions.Concat(filterResult.PreviouslyImported);
        }
    }
}
