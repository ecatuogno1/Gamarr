using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Metadata.Consumers.EmulationStation;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Games;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Metadata.Consumers.EmulationStation
{
    [TestFixture]
    public class EmulationStationGamelistServiceFixture : CoreTest<EmulationStationGamelistService>
    {
        private Game _game;
        private GameFile _gameFile;
        private string _gamelistPath;
        private string _existing;
        private string _written;

        [SetUp]
        public void Setup()
        {
            _game = new Game
            {
                Id = 1,
                Path = @"C:\Games\snes\Super Mario World (1990)".AsOsAgnostic(),
                Platform = new Platform { Id = 2, Name = "Super Nintendo Entertainment System", IgdbPlatformId = 19 },
                GameMetadata = new GameMetadata
                {
                    IgdbId = 1070,
                    Title = "Super Mario World",
                    Overview = "Mario & Luigi <save> Dinosaur Land.",
                    ReleaseDate = new DateTime(1990, 11, 21),
                    PlatformReleaseDates = new List<PlatformReleaseDate> { new() { IgdbPlatformId = 19, Date = new DateTime(1991, 8, 13) } },
                    Developers = new List<string> { "Nintendo EAD" },
                    Publishers = new List<string> { "Nintendo" },
                    Genres = new List<string> { "Platform", "Adventure" },
                    GameModes = new List<string> { "Single player" },
                    Ratings = new Ratings { Igdb = new RatingChild { Value = 92.4m, Votes = 100 } },
                    Images = new List<MediaCover.MediaCover>
                    {
                        new(MediaCoverTypes.Poster, "https://images.igdb.com/igdb/image/upload/t_original/co1.jpg"),
                        new(MediaCoverTypes.Screenshot, "https://images.igdb.com/igdb/image/upload/t_1080p/sc1.jpg")
                    }
                }
            };

            _gameFile = new GameFile { Id = 3, RelativePath = "Super Mario World (1990) (USA).sfc" };
            _gamelistPath = @"C:\Games\snes\gamelist.xml".AsOsAgnostic();
            _existing = null;
            _written = null;

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.FileExists(_gamelistPath))
                .Returns(() => _existing != null);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.ReadAllText(_gamelistPath))
                .Returns(() => _existing);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.WriteAllText(_gamelistPath, It.IsAny<string>()))
                .Callback<string, string>((_, contents) =>
                {
                    _written = contents;
                    _existing = contents;
                });
        }

        private List<ImageFileResult> Images()
        {
            return EmulationStationMetadata.GetImages(_game);
        }

        private List<XElement> WrittenGames()
        {
            return XDocument.Parse(_written).Root.Elements("game").ToList();
        }

        [Test]
        public void should_create_gamelist_in_platform_folder()
        {
            Subject.UpdateGame(_game, _gameFile, Images());

            _written.Should().StartWith("<?xml");

            var game = WrittenGames().Single();

            game.Attribute("id").Value.Should().Be("1070");
            game.Attribute("source").Value.Should().Be("IGDB");
            game.Element("path").Value.Should().Be("./Super Mario World (1990)/Super Mario World (1990) (USA).sfc");
            game.Element("name").Value.Should().Be("Super Mario World");
            game.Element("desc").Value.Should().Be("Mario & Luigi <save> Dinosaur Land.");
            game.Element("releasedate").Value.Should().Be("19910813T000000");
            game.Element("developer").Value.Should().Be("Nintendo EAD");
            game.Element("publisher").Value.Should().Be("Nintendo");
            game.Element("genre").Value.Should().Be("Platform, Adventure");
            game.Element("players").Value.Should().Be("1");
            game.Element("rating").Value.Should().Be("0.92");
            game.Element("image").Value.Should().Be("./Super Mario World (1990)/media/cover.jpg");
            game.Element("thumbnail").Value.Should().Be("./Super Mario World (1990)/media/screenshot.jpg");
            game.Element("fanart").Should().BeNull();
        }

        [Test]
        public void should_keep_other_games_and_update_own_entry_keeping_user_fields()
        {
            _existing = @"<?xml version=""1.0""?>
<gameList>
	<game>
		<path>./Chrono Trigger (USA).sfc</path>
		<name>Chrono Trigger</name>
		<favorite>true</favorite>
	</game>
	<game id=""1070"" source=""IGDB"">
		<path>./Super Mario World (1990)/Old Name.sfc</path>
		<name>Old</name>
		<developer>Someone</developer>
		<playcount>12</playcount>
		<lastplayed>20240101T120000</lastplayed>
	</game>
</gameList>";

            Subject.UpdateGame(_game, _gameFile, new List<ImageFileResult>());

            var games = WrittenGames();

            games.Should().HaveCount(2);
            games[0].Element("name").Value.Should().Be("Chrono Trigger");
            games[0].Element("favorite").Value.Should().Be("true");

            games[1].Element("path").Value.Should().Be("./Super Mario World (1990)/Super Mario World (1990) (USA).sfc");
            games[1].Element("name").Value.Should().Be("Super Mario World");
            games[1].Element("developer").Value.Should().Be("Nintendo EAD");
            games[1].Element("playcount").Value.Should().Be("12");
            games[1].Element("lastplayed").Value.Should().Be("20240101T120000");
            games[1].Element("image").Should().BeNull();
        }

        [Test]
        public void should_take_over_scraped_entry_inside_game_folder()
        {
            _existing = @"<gameList><game><path>./Super Mario World (1990)/smw.sfc</path><name>SMW</name><hidden>false</hidden></game></gameList>";

            Subject.UpdateGame(_game, _gameFile, Images());

            var game = WrittenGames().Single();

            game.Element("name").Value.Should().Be("Super Mario World");
            game.Element("hidden").Value.Should().Be("false");
            game.Attribute("source").Value.Should().Be("IGDB");
        }

        [Test]
        public void should_not_write_when_nothing_changed()
        {
            Subject.UpdateGame(_game, _gameFile, Images());
            Subject.UpdateGame(_game, _gameFile, Images());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.WriteAllText(_gamelistPath, It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_not_overwrite_invalid_gamelist()
        {
            _existing = "<gameList><game>";

            Subject.UpdateGame(_game, _gameFile, Images());

            _written.Should().BeNull();
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_remove_game_and_keep_others()
        {
            _existing = @"<gameList><game><path>./Other.sfc</path><name>Other</name></game></gameList>";

            Subject.UpdateGame(_game, _gameFile, Images());
            WrittenGames().Should().HaveCount(2);

            Subject.RemoveGame(_game);

            WrittenGames().Single().Element("name").Value.Should().Be("Other");
        }

        [Test]
        public void should_use_cue_path_for_multi_file_release_in_sub_folder()
        {
            _gameFile.RelativePath = Path.Combine("Disc 1", "Game (USA).cue");

            Subject.UpdateGame(_game, _gameFile, Images());

            WrittenGames().Single().Element("path").Value.Should().Be("./Super Mario World (1990)/Disc 1/Game (USA).cue");
        }

        [Test]
        public void should_not_set_players_for_multiplayer_games()
        {
            _game.GameMetadata.Value.GameModes = new List<string> { "Single player", "Multiplayer" };

            Subject.UpdateGame(_game, _gameFile, Images());

            WrittenGames().Single().Element("players").Should().BeNull();
        }
    }
}
