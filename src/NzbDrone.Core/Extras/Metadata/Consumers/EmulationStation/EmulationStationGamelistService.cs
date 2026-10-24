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
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.Events;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras.Metadata.Consumers.EmulationStation
{
    public interface IEmulationStationGamelistService
    {
        void UpdateGame(Game game, GameFile gameFile, List<ImageFileResult> images);
        void RemoveGame(Game game);
    }

    // Maintains the gamelist.xml of a platform folder. Entries of other games (added by ES-DE scrapers or by hand) are
    // kept, an entry of a Gamarr game is found by its IGDB id or by a path inside the game's folder. Fields Gamarr
    // doesn't write (favorite, playcount, lastplayed, hidden…) are kept when an entry is updated.
    public class EmulationStationGamelistService : IEmulationStationGamelistService, IHandle<GamesDeletedEvent>
    {
        public const string GamelistFileName = "gamelist.xml";
        public const string IdSource = "IGDB";

        private static readonly object Lock = new();

        private static readonly string[] ManagedElements =
        {
            "path", "name", "desc", "rating", "releasedate", "developer", "publisher", "genre", "players", "image", "thumbnail", "fanart"
        };

        private readonly IDiskProvider _diskProvider;
        private readonly IMetadataRepository _metadataRepository;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly Logger _logger;

        public EmulationStationGamelistService(IDiskProvider diskProvider,
                                               IMetadataRepository metadataRepository,
                                               IMediaFileAttributeService mediaFileAttributeService,
                                               Logger logger)
        {
            _diskProvider = diskProvider;
            _metadataRepository = metadataRepository;
            _mediaFileAttributeService = mediaFileAttributeService;
            _logger = logger;
        }

        public static string GetGamelistPath(Game game)
        {
            var platformFolder = Path.GetDirectoryName(game.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            return platformFolder.IsNullOrWhiteSpace() ? null : Path.Combine(platformFolder, GamelistFileName);
        }

        public void UpdateGame(Game game, GameFile gameFile, List<ImageFileResult> images)
        {
            var gamelistPath = GetGamelistPath(game);

            if (gamelistPath == null || gameFile == null)
            {
                return;
            }

            Modify(gamelistPath, gameList =>
            {
                var existing = FindEntries(gameList, game).ToList();
                var entry = existing.FirstOrDefault() ?? new XElement("game");

                foreach (var duplicate in existing.Skip(1))
                {
                    duplicate.Remove();
                }

                UpdateEntry(entry, game, gameFile, images ?? new List<ImageFileResult>());

                if (entry.Parent == null)
                {
                    gameList.Add(entry);
                }
            });
        }

        public void RemoveGame(Game game)
        {
            var gamelistPath = GetGamelistPath(game);

            if (gamelistPath == null || !_diskProvider.FileExists(gamelistPath))
            {
                return;
            }

            Modify(gamelistPath, gameList => FindEntries(gameList, game).ToList().ForEach(e => e.Remove()));
        }

        public void Handle(GamesDeletedEvent message)
        {
            if (!message.DeleteFiles || !_metadataRepository.All().Any(d => d.Enable && d.Implementation == nameof(EmulationStationMetadata)))
            {
                return;
            }

            foreach (var game in message.Games.Where(g => g.Path.IsNotNullOrWhiteSpace()))
            {
                try
                {
                    RemoveGame(game);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to remove {0} from {1}", game, GetGamelistPath(game));
                }
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

                _logger.Debug("Writing EmulationStation game list: {0}", gamelistPath);
                _diskProvider.WriteAllText(gamelistPath, contents);
                _mediaFileAttributeService.SetFilePermissions(gamelistPath);
            }
        }

        private static IEnumerable<XElement> FindEntries(XElement gameList, Game game)
        {
            var folderPrefix = "./" + GetGameFolderName(game) + "/";

            return gameList.Elements("game").Where(e =>
            {
                var isGamarrEntry = (string)e.Attribute("source") == IdSource && (string)e.Attribute("id") == game.IgdbId.ToString(CultureInfo.InvariantCulture);
                var path = NormalizePath((string)e.Element("path"));

                return isGamarrEntry || (path != null && path.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase));
            });
        }

        private static void UpdateEntry(XElement entry, Game game, GameFile gameFile, List<ImageFileResult> images)
        {
            var metadata = game.GameMetadata?.Value ?? new GameMetadata();
            var folder = "./" + GetGameFolderName(game) + "/";
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

        private static string GetGameFolderName(Game game)
        {
            return Path.GetFileName(game.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
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
