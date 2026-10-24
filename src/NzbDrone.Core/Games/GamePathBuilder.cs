using System;
using System.IO;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Games
{
    public interface IBuildGamePaths
    {
        string BuildPath(Game game, bool useExistingRelativeFolder);
        string BuildPathForPlatform(Game game, Platform oldPlatform, Platform newPlatform);
    }

    public class GamePathBuilder : IBuildGamePaths
    {
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IRootFolderService _rootFolderService;
        private readonly Logger _logger;

        public GamePathBuilder(IBuildFileNames fileNameBuilder, IRootFolderService rootFolderService, Logger logger)
        {
            _fileNameBuilder = fileNameBuilder;
            _rootFolderService = rootFolderService;
            _logger = logger;
        }

        public string BuildPath(Game game, bool useExistingRelativeFolder)
        {
            if (game.RootFolderPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Root folder was not provided", nameof(game));
            }

            if (useExistingRelativeFolder && game.Path.IsNotNullOrWhiteSpace())
            {
                var relativePath = GetExistingRelativePath(game);
                return Path.Combine(game.RootFolderPath, relativePath);
            }

            return Path.Combine(game.RootFolderPath, _fileNameBuilder.GetGameFolder(game));
        }

        // The game's path after a platform change: in the standard layout ({Root}/{Platform Folder}/{Game Folder}) the
        // platform folder is swapped for the new platform's. Returns null when the game isn't in its platform's folder
        // of a root folder (custom layouts are left alone).
        public string BuildPathForPlatform(Game game, Platform oldPlatform, Platform newPlatform)
        {
            if (game.Path.IsNullOrWhiteSpace() || oldPlatform?.FolderName.IsNullOrWhiteSpace() != false || newPlatform?.FolderName.IsNullOrWhiteSpace() != false)
            {
                return null;
            }

            var rootFolderPath = _rootFolderService.GetBestRootFolderPath(game.Path);

            if (rootFolderPath.IsNullOrWhiteSpace() || !rootFolderPath.IsParentPath(game.Path))
            {
                return null;
            }

            var segments = rootFolderPath.GetRelativePath(game.Path).Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 2 || !segments[0].Equals(oldPlatform.FolderName, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (oldPlatform.FolderName.Equals(newPlatform.FolderName, StringComparison.OrdinalIgnoreCase))
            {
                return game.Path;
            }

            segments[0] = newPlatform.FolderName;

            return Path.Combine(rootFolderPath, Path.Combine(segments));
        }

        private string GetExistingRelativePath(Game game)
        {
            var rootFolderPath = _rootFolderService.GetBestRootFolderPath(game.Path);

            if (rootFolderPath.IsParentPath(game.Path))
            {
                return rootFolderPath.GetRelativePath(game.Path);
            }

            var directoryName = game.Path.GetDirectoryName();

            _logger.Warn("Unable to get relative path for game path {0}, using game folder name {1}", game.Path, directoryName);

            return directoryName;
        }
    }
}
