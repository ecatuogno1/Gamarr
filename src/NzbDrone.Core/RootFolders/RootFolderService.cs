using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Games;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Platforms;

namespace NzbDrone.Core.RootFolders
{
    public interface IRootFolderService
    {
        List<RootFolder> All();
        List<RootFolder> AllWithUnmappedFolders();
        RootFolder Add(RootFolder rootDir);
        void Remove(int id);
        RootFolder Get(int id, bool timeout);
        string GetBestRootFolderPath(string path, List<RootFolder> rootFolders = null);
    }

    public class RootFolderService : IRootFolderService
    {
        private readonly IRootFolderRepository _rootFolderRepository;
        private readonly IDiskProvider _diskProvider;
        private readonly IGameRepository _gameRepository;
        private readonly IConfigService _configService;
        private readonly INamingConfigService _namingConfigService;
        private readonly IPlatformService _platformService;
        private readonly Logger _logger;

        private readonly ICached<string> _cache;

        private static readonly HashSet<string> SpecialFolders = new HashSet<string>
                                                                 {
                                                                     "$recycle.bin",
                                                                     "system volume information",
                                                                     "recycler",
                                                                     "lost+found",
                                                                     ".appledb",
                                                                     ".appledesktop",
                                                                     ".appledouble",
                                                                     "@eadir",
                                                                     ".grab"
                                                                 };

        public RootFolderService(IRootFolderRepository rootFolderRepository,
                                 IDiskProvider diskProvider,
                                 IGameRepository gameRepository,
                                 IConfigService configService,
                                 INamingConfigService namingConfigService,
                                 IPlatformService platformService,
                                 ICacheManager cacheManager,
                                 Logger logger)
        {
            _rootFolderRepository = rootFolderRepository;
            _diskProvider = diskProvider;
            _gameRepository = gameRepository;
            _configService = configService;
            _namingConfigService = namingConfigService;
            _platformService = platformService;
            _logger = logger;

            _cache = cacheManager.GetCache<string>(GetType());
        }

        public List<RootFolder> All()
        {
            var rootFolders = _rootFolderRepository.All().ToList();

            return rootFolders;
        }

        public List<RootFolder> AllWithUnmappedFolders()
        {
            var rootFolders = _rootFolderRepository.All().ToList();

            var gamePaths = _gameRepository.AllGamePaths();

            rootFolders.ForEach(folder =>
            {
                try
                {
                    if (folder.Path.IsPathValid(PathValidationType.CurrentOs))
                    {
                        GetDetails(folder, gamePaths, true);
                    }
                }

                // We don't want an exception to prevent the root folders from loading in the UI, so they can still be deleted
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unable to get free space and unmapped folders for root folder {0}", folder.Path);
                    folder.UnmappedFolders = new List<UnmappedFolder>();
                }
            });

            return rootFolders;
        }

        public RootFolder Add(RootFolder rootFolder)
        {
            var all = All();

            if (string.IsNullOrWhiteSpace(rootFolder.Path) || !Path.IsPathRooted(rootFolder.Path))
            {
                throw new ArgumentException("Invalid path");
            }

            if (!_diskProvider.FolderExists(rootFolder.Path))
            {
                throw new DirectoryNotFoundException("Can't add root directory that doesn't exist.");
            }

            if (all.Exists(r => r.Path.PathEquals(rootFolder.Path)))
            {
                throw new InvalidOperationException("Recent directory already exists.");
            }

            if (!_diskProvider.FolderWritable(rootFolder.Path))
            {
                throw new UnauthorizedAccessException($"Root folder path '{rootFolder.Path}' is not writable by user '{Environment.UserName}'");
            }

            _rootFolderRepository.Insert(rootFolder);

            var gamePaths = _gameRepository.AllGamePaths();

            GetDetails(rootFolder, gamePaths, true);
            _cache.Clear();

            return rootFolder;
        }

        public void Remove(int id)
        {
            _rootFolderRepository.Delete(id);
            _cache.Clear();
        }

        private List<UnmappedFolder> GetUnmappedFolders(string path, Dictionary<int, string> gamePaths)
        {
            _logger.Debug("Generating list of unmapped folders");

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid path provided", nameof(path));
            }

            var results = new List<UnmappedFolder>();

            if (!_diskProvider.FolderExists(path))
            {
                _logger.Debug("Path supplied does not exist: {0}", path);
                return results;
            }

