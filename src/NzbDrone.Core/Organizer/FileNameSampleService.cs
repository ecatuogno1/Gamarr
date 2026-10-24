using System.Collections.Generic;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Games;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Organizer
{
    public interface IFilenameSampleService
    {
        SampleResult GetGameSample(NamingConfig nameSpec);
        string GetGameFolderSample(NamingConfig nameSpec);
    }

    public class FileNameSampleService : IFilenameSampleService
    {
        private readonly IBuildFileNames _buildFileNames;

        private static GameFile _gameFile;
        private static Game _game;
        private static List<CustomFormat> _customFormats;

        public FileNameSampleService(IBuildFileNames buildFileNames)
        {
            _buildFileNames = buildFileNames;

            _gameFile = new GameFile
            {
                Quality = new QualityModel(Quality.RomVerified, new Revision(2)),
                RelativePath = "Game Title, The (USA, Europe) (En,Fr,De) (Rev 1).sfc",
                SceneName = "Game.Title.USA.SNES-GAMARR",
                ReleaseGroup = "GAMARR",
                Edition = "Deluxe edition",
                Regions = new List<Region> { Region.USA, Region.Europe },
                Languages = new List<Language> { Language.English, Language.French, Language.German },
                Version = "1.1",
                ReleaseType = ReleaseType.Base,
                DiscNumber = 1,
                DiscCount = 2
            };

            _game = new Game
            {
                GameMetadata = new GameMetadata
                {
                    Title = "The Game: Title",
                    OriginalTitle = "The Original Game Title",
                    CollectionTitle = "The Game Collection",
                    CollectionIgdbId = 123654,
                    Year = 2010,
                    IgdbId = 345691
                },
                GameMetadataId = 1,
                Platform = new Platform
                {
                    Name = "Super Nintendo Entertainment System",
                    ShortName = "SNES",
                    FolderName = "snes",
                    IgdbPlatformId = 19
                }
            };

            _customFormats = new List<CustomFormat>
            {
                new CustomFormat
                {
                    Name = "Verified",
                    IncludeCustomFormatWhenRenaming = true
                },
                new CustomFormat
                {
                    Name = "No-Intro",
                    IncludeCustomFormatWhenRenaming = true
                }
            };
        }

        public SampleResult GetGameSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_game, _gameFile, nameSpec),
                Game = _game,
                GameFile = _gameFile
            };

            return result;
        }

        public string GetGameFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetGameFolder(_game, nameSpec);
        }

        private string BuildSample(Game game, GameFile gameFile, NamingConfig nameSpec)
        {
            try
            {
                return _buildFileNames.BuildFileName(game, gameFile, nameSpec, _customFormats);
            }
            catch (NamingFormatException)
            {
                return string.Empty;
            }
        }
    }
}
