using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Games;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Extras.Others
{
    public class OtherExtraService : ExtraFileManager<OtherExtraFile>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IOtherExtraFileService _otherExtraFileService;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly Logger _logger;

        public OtherExtraService(IConfigService configService,
                                 IDiskProvider diskProvider,
                                 IDiskTransferService diskTransferService,
                                 IOtherExtraFileService otherExtraFileService,
                                 IMediaFileAttributeService mediaFileAttributeService,
                                 Logger logger)
            : base(configService, diskProvider, diskTransferService, logger)
        {
            _diskProvider = diskProvider;
            _otherExtraFileService = otherExtraFileService;
            _mediaFileAttributeService = mediaFileAttributeService;
            _logger = logger;
        }

        public override int Order => 2;

        public override IEnumerable<ExtraFile> CreateAfterMediaCoverUpdate(Game game)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterGameScan(Game game, List<GameFile> gameFiles)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterGameImport(Game game, GameFile gameFile)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterGameFolder(Game game, string gameFolder)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Game game, List<GameFile> gameFiles)
        {
            var extraFiles = _otherExtraFileService.GetFilesByGame(game.Id);
            var movedFiles = new List<OtherExtraFile>();

            foreach (var gameFile in gameFiles)
            {
                var extraFilesForGameFile = extraFiles.Where(m => m.GameFileId == gameFile.Id).ToList();

                foreach (var extraFile in extraFilesForGameFile)
                {
                    movedFiles.AddIfNotNull(MoveFile(game, gameFile, extraFile));
                }
            }

            _otherExtraFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override bool CanImportFile(LocalGame localGame, GameFile gameFile, string path, string extension, bool readOnly)
        {
            return true;
        }

        private static bool MatchesGame(string file, LocalGame localGame)
        {
            var gameTitles = (localGame.FileGameInfo?.GameTitles ?? new List<string>())
                .Concat(new[] { localGame.Game?.Title })
                .Where(t => t.IsNotNullOrWhiteSpace())
                .Select(t => t.CleanGameTitle())
                .ToHashSet();

            var names = new[]
            {
                Path.GetFileNameWithoutExtension(file),
                Path.GetFileName(Path.GetDirectoryName(file))
            };

            return names.Where(n => n.IsNotNullOrWhiteSpace())
                .Select(n => Parser.Parser.ParseGameTitle(n))
                .Where(p => p != null)
                .SelectMany(p => p.GameTitles)
                .Any(t => gameTitles.Contains(t.CleanGameTitle()));
        }

        public override IEnumerable<ExtraFile> ImportFiles(LocalGame localGame, GameFile gameFile, List<string> files, bool isReadOnly)
        {
            var importedFiles = new List<ExtraFile>();
            var filteredFiles = files.Where(f => CanImportFile(localGame, gameFile, f, Path.GetExtension(f), isReadOnly)).ToList();
            var sourcePath = localGame.Path;
            var sourceFolder = _diskProvider.GetParentFolder(sourcePath);
            var sourceFileName = Path.GetFileNameWithoutExtension(sourcePath);
            var matchingFiles = new List<string>();
            var hasNfo = false;

            foreach (var file in filteredFiles)
            {
                try
                {
                    // Filter out duplicate NFO files
                    if (file.EndsWith(".nfo", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (hasNfo)
                        {
                            continue;
                        }

                        hasNfo = true;
                    }

                    // Filename match
                    if (Path.GetFileNameWithoutExtension(file).StartsWith(sourceFileName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        matchingFiles.Add(file);
                        continue;
                    }

                    // Game match, by the file's name or its folder's name
                    if (MatchesGame(file, localGame))
                    {
                        matchingFiles.Add(file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to import extra file: {0}", file);
                }
            }

            foreach (var file in matchingFiles)
            {
                try
                {
                    var extraFile = ImportFile(localGame.Game, gameFile, file, isReadOnly, Path.GetExtension(file), null);
                    _mediaFileAttributeService.SetFilePermissions(file);
                    _otherExtraFileService.Upsert(extraFile);
                    importedFiles.Add(extraFile);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to import extra file: {0}", file);
                }
            }

            return importedFiles;
        }
    }
}
