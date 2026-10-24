using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Games;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Extras.Companions
{
    // Imports the companion files of a multi-file release next to its game file. Companions keep their names
    // and their path relative to the primary file (or the release root), so cue sheets, m3u playlists and
    // installers keep finding them. Only the primary file is renamed.
    public class CompanionFileManager : ExtraFileManager<CompanionFile>
    {
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly ICompanionFileService _companionFileService;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly Logger _logger;

        public CompanionFileManager(IConfigService configService,
                                    IDiskProvider diskProvider,
                                    IDiskTransferService diskTransferService,
                                    ICompanionFileService companionFileService,
                                    IMediaFileAttributeService mediaFileAttributeService,
                                    Logger logger)
            : base(configService, diskProvider, diskTransferService, logger)
        {
            _configService = configService;
            _diskProvider = diskProvider;
            _diskTransferService = diskTransferService;
            _companionFileService = companionFileService;
            _mediaFileAttributeService = mediaFileAttributeService;
            _logger = logger;
        }

        public override int Order => -1;

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

        // Companions aren't renamed, but they move along if the game file moved to another folder
        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Game game, List<GameFile> gameFiles)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override bool CanImportFile(LocalGame localGame, GameFile gameFile, string path, string extension, bool readOnly)
        {
            return localGame.CompanionFiles.Any(c => c.PathEquals(path));
        }

        public override IEnumerable<ExtraFile> ImportFiles(LocalGame localGame, GameFile gameFile, List<string> files, bool isReadOnly)
        {
            var game = localGame.Game;
            var imported = new List<CompanionFile>();

            var sourceFolder = Path.GetDirectoryName(localGame.Path);
            var releaseRoot = localGame.ReleaseRoot.IsNotNullOrWhiteSpace() ? localGame.ReleaseRoot : sourceFolder;
            var destinationFolder = Path.GetDirectoryName(Path.Combine(game.Path, gameFile.RelativePath));

            var transferMode = TransferMode.Move;

            if (isReadOnly)
            {
                transferMode = _configService.CopyUsingHardlinks ? TransferMode.HardLinkOrCopy : TransferMode.Copy;
            }

            foreach (var file in files.Distinct(PathEqualityComparer.Instance))
            {
                try
                {
                    var relativePath = sourceFolder.IsParentPath(file)
                        ? sourceFolder.GetRelativePath(file)
                        : releaseRoot.IsParentPath(file) ? releaseRoot.GetRelativePath(file) : Path.GetFileName(file);

                    var destination = Path.Combine(destinationFolder, relativePath);

                    if (destination.PathEquals(Path.Combine(game.Path, gameFile.RelativePath)))
                    {
                        _logger.Debug("Companion file {0} not imported, it would replace the game file", file);
                        continue;
                    }

                    _diskProvider.CreateFolder(Path.GetDirectoryName(destination));
                    _diskTransferService.TransferFile(file, destination, transferMode, true);
                    _mediaFileAttributeService.SetFilePermissions(destination);

                    imported.Add(new CompanionFile
                    {
                        GameId = game.Id,
                        GameFileId = gameFile.Id,
                        RelativePath = game.Path.GetRelativePath(destination),
                        Extension = Path.GetExtension(destination)
                    });
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to import companion file: {0}", file);
                }
            }

            _companionFileService.Upsert(imported);

            _logger.Debug("Imported {0} companion files for {1}", imported.Count, gameFile);

            return imported;
        }
    }
}