            // Games are usually kept in platform folders ("{Platform Folder}/{Game Title} ({Release Year})"). Folders
            // named like a platform's folder are looked into and their sub folders are games on that platform, other
            // folders are games themselves (a flat library).
            var gameFolderFormat = _namingConfigService.GetConfig().GameFolderFormat ?? string.Empty;
            var subFolderDepth = gameFolderFormat.Count(f => f == '/' || f == '\\');
            var platformLevel = gameFolderFormat.TrimStart().StartsWith("{Platform", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            var possibleGameFolders = new List<(string Path, int PlatformId)>();

            foreach (var folder in _diskProvider.GetDirectories(path))
            {
                var platform = _platformService.FindByFolderName(new DirectoryInfo(folder).Name)?.FirstOrDefault();

                // A platform folder holds the games, other folders are games (or the format's other levels)
                var depth = platform != null ? Math.Max(subFolderDepth, 1) : Math.Max(subFolderDepth - platformLevel, 0);
                IEnumerable<string> gameFolders = new[] { folder };

                for (var i = 0; i < depth; i++)
                {
                    gameFolders = gameFolders.SelectMany(_diskProvider.GetDirectories);
                }

                possibleGameFolders.AddRange(gameFolders.Select(f => (f, platform?.Id ?? 0)));
            }

            var mappedPaths = gamePaths.Select(s => s.Value).ToHashSet(PathEqualityComparer.Instance);
            var unmappedFolders = possibleGameFolders.Where(f => !mappedPaths.Contains(f.Path)).ToList();

            var recycleBinPath = _configService.RecycleBin;

            foreach (var unmappedFolder in unmappedFolders)
            {
                var di = new DirectoryInfo(unmappedFolder.Path.Normalize());

                if ((!di.Attributes.HasFlag(FileAttributes.System) && !di.Attributes.HasFlag(FileAttributes.Hidden)) || di.Attributes.ToString() == "-1")
                {
                    if (string.IsNullOrWhiteSpace(recycleBinPath) || di.FullName.PathNotEquals(recycleBinPath))
                    {
                        results.Add(new UnmappedFolder
                        {
                            Name = di.Name,
                            Path = di.FullName,
                            RelativePath = path.GetRelativePath(di.FullName),
                            PlatformId = unmappedFolder.PlatformId
                        });
                    }
                }
            }

            var setToRemove = SpecialFolders;
            results.RemoveAll(x => setToRemove.Contains(new DirectoryInfo(x.Path.ToLowerInvariant()).Name));

            _logger.Debug("{0} unmapped folders detected.", results.Count);
            return results.OrderBy(u => u.Name, StringComparer.InvariantCultureIgnoreCase).ToList();
        }

        public RootFolder Get(int id, bool timeout)
        {
            var rootFolder = _rootFolderRepository.Get(id);
            var gamePaths = _gameRepository.AllGamePaths();

            GetDetails(rootFolder, gamePaths, timeout);

            return rootFolder;
        }

        public string GetBestRootFolderPath(string path, List<RootFolder> rootFolders = null)
        {
            return _cache.Get(path, () => GetBestRootFolderPathInternal(path, rootFolders), TimeSpan.FromDays(1));
        }

        private void GetDetails(RootFolder rootFolder, Dictionary<int, string> gamePaths, bool timeout)
        {
            Task.Run(() =>
            {
                if (_diskProvider.FolderExists(rootFolder.Path))
                {
                    rootFolder.Accessible = true;
                    rootFolder.FreeSpace = _diskProvider.GetAvailableSpace(rootFolder.Path);
                    rootFolder.TotalSpace = _diskProvider.GetTotalSize(rootFolder.Path);
                    rootFolder.UnmappedFolders = GetUnmappedFolders(rootFolder.Path, gamePaths);
                }
            }).Wait(timeout ? 5000 : -1);
        }

        private string GetBestRootFolderPathInternal(string path, List<RootFolder> rootFolders = null)
        {
            var allRootFoldersToConsider = rootFolders ?? All();

            var possibleRootFolder = allRootFoldersToConsider.Where(r => r.Path.IsParentPath(path)).MaxBy(r => r.Path.Length);

            if (possibleRootFolder == null)
            {
                var osPath = new OsPath(path);

                return osPath.Directory.ToString().GetCleanPath();
            }

            return possibleRootFolder.Path.GetCleanPath();
        }
    }
}
