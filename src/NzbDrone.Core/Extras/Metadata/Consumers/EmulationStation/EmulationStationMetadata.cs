using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Games;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Extras.Metadata.Consumers.EmulationStation
{
    // EmulationStation (ES-DE, RetroPie, Batocera) frontends: a gamelist.xml per platform folder (system) and cover,
    // screenshot and fanart images. Either in the ROM folders (gamelist.xml in the platform folder, images in a media
    // folder inside the game folder referenced from the gamelist) or in ES-DE's gamelists and downloaded_media
    // directories, see EmulationStationGamelistService.
    public class EmulationStationMetadata : MetadataBase<EmulationStationMetadataSettings>
    {
        public const string MediaFolder = "media";

        private static readonly Regex GameImagesRegex = new(@"^media[\\/](?:cover|screenshot|fanart)\.(?:png|jpe?g|webp)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IEmulationStationGamelistService _gamelistService;

        public EmulationStationMetadata(IEmulationStationGamelistService gamelistService)
        {
            _gamelistService = gamelistService;
        }

        public override string Name => "EmulationStation (ES-DE)";

        public override string GetFilenameAfterMove(Game game, GameFile gameFile, MetadataFile metadataFile)
        {
            // Images don't depend on the game file name
            return Path.Combine(game.Path, metadataFile.RelativePath);
        }

        public override MetadataFile FindMetadataFile(Game game, string path)
        {
            var relativePath = game.Path.GetRelativePath(path);

            if (relativePath.IsNullOrWhiteSpace() || !GameImagesRegex.IsMatch(relativePath))
            {
                return null;
            }

            return new MetadataFile
            {
                GameId = game.Id,
                Consumer = GetType().Name,
                RelativePath = relativePath,
                Type = MetadataType.GameImage
            };
        }

        public override MetadataFileResult GameMetadata(Game game, GameFile gameFile)
        {
            if (Settings.Gamelist)
            {
                // gamelist.xml is shared by every game of the platform folder, so it isn't tracked as a metadata file
                // of the game (which would delete it with the game).
                _gamelistService.UpdateGame(game, gameFile, Settings.GameImages ? GetImages(game) : new List<ImageFileResult>(), Settings);
            }

            return null;
        }

        public override List<ImageFileResult> GameImages(Game game)
        {
            // With ES-DE's directories the images go to its downloaded_media folder (written with the gamelist)
            return Settings.GameImages && !Settings.UseEsDeDirectories ? GetImages(game) : new List<ImageFileResult>();
        }

        public static List<ImageFileResult> GetImages(Game game)
        {
            var images = game.GameMetadata?.Value?.Images ?? new List<MediaCover.MediaCover>();
            var result = new List<ImageFileResult>();

            AddImage(result, "cover", images.FirstOrDefault(i => i.CoverType == MediaCoverTypes.Poster));
            AddImage(result, "screenshot", images.FirstOrDefault(i => i.CoverType == MediaCoverTypes.Screenshot));
            AddImage(result, "fanart", images.FirstOrDefault(i => i.CoverType == MediaCoverTypes.Fanart));

            return result;
        }

        private static void AddImage(List<ImageFileResult> result, string name, MediaCover.MediaCover image)
        {
            if (image == null || image.RemoteUrl.IsNullOrWhiteSpace())
            {
                return;
            }

            var extension = Path.GetExtension(image.RemoteUrl.Split('?')[0]);

            if (extension.IsNullOrWhiteSpace() || extension.Length > 5)
            {
                extension = ".jpg";
            }

            result.Add(new ImageFileResult(Path.Combine(MediaFolder, name + extension.ToLowerInvariant()), image.RemoteUrl));
        }
    }
}
