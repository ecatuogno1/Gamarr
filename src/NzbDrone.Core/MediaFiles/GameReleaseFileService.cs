using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles
{
    public class GameReleaseFiles
    {
        public GameReleaseFiles()
        {
            CompanionFiles = new List<string>();
        }

        // The folder the release's relative structure is kept from
        public string Root { get; set; }

        // The file that represents the release (the GameFile)
        public string PrimaryFile { get; set; }

        // Everything else that belongs to the release
        public List<string> CompanionFiles { get; set; }

        public bool IsInstaller { get; set; }

        public override string ToString()
        {
            return CompanionFiles.Any() ? $"{PrimaryFile} (+{CompanionFiles.Count} files)" : PrimaryFile;
        }
    }

    public interface IGameReleaseFileService
    {
        // All game files form one release (a download or a game folder)
        GameReleaseFiles GetRelease(List<string> gameFiles);

        // Files of several releases (a folder of loose ROMs/discs), grouped by cue/m3u/gdi references and disc names
        List<GameReleaseFiles> GroupReleases(List<string> gameFiles);

        // The companions of a primary file when only its path is known (manual import)
        List<string> FindCompanions(string primaryFile);

        bool IsJunkFile(string path);
    }

    // A release can be several files: a cue sheet and its bin tracks, a multi-disc set with an m3u playlist,
    // a PC installer with its data files or a whole installed folder, a Switch base game with its update and DLC.
    // The primary file is picked in this order: .m3u > .cue > .gdi > .ccd/.mds/.cdi > single image/ROM/package/
    // archive > installer executable > largest file. All other files of the release are its companions.
    public class GameReleaseFileService : IGameReleaseFileService
    {
        private static readonly Regex InstallerNameRegex = new(@"^(?:setup|install(?:er)?|autorun)(?:[_. -].*)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex DiscTokenRegex = new(@"[ ._-]*[(\[]?(?:Disc|Disk|CD)[ ._-]?(?<disc>\d{1,2})(?:[ ._-]?of[ ._-]?\d{1,2})?[)\]]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex CueFileRegex = new(@"^\s*FILE\s+(?:""(?<file>[^""]+)""|(?<file>\S+))", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static readonly Regex GdiTrackRegex = new(@"^\s*\d+\s+\d+\s+\d+\s+\d+\s+(?:""(?<file>[^""]+)""|(?<file>\S+))", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex JunkFileRegex = new(@"^(?:\._.*|\.DS_Store|Thumbs\.db|desktop\.ini|.*\.(?:par2|nzb|url|lnk|torrent|!qb|part|tmp|srr|srs))$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly HashSet<string> SheetExtensions = new(StringComparer.OrdinalIgnoreCase) { ".m3u", ".cue", ".gdi", ".ccd", ".mds", ".cdi" };
        private static readonly HashSet<string> InstallerExtensions = new(StringComparer.OrdinalIgnoreCase) { ".exe", ".msi", ".sh", ".dmg" };

        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public GameReleaseFileService(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public GameReleaseFiles GetRelease(List<string> gameFiles)
        {
            var files = gameFiles.Where(f => !IsJunkFile(f)).Distinct(PathEqualityComparer.Instance).ToList();

            if (!files.Any())
            {
                return null;
            }

            var primary = SelectPrimary(files);
            var release = new GameReleaseFiles
            {
                PrimaryFile = primary,
                Root = GetCommonRoot(files)
            };

            if (IsInstaller(primary))
            {
                // An installer (or a portable/installed game) needs its whole folder
                release.IsInstaller = true;
                release.Root = Path.GetDirectoryName(primary);

                var folderFiles = GetFiles(release.Root).Where(f => !IsJunkFile(f));

                release.CompanionFiles = folderFiles
                    .Concat(files)
                    .Where(f => !f.PathEquals(primary))
                    .Distinct(PathEqualityComparer.Instance)
                    .ToList();
            }
            else
            {
                release.CompanionFiles = files
                    .Concat(files.SelectMany(GetReferencedFiles))
                    .Where(f => !f.PathEquals(primary))
                    .Distinct(PathEqualityComparer.Instance)
                    .ToList();
            }

            _logger.Debug("Release files: {0}", release);

            return release;
        }

        public List<GameReleaseFiles> GroupReleases(List<string> gameFiles)
        {
            var files = gameFiles.Where(f => !IsJunkFile(f)).Distinct(PathEqualityComparer.Instance).ToList();

            if (!files.Any())
            {
                return new List<GameReleaseFiles>();
            }

            // A folder with an installer is one release
            var installer = files.FirstOrDefault(f => IsInstaller(f) && InstallerNameRegex.IsMatch(Path.GetFileNameWithoutExtension(f)));

            if (installer != null)
            {
                return new List<GameReleaseFiles> { GetRelease(files) };
            }

            // Union files that reference each other or are discs of the same set
            var parent = files.ToDictionary(f => f, f => f, PathEqualityComparer.Instance);

            string Find(string file)
            {
                while (!parent[file].PathEquals(file))
                {
                    file = parent[file];
                }

                return file;
            }

            void Union(string a, string b)
            {
                var rootA = Find(a);
                var rootB = Find(b);

                if (!rootA.PathEquals(rootB))
                {
                    parent[rootB] = rootA;
                }
            }

            foreach (var file in files.Where(f => SheetExtensions.Contains(Path.GetExtension(f))))
            {
                foreach (var referenced in GetReferencedFiles(file).Where(r => parent.ContainsKey(r)))
                {
                    Union(file, referenced);
                }
            }

            foreach (var discSet in files.Where(f => DiscTokenRegex.IsMatch(Path.GetFileNameWithoutExtension(f)))
                                         .GroupBy(f => Path.Combine(Path.GetDirectoryName(f) ?? string.Empty, DiscTokenRegex.Replace(Path.GetFileNameWithoutExtension(f), string.Empty).Trim()), PathEqualityComparer.Instance))
            {
                var first = discSet.First();

                foreach (var file in discSet.Skip(1))
                {
                    Union(first, file);
                }
            }

            return files.GroupBy(Find, PathEqualityComparer.Instance)
                .Select(g => GetRelease(g.ToList()))
                .Where(r => r != null)
                .ToList();
        }

        public List<string> FindCompanions(string primaryFile)
        {
            var folder = Path.GetDirectoryName(primaryFile);

            if (folder == null || !_diskProvider.FolderExists(folder))
            {
                return new List<string>();
            }

            if (IsInstaller(primaryFile))
            {
                return GetFiles(folder).Where(f => !f.PathEquals(primaryFile) && !IsJunkFile(f)).ToList();
            }

            var gameFiles = _diskProvider.GetFiles(folder, false)
                .Where(f => MediaFileExtensions.IsGameFile(Path.GetExtension(f)))
                .ToList();

            var release = GroupReleases(gameFiles).FirstOrDefault(r => r.PrimaryFile.PathEquals(primaryFile) || r.CompanionFiles.Any(c => c.PathEquals(primaryFile)));

            if (release == null)
            {
                return GetReferencedFiles(primaryFile);
            }

            return release.CompanionFiles.Concat(new[] { release.PrimaryFile })
                .Where(f => !f.PathEquals(primaryFile))
                .ToList();
        }

        public bool IsJunkFile(string path)
        {
            return JunkFileRegex.IsMatch(Path.GetFileName(path));
        }

        public static bool IsInstaller(string path)
        {
            return InstallerExtensions.Contains(Path.GetExtension(path));
        }

        private string SelectPrimary(List<string> files)
        {
            return files
                .OrderBy(f => GetPrimaryClass(f, files))
                .ThenBy(f => GetReleaseTypeOrder(f))
                .ThenBy(f => GetDiscNumber(f))
                .ThenByDescending(f => InstallerNameRegex.IsMatch(Path.GetFileNameWithoutExtension(f)))
                .ThenBy(f => f.Count(c => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar))
                .ThenByDescending(GetFileSize)
                .ThenBy(f => f, StringComparer.OrdinalIgnoreCase)
                .First();
        }

        private int GetPrimaryClass(string file, List<string> files)
        {
            var extension = Path.GetExtension(file);

            if (SheetExtensions.Contains(extension))
            {
                // A sheet that is referenced by another sheet (a cue listed in an m3u) isn't the primary
                return MediaFileExtensions.GetPrimaryPriority(extension);
            }

            if (MediaFileExtensions.IsCompanionOnly(extension))
            {
                return 30;
            }

            if (IsInstaller(file))
            {
                return 20;
            }

            if (MediaFileExtensions.IsGameFile(extension))
            {
                return 10;
            }

            return 40;
        }

        private static int GetReleaseTypeOrder(string file)
        {
            var parsed = Parser.Parser.ParseGameTitle(Path.GetFileName(file));

            return parsed?.ReleaseType switch
            {
                null or ReleaseType.Base or ReleaseType.Bundle => 0,
                ReleaseType.Update => 2,
                ReleaseType.Dlc => 3,
                _ => 1
            };
        }

        private static int GetDiscNumber(string file)
        {
            var match = DiscTokenRegex.Match(Path.GetFileNameWithoutExtension(file));

            return match.Success ? int.Parse(match.Groups["disc"].Value) : 0;
        }

        private long GetFileSize(string file)
        {
            try
            {
                return _diskProvider.FileExists(file) ? _diskProvider.GetFileSize(file) : 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private List<string> GetReferencedFiles(string sheet)
        {
            var extension = Path.GetExtension(sheet);

            if (!SheetExtensions.Contains(extension))
            {
                return new List<string>();
            }

            var folder = Path.GetDirectoryName(sheet) ?? string.Empty;
            var referenced = new List<string>();

            try
            {
                if (extension.Equals(".ccd", StringComparison.OrdinalIgnoreCase) || extension.Equals(".mds", StringComparison.OrdinalIgnoreCase))
                {
                    // CloneCD/Alcohol images: same name, other extensions (.img/.sub, .mdf)
                    var baseName = Path.GetFileNameWithoutExtension(sheet);

                    referenced.AddRange(new[] { ".img", ".sub", ".mdf" }
                        .Select(e => Path.Combine(folder, baseName + e))
                        .Where(f => _diskProvider.FileExists(f)));

                    return referenced;
                }

                if (!_diskProvider.FileExists(sheet))
                {
                    return referenced;
                }

                var content = _diskProvider.ReadAllText(sheet);

                var names = extension.ToLowerInvariant() switch
                {
                    ".cue" => CueFileRegex.Matches(content).Select(m => m.Groups["file"].Value).ToList(),
                    ".gdi" => GdiTrackRegex.Matches(content).Select(m => m.Groups["file"].Value).ToList(),
                    ".m3u" => content.Split('\n').Select(l => l.Trim()).Where(l => l.IsNotNullOrWhiteSpace() && !l.StartsWith('#')).ToList(),
                    _ => new List<string>()
                };

                foreach (var name in names)
                {
                    var path = Path.GetFullPath(Path.Combine(folder, name.Replace('\\', Path.DirectorySeparatorChar)));

                    if (_diskProvider.FileExists(path))
                    {
                        referenced.Add(path);

                        // An m3u lists cue sheets, which list their tracks
                        if (!path.PathEquals(sheet))
                        {
                            referenced.AddRange(GetReferencedFiles(path));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Unable to read {0}", sheet);
            }

            return referenced;
        }

        private IEnumerable<string> GetFiles(string folder)
        {
            try
            {
                return _diskProvider.GetFiles(folder, true);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Unable to list files in {0}", folder);
                return Enumerable.Empty<string>();
            }
        }

        private static string GetCommonRoot(List<string> files)
        {
            var folders = files.Select(f => Path.GetDirectoryName(f) ?? string.Empty).Distinct(PathEqualityComparer.Instance).ToList();
            var root = folders.First();

            while (root.IsNotNullOrWhiteSpace() && folders.Any(f => !f.PathEquals(root) && !root.IsParentPath(f)))
            {
                root = Path.GetDirectoryName(root);
            }

            return root;
        }
    }
}
