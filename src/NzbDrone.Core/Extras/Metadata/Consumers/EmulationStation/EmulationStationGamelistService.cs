using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.Events;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras.Metadata.Consumers.EmulationStation
{
    public interface IEmulationStationGamelistService
    {
        void UpdateGame(Game game, GameFile gameFile, List<ImageFileResult> images, EmulationStationMetadataSettings settings = null);
        void RemoveGame(Game game, EmulationStationMetadataSettings settings = null);
        void MoveGame(Game game, string sourcePath, string destinationPath, EmulationStationMetadataSettings settings = null);
        string GetGamelistPath(string gamePath, EmulationStationMetadataSettings settings = null);
    }

    // Maintains the gamelist.xml of a system (platform folder). Entries of other games (added by ES-DE scrapers or by
    // hand) are kept, an entry of a Gamarr game is found by its IGDB id or by a path inside the game's folder. Fields
    // Gamarr doesn't write (favorite, playcount, lastplayed, hidden…) are kept when an entry is updated.
    //
    // Two locations (see ES-DE's INSTALL.md "gamelist.xml" and USERGUIDE.md "Manually copying game media files"):
    // - ROM folder: <platform folder>/gamelist.xml (RetroPie, Batocera, ES-DE with LegacyGamelistFileLocation), images
    //   in <game folder>/media referenced by <image>/<thumbnail>/<fanart>.
    // - ES-DE: <gamelists directory>/<system>/gamelist.xml, the system being the name of the platform folder. ES-DE
    //   ignores image tags, it finds media by the ROM path: <media directory>/<system>/<covers|screenshots|fanart>/
    //   <folders below the system folder>/<ROM file name without extension>.<ext>.
    // In both cases <path> is relative to the system's ROM folder (the parent of the game folder) and starts with "./".
    public class EmulationStationGamelistService : IEmulationStationGamelistService, IHandle<GamesDeletedEvent>, IHandle<GameMovedEvent>
    {
        public const string GamelistFileName = "gamelist.xml";
        public const string IdSource = "IGDB";

        private static readonly object Lock = new();

        private static readonly string[] ManagedElements =
        {
            "path", "name", "desc", "rating", "releasedate", "developer", "publisher", "genre", "players", "image", "thumbnail", "fanart"
        };

        private static readonly string[] PathElements = { "path", "image", "thumbnail", "fanart", "video", "marquee", "manual" };

        // Gamarr image name -> ES-DE downloaded_media folder
        private static readonly Dictionary<string, string> EsDeMediaFolders = new()
        {
            { "cover", "covers" },
            { "screenshot", "screenshots" },
            { "fanart", "fanart" }
        };

        private readonly IDiskProvider _diskProvider;
        private readonly IMetadataRepository _metadataRepository;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public EmulationStationGamelistService(IDiskProvider diskProvider,
                                               IMetadataRepository metadataRepository,
                                               IMediaFileAttributeService mediaFileAttributeService,
                                               IHttpClient httpClient,
                                               Logger logger)
        {
            _diskProvider = diskProvider;
            _metadataRepository = metadataRepository;
            _mediaFileAttributeService = mediaFileAttributeService;
            _httpClient = httpClient;
            _logger = logger;
        }

        public string GetGamelistPath(string gamePath, EmulationStationMetadataSettings settings = null)
        {
            settings ??= new EmulationStationMetadataSettings();

            if (!settings.UseEsDeDirectories)
            {
                return GetRomFolderGamelistPath(gamePath);
            }

            var system = GetSystemName(gamePath);
            var directory = EmulationStationMetadataSettings.ExpandHome(settings.GamelistDirectory);

            return system.IsNullOrWhiteSpace() || directory.IsNullOrWhiteSpace() ? null : Path.Combine(directory, system, GamelistFileName);
        }

        public void UpdateGame(Game game, GameFile gameFile, List<ImageFileResult> images, EmulationStationMetadataSettings settings = null)
        {
            settings ??= new EmulationStationMetadataSettings();
            images ??= new List<ImageFileResult>();

            var gamelistPath = GetGamelistPath(game.Path, settings);

            if (gamelistPath == null || gameFile == null)
            {
                return;
            }

            string previousPath = null;

            Modify(gamelistPath, gameList =>
            {
                var existing = FindEntries(gameList, game.IgdbId, GetGameFolderName(game.Path)).ToList();
                var entry = existing.FirstOrDefault() ?? new XElement("game");

                previousPath = NormalizePath((string)entry.Element("path"));

                foreach (var duplicate in existing.Skip(1))
                {
                    duplicate.Remove();
                }

                // ES-DE doesn't read image tags (and drops them when it saves the entry), its media is matched by file name
                UpdateEntry(entry, game, gameFile, settings.UseEsDeDirectories ? new List<ImageFileResult>() : images);

                if (entry.Parent == null)
                {
                    gameList.Add(entry);
                }
            });

            if (settings.UseEsDeDirectories && settings.GameImages)
            {
                SaveEsDeMedia(game, gameFile, previousPath, images, settings);
            }
        }

        public void RemoveGame(Game game, EmulationStationMetadataSettings settings = null)
        {
            settings ??= new EmulationStationMetadataSettings();

            var gamelistPath = GetGamelistPath(game.Path, settings);

            if (gamelistPath == null || !_diskProvider.FileExists(gamelistPath))
            {
                return;
            }

            var removedPaths = new List<string>();

            Modify(gamelistPath, gameList =>
            {
                foreach (var entry in FindEntries(gameList, game.IgdbId, GetGameFolderName(game.Path)).ToList())
                {
                    removedPaths.AddIfNotNull(NormalizePath((string)entry.Element("path")));
                    entry.Remove();
                }
            });

            if (settings.UseEsDeDirectories)
            {
                foreach (var romPath in removedPaths)
                {
                    foreach (var mediaFile in GetEsDeMediaFiles(game.Path, romPath, settings).Values.SelectMany(f => f))
                    {
                        if (_diskProvider.FileExists(mediaFile))
                        {
                            _diskProvider.DeleteFile(mediaFile);
                        }
                    }
                }
            }
        }

        // Moves the game's entries (with their user fields) to the gamelist of the new location and fixes their paths.
        // A platform change moves the game to another platform folder, so another system.
        public void MoveGame(Game game, string sourcePath, string destinationPath, EmulationStationMetadataSettings settings = null)
        {
            settings ??= new EmulationStationMetadataSettings();

            var sourceGamelist = GetGamelistPath(sourcePath, settings);
            var destinationGamelist = GetGamelistPath(destinationPath, settings);
            var sourceFolder = GetGameFolderName(sourcePath);
            var destinationFolder = GetGameFolderName(destinationPath);

            if (sourceGamelist == null || destinationGamelist == null || !_diskProvider.FileExists(sourceGamelist))
            {
                return;
            }

            if (sourceGamelist.PathEquals(destinationGamelist) && sourceFolder == destinationFolder)
            {
                return;
            }

            var moved = new List<XElement>();

            Modify(sourceGamelist, gameList =>
            {
                foreach (var entry in FindEntries(gameList, game.IgdbId, sourceFolder).ToList())
                {
                    entry.Remove();
                    moved.Add(entry);
                }
            });

            if (!moved.Any())
            {
                return;
            }

            var sourcePrefix = "./" + sourceFolder + "/";
            var destinationPrefix = "./" + destinationFolder + "/";
            var romPaths = new List<string>();

            foreach (var element in moved.SelectMany(e => e.Elements()).Where(e => PathElements.Contains(e.Name.LocalName)))
            {
                var value = NormalizePath(element.Value);

                if (value != null && value.StartsWith(sourcePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    element.Value = string.Concat(destinationPrefix, value.AsSpan(sourcePrefix.Length));
                }
            }

            romPaths.AddRange(moved.Select(e => NormalizePath((string)e.Element("path"))).Where(p => p != null && p.StartsWith(destinationPrefix, StringComparison.OrdinalIgnoreCase)));

            _logger.Debug("Moving EmulationStation game list entries of {0} from {1} to {2}", game, sourceGamelist, destinationGamelist);

            Modify(destinationGamelist, gameList =>
            {
                FindEntries(gameList, game.IgdbId, destinationFolder).ToList().ForEach(e => e.Remove());
                gameList.Add(moved);
            });

            if (settings.UseEsDeDirectories)
            {
                foreach (var romPath in romPaths)
                {
                    var sourceRomPath = string.Concat(sourcePrefix, romPath.AsSpan(destinationPrefix.Length));
                    var sourceMedia = GetEsDeMediaFiles(sourcePath, sourceRomPath, settings);
                    var destinationMedia = GetEsDeMediaFiles(destinationPath, romPath, settings);

                    foreach (var (name, files) in sourceMedia)
                    {
                        MoveMediaFiles(files, destinationMedia[name]);
                    }
                }
            }
        }

        public void Handle(GamesDeletedEvent message)
        {
            if (!message.DeleteFiles)
            {
                return;
            }

            foreach (var settings in GetEnabledSettings())
            {
                foreach (var game in message.Games.Where(g => g.Path.IsNotNullOrWhiteSpace()))
                {
                    try
                    {
                        RemoveGame(game, settings);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Unable to remove {0} from {1}", game, GetGamelistPath(game.Path, settings));
                    }
                }
            }
        }

        public void Handle(GameMovedEvent message)
        {
            foreach (var settings in GetEnabledSettings())
            {
                try
                {
                    MoveGame(message.Game, message.SourcePath, message.DestinationPath, settings);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to move the EmulationStation game list entry of {0}", message.Game);
                }
            }
        }

        private IEnumerable<EmulationStationMetadataSettings> GetEnabledSettings()
        {
            return _metadataRepository.All()
                .Where(d => d.Enable && d.Implementation == nameof(EmulationStationMetadata))
                .Select(d => d.Settings as EmulationStationMetadataSettings)
                .Where(s => s is { Gamelist: true });
        }

        private static string GetRomFolderGamelistPath(string gamePath)
        {
            var platformFolder = GetSystemFolder(gamePath);

            return platformFolder.IsNullOrWhiteSpace() ? null : Path.Combine(platformFolder, GamelistFileName);
        }

        private static string GetSystemFolder(string gamePath)
        {
            return gamePath.IsNullOrWhiteSpace() ? null : Path.GetDirectoryName(gamePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        // ES-DE system name, the platform folder's name (Gamarr's folder names follow ES-DE's system names)
        private static string GetSystemName(string gamePath)
        {
            var systemFolder = GetSystemFolder(gamePath);

            return systemFolder.IsNullOrWhiteSpace() ? null : Path.GetFileName(systemFolder);
        }

        // <media directory>/<system>/<covers|screenshots|fanart>/<folders of the ROM below the system folder>/<ROM name
        // without extension>.<ext> for every extension ES-DE reads, keyed by Gamarr image name
        private static Dictionary<string, List<string>> GetEsDeMediaFiles(string gamePath, string romPath, EmulationStationMetadataSettings settings)
        {
            var mediaDirectory = EmulationStationMetadataSettings.ExpandHome(settings.MediaDirectory);
            var system = GetSystemName(gamePath);
            var result = new Dictionary<string, List<string>>();

            if (mediaDirectory.IsNullOrWhiteSpace() || system.IsNullOrWhiteSpace() || romPath.IsNullOrWhiteSpace())
            {
                return EsDeMediaFolders.Keys.ToDictionary(k => k, _ => new List<string>());
            }

            var relative = romPath.Substring(2).Split('/', StringSplitOptions.RemoveEmptyEntries);
            var stem = Path.GetFileNameWithoutExtension(relative.Last());
            var folders = relative.SkipLast(1).ToArray();

            foreach (var (name, folder) in EsDeMediaFolders)
            {
                var directory = Path.Combine(new[] { mediaDirectory, system, folder }.Concat(folders).ToArray());

                result[name] = new[] { ".jpg", ".png", ".webp" }.Select(e => Path.Combine(directory, stem + e)).ToList();
            }

            return result;
        }

        private void SaveEsDeMedia(Game game, GameFile gameFile, string previousRomPath, List<ImageFileResult> images, EmulationStationMetadataSettings settings)
        {
            var romPath = "./" + GetGameFolderName(game.Path) + "/" + gameFile.RelativePath.Replace('\\', '/');
            var mediaFiles = GetEsDeMediaFiles(game.Path, romPath, settings);

            // The game file was renamed, keep its media (also media scraped by ES-DE)
            if (previousRomPath != null && !previousRomPath.Equals(romPath, StringComparison.Ordinal))
            {
                foreach (var (name, files) in GetEsDeMediaFiles(game.Path, previousRomPath, settings))
                {
                    MoveMediaFiles(files, mediaFiles[name]);
                }
            }

            foreach (var image in images)
            {
                var name = Path.GetFileNameWithoutExtension(image.RelativePath);

                if (!mediaFiles.TryGetValue(name, out var candidates) || candidates.Any(_diskProvider.FileExists))
                {
                    continue;
                }

                var extension = Path.GetExtension(image.RelativePath).ToLowerInvariant();
                var path = candidates.FirstOrDefault(c => Path.GetExtension(c) == (extension == ".jpeg" ? ".jpg" : extension)) ?? candidates.First();

                try
                {
                    _diskProvider.CreateFolder(Path.GetDirectoryName(path));
                    _httpClient.DownloadFile(image.Url, path);
                    _mediaFileAttributeService.SetFilePermissions(path);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Couldn't download image {0} for {1} to {2}", image.Url, game, path);
                }
            }
        }

        private void MoveMediaFiles(List<string> sources, List<string> destinations)
        {
            foreach (var source in sources)
            {
                var destination = destinations.FirstOrDefault(d => Path.GetExtension(d) == Path.GetExtension(source));

                if (destination == null || source.PathEquals(destination) || !_diskProvider.FileExists(source) || _diskProvider.FileExists(destination))
                {
                    continue;
                }

                _diskProvider.CreateFolder(Path.GetDirectoryName(destination));
                _diskProvider.MoveFile(source, destination);
            }
        }

        private void Modify(string gamelistPath, Action<XElement> modify)
        {
            lock (Lock)
            {
                XDocument document;
                string existingContents = null;

                if (_diskProvider.FileExists(gamelistPath))
                {
                    existingContents = _diskProvider.ReadAllText(gamelistPath);
                }

                if (existingContents.IsNullOrWhiteSpace())
                {
                    document = new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement("gameList"));
                }
                else
                {
                    try
                    {
                        document = XDocument.Parse(existingContents, LoadOptions.PreserveWhitespace);
                    }
                    catch (XmlException ex)
                    {
                        _logger.Warn(ex, "{0} isn't valid XML, not updating it", gamelistPath);
                        return;
                    }

                    if (document.Root == null || document.Root.Name.LocalName != "gameList")
                    {
                        _logger.Warn("{0} isn't an EmulationStation game list, not updating it", gamelistPath);
                        return;
                    }
                }

                modify(document.Root);

                var contents = Serialize(document);

                if (contents == existingContents)
                {
                    return;
                }

                var directory = Path.GetDirectoryName(gamelistPath);

                if (directory.IsNotNullOrWhiteSpace() && !_diskProvider.FolderExists(directory))
                {
                    _diskProvider.CreateFolder(directory);
                }

                _logger.Debug("Writing EmulationStation game list: {0}", gamelistPath);
                _diskProvider.WriteAllText(gamelistPath, contents);
                _mediaFileAttributeService.SetFilePermissions(gamelistPath);
            }
        }

        private static IEnumerable<XElement> FindEntries(XElement gameList, int igdbId, string gameFolderName)
        {
            var folderPrefix = "./" + gameFolderName + "/";
            var id = igdbId.ToString(CultureInfo.InvariantCulture);

            return gameList.Elements("game").Where(e =>
            {
                var isGamarrEntry = (string)e.Attribute("source") == IdSource && (string)e.Attribute("id") == id;
                var path = NormalizePath((string)e.Element("path"));

                return isGamarrEntry || (path != null && path.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase));
            });
        }

        private static void UpdateEntry(XElement entry, Game game, GameFile gameFile, List<ImageFileResult> images)
        {
            var metadata = game.GameMetadata?.Value ?? new GameMetadata();
            var folder = "./" + GetGameFolderName(game.Path) + "/";
            var values = new Dictionary<string, string>
            {
                { "path", folder + gameFile.RelativePath.Replace('\\', '/') },
                { "name", metadata.Title },
                { "desc", metadata.Overview },
                { "rating", FormatRating(metadata.Ratings) },
                { "releasedate", game.GetReleaseDate()?.ToString("yyyyMMdd'T'000000", CultureInfo.InvariantCulture) },
                { "developer", Join(metadata.Developers) },
                { "publisher", Join(metadata.Publishers) },
                { "genre", Join(metadata.Genres) },
                { "players", GetPlayers(metadata.GameModes) },
                { "image", GetImagePath(folder, images, "cover") },
                { "thumbnail", GetImagePath(folder, images, "screenshot") },
                { "fanart", GetImagePath(folder, images, "fanart") }
            };

            entry.SetAttributeValue("id", game.IgdbId.ToString(CultureInfo.InvariantCulture));
            entry.SetAttributeValue("source", IdSource);

            foreach (var name in ManagedElements)
            {
                var value = values[name];
                var element = entry.Element(name);

                if (value.IsNullOrWhiteSpace())
                {
                    element?.Remove();
                }
                else if (element == null)
                {
                    entry.Add(new XElement(name, value));
                }
                else
                {
                    element.Value = value;
                }
            }

            // Keep the managed elements first and in a stable order, the others (playcount, favorite…) after them
            var managed = ManagedElements.Select(n => entry.Element(n)).Where(e => e != null).ToList();
            var others = entry.Elements().Where(e => !ManagedElements.Contains(e.Name.LocalName)).ToList();

            entry.RemoveNodes();
            entry.Add(managed);
            entry.Add(others);
        }

        private static string GetGameFolderName(string gamePath)
        {
            return Path.GetFileName(gamePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        private static string NormalizePath(string path)
        {
            if (path.IsNullOrWhiteSpace())
            {
                return null;
            }

            path = path.Trim().Replace('\\', '/');

            return path.StartsWith("./") ? path : "./" + path.TrimStart('/');
        }

        private static string GetImagePath(string folder, List<ImageFileResult> images, string name)
        {
            var image = images.FirstOrDefault(i => Path.GetFileNameWithoutExtension(i.RelativePath) == name);

            return image == null ? null : folder + image.RelativePath.Replace('\\', '/');
        }

        private static string FormatRating(Ratings ratings)
        {
            var rating = ratings?.Igdb?.Value ?? 0m;

            if (rating <= 0)
            {
                rating = ratings?.Critic?.Value ?? 0m;
            }

            if (rating <= 0)
            {
                return null;
            }

            return Math.Round(Math.Min(rating, 100) / 100m, 2).ToString("0.0#", CultureInfo.InvariantCulture);
        }

        private static string GetPlayers(List<string> gameModes)
        {
            if (gameModes == null || gameModes.Count == 0)
            {
                return null;
            }

            // IGDB game modes don't have player counts, only a single player game is certain
            return gameModes.All(m => m.Equals("Single player", StringComparison.OrdinalIgnoreCase)) ? "1" : null;
        }

        private static string Join(List<string> values)
        {
            return values == null || values.Count == 0 ? null : string.Join(", ", values.Where(v => v.IsNotNullOrWhiteSpace()).Distinct());
        }

        private static string Serialize(XDocument document)
        {
            // Re-indent from scratch so updated entries are formatted like the rest of the file
            foreach (var text in document.DescendantNodes().OfType<XText>().Where(t => t is not XCData && string.IsNullOrWhiteSpace(t.Value) && t.Parent?.HasElements == true).ToList())
            {
                text.Remove();
            }

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t",
                Encoding = new UTF8Encoding(false),
                OmitXmlDeclaration = false
            };

            using var stream = new MemoryStream();

            using (var writer = XmlWriter.Create(stream, settings))
            {
                document.Save(writer);
            }

            return Encoding.UTF8.GetString(stream.ToArray()).Trim() + Environment.NewLine;
        }
    }
}
