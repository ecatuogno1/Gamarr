using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Consumers.EmulationStation;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.Events;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Metadata.Consumers.EmulationStation
{
    // Gamelist locations (ROM folder / ES-DE directories) and moving entries with the game folder, on an in-memory disk
    [TestFixture]
    public class EmulationStationGamelistLocationFixture : CoreTest<EmulationStationGamelistService>
    {
        private static readonly string Root = @"C:\Games".AsOsAgnostic();
        private static readonly string EsDe = @"C:\Home\ES-DE".AsOsAgnostic();

        private Dictionary<string, string> _files;
        private Game _game;
        private GameFile _gameFile;
        private EmulationStationMetadataSettings _romFolder;
        private EmulationStationMetadataSettings _esDe;

        [SetUp]
        public void Setup()
        {
            _files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            _game = new Game
            {
                Id = 1,
                Path = Path.Combine(Root, "snes", "Super Mario World (1990)"),
                GameMetadata = new GameMetadata
                {
                    IgdbId = 1070,
                    Title = "Super Mario World",
                    Images = new List<MediaCover.MediaCover>
                    {
                        new(MediaCoverTypes.Poster, "https://images.igdb.com/igdb/image/upload/t_original/co1.jpg"),
                        new(MediaCoverTypes.Screenshot, "https://images.igdb.com/igdb/image/upload/t_1080p/sc1.png")
                    }
                }
            };

            _gameFile = new GameFile { Id = 3, RelativePath = "Super Mario World (USA).sfc" };

            _romFolder = new EmulationStationMetadataSettings();
            _esDe = new EmulationStationMetadataSettings
            {
                GamelistLocation = (int)EmulationStationGamelistLocation.EsDeGamelists,
                GamelistDirectory = Path.Combine(EsDe, "gamelists"),
                MediaDirectory = Path.Combine(EsDe, "downloaded_media")
            };

            var disk = Mocker.GetMock<IDiskProvider>();

            disk.Setup(s => s.FileExists(It.IsAny<string>())).Returns<string>(p => _files.ContainsKey(p));
            disk.Setup(s => s.FolderExists(It.IsAny<string>())).Returns(true);
            disk.Setup(s => s.ReadAllText(It.IsAny<string>())).Returns<string>(p => _files[p]);
            disk.Setup(s => s.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((p, c) => _files[p] = c);
            disk.Setup(s => s.DeleteFile(It.IsAny<string>())).Callback<string>(p => _files.Remove(p));
            disk.Setup(s => s.MoveFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, string, bool>((source, destination, _) =>
                {
                    _files[destination] = _files[source];
                    _files.Remove(source);
                });

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()))
                  .Callback<string, string>((url, path) => _files[path] = url);
        }

        private List<XElement> Games(string gamelistPath)
        {
            return XDocument.Parse(_files[gamelistPath]).Root.Elements("game").ToList();
        }

        private string Media(params string[] parts)
        {
            return Path.Combine(new[] { EsDe, "downloaded_media" }.Concat(parts).ToArray());
        }

        [Test]
        public void should_expand_home_folder()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify);

            EmulationStationMetadataSettings.ExpandHome("~/ES-DE/gamelists").Should().Be(Path.Combine(home, "ES-DE/gamelists"));
            EmulationStationMetadataSettings.ExpandHome("~").Should().Be(home);
            EmulationStationMetadataSettings.ExpandHome("/data/gamelists").Should().Be("/data/gamelists");
        }

        [Test]
        public void should_default_to_rom_folder_location_and_es_de_directories()
        {
            _romFolder.UseEsDeDirectories.Should().BeFalse();
            _romFolder.GamelistDirectory.Should().Be("~/ES-DE/gamelists");
            _romFolder.MediaDirectory.Should().Be("~/ES-DE/downloaded_media");
        }

        [Test]
        public void should_write_gamelist_to_es_de_gamelists_directory_of_the_system()
        {
            Subject.UpdateGame(_game, _gameFile, EmulationStationMetadata.GetImages(_game), _esDe);

            var gamelistPath = Path.Combine(EsDe, "gamelists", "snes", "gamelist.xml");

            _files.Should().NotContainKey(Path.Combine(Root, "snes", "gamelist.xml"));

            var entry = Games(gamelistPath).Single();

            entry.Element("path").Value.Should().Be("./Super Mario World (1990)/Super Mario World (USA).sfc");
            entry.Element("image").Should().BeNull();
            entry.Element("thumbnail").Should().BeNull();
        }

        [Test]
        public void should_save_images_in_es_de_downloaded_media_layout()
        {
            Subject.UpdateGame(_game, _gameFile, EmulationStationMetadata.GetImages(_game), _esDe);

            _files.Should().ContainKey(Media("snes", "covers", "Super Mario World (1990)", "Super Mario World (USA).jpg"));
            _files.Should().ContainKey(Media("snes", "screenshots", "Super Mario World (1990)", "Super Mario World (USA).png"));
        }

        [Test]
        public void should_not_download_es_de_images_that_exist()
        {
            _files[Media("snes", "covers", "Super Mario World (1990)", "Super Mario World (USA).png")] = "scraped";

            Subject.UpdateGame(_game, _gameFile, EmulationStationMetadata.GetImages(_game), _esDe);

            _files.Should().NotContainKey(Media("snes", "covers", "Super Mario World (1990)", "Super Mario World (USA).jpg"));
        }

        [Test]
        public void should_keep_es_de_media_when_game_file_is_renamed()
        {
            Subject.UpdateGame(_game, _gameFile, EmulationStationMetadata.GetImages(_game), _esDe);
            _files[Media("snes", "covers", "Super Mario World (1990)", "Super Mario World (USA).jpg")] = "scraped";

            _gameFile.RelativePath = "Super Mario World (1990) (USA).sfc";
            Subject.UpdateGame(_game, _gameFile, new List<ImageFileResult>(), _esDe);

            _files[Media("snes", "covers", "Super Mario World (1990)", "Super Mario World (1990) (USA).jpg")].Should().Be("scraped");
            _files.Should().NotContainKey(Media("snes", "covers", "Super Mario World (1990)", "Super Mario World (USA).jpg"));
        }

        [Test]
        public void should_remove_entry_and_es_de_media()
        {
            Subject.UpdateGame(_game, _gameFile, EmulationStationMetadata.GetImages(_game), _esDe);

            Subject.RemoveGame(_game, _esDe);

            Games(Path.Combine(EsDe, "gamelists", "snes", "gamelist.xml")).Should().BeEmpty();
            _files.Keys.Should().NotContain(k => k.StartsWith(Media()));
        }

        [Test]
        public void should_move_entry_to_gamelist_of_new_platform_folder_keeping_user_fields()
        {
            var source = Path.Combine(Root, "snes", "gamelist.xml");

            _files[source] = @"<gameList>
	<game><path>./Chrono Trigger (1995)/Chrono Trigger.sfc</path><name>Chrono Trigger</name></game>
	<game id=""1070"" source=""IGDB""><path>./Super Mario World (1990)/Super Mario World (USA).sfc</path><name>Super Mario World</name><image>./Super Mario World (1990)/media/cover.jpg</image><playcount>3</playcount></game>
</gameList>";

            var destinationPath = Path.Combine(Root, "sfc", "Super Mario World (1990)");

            Subject.MoveGame(_game, _game.Path, destinationPath, _romFolder);

            Games(source).Single().Element("name").Value.Should().Be("Chrono Trigger");

            var moved = Games(Path.Combine(Root, "sfc", "gamelist.xml")).Single();

            moved.Element("path").Value.Should().Be("./Super Mario World (1990)/Super Mario World (USA).sfc");
            moved.Element("image").Value.Should().Be("./Super Mario World (1990)/media/cover.jpg");
            moved.Element("playcount").Value.Should().Be("3");
        }

        [Test]
        public void should_fix_paths_when_game_folder_name_changes()
        {
            var source = Path.Combine(Root, "snes", "gamelist.xml");
            _files[source] = @"<gameList><game id=""1070"" source=""IGDB""><path>./Super Mario World (1990)/smw.sfc</path></game></gameList>";

            Subject.MoveGame(_game, _game.Path, Path.Combine(Root, "snes", "Super Mario World (1991)"), _romFolder);

            Games(source).Single().Element("path").Value.Should().Be("./Super Mario World (1991)/smw.sfc");
        }

        [Test]
        public void should_move_entry_and_media_between_es_de_systems()
        {
            Subject.UpdateGame(_game, _gameFile, EmulationStationMetadata.GetImages(_game), _esDe);

            Subject.MoveGame(_game, _game.Path, Path.Combine(Root, "sfc", "Super Mario World (1990)"), _esDe);

            Games(Path.Combine(EsDe, "gamelists", "snes", "gamelist.xml")).Should().BeEmpty();
            Games(Path.Combine(EsDe, "gamelists", "sfc", "gamelist.xml")).Should().ContainSingle();

            _files.Should().ContainKey(Media("sfc", "covers", "Super Mario World (1990)", "Super Mario World (USA).jpg"));
            _files.Should().NotContainKey(Media("snes", "covers", "Super Mario World (1990)", "Super Mario World (USA).jpg"));
        }

        [Test]
        public void should_move_entries_of_enabled_consumers_when_game_is_moved()
        {
            var source = Path.Combine(Root, "snes", "gamelist.xml");
            _files[source] = @"<gameList><game id=""1070"" source=""IGDB""><path>./Super Mario World (1990)/smw.sfc</path></game></gameList>";

            Mocker.GetMock<IMetadataRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<MetadataDefinition>
                  {
                      new() { Enable = true, Implementation = nameof(EmulationStationMetadata), Settings = _romFolder }
                  });

            var destination = Path.Combine(Root, "sfc", "Super Mario World (1990)");

            Subject.Handle(new GameMovedEvent(_game, _game.Path, destination));

            Games(Path.Combine(Root, "sfc", "gamelist.xml")).Should().ContainSingle();
        }

        private string GivenEsDeGamelistEntry(string romPath)
        {
            var gamelistPath = Path.Combine(EsDe, "gamelists", "snes", "gamelist.xml");

            _files[gamelistPath] = $@"<?xml version=""1.0""?>
<gameList>
	<game id=""1070"" source=""IGDB"">
		<path>{romPath}</path>
		<name>Super Mario World</name>
	</game>
</gameList>";

            return gamelistPath;
        }

        [Test]
        public void should_not_delete_files_outside_of_es_de_media_when_game_list_path_goes_up()
        {
            GivenEsDeGamelistEntry("./Super Mario World (1990)/../../../../../Secret/config.sfc");

            // Where the media of that path would be, outside of downloaded_media
            var outside = Path.Combine(Media("snes", "covers"), "Super Mario World (1990)", "..", "..", "..", "..", "..", "Secret", "config.jpg");
            _files[outside] = "secret";
            _files[Path.GetFullPath(outside)] = "secret";

            Subject.RemoveGame(_game, _esDe);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFile(It.Is<string>(p => !Path.GetFullPath(p).StartsWith(Media()))), Times.Never());

            _files[outside].Should().Be("secret");
        }

        [Test]
        public void should_not_move_files_from_outside_of_es_de_media_when_game_list_path_goes_up()
        {
            GivenEsDeGamelistEntry("./Super Mario World (1990)/../../../../../Secret/config.sfc");

            var outside = Path.Combine(Media("snes", "covers"), "Super Mario World (1990)", "..", "..", "..", "..", "..", "Secret", "config.jpg");
            _files[outside] = "secret";

            Subject.UpdateGame(_game, _gameFile, new List<ImageFileResult>(), _esDe);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.MoveFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());

            _files[outside].Should().Be("secret");
        }

        [Test]
        [Platform(Exclude = "Win")]
        public void should_not_delete_files_through_a_symlink_out_of_es_de_media()
        {
            var media = Path.Combine(TempFolder, "downloaded_media");
            var outside = Path.Combine(TempFolder, "outside");
            Directory.CreateDirectory(Path.Combine(media, "snes", "covers"));
            Directory.CreateDirectory(outside);

            var secret = Path.Combine(outside, "Super Mario World (USA).jpg");
            File.WriteAllText(secret, "secret");

            // downloaded_media/snes/covers/<game folder> links to a folder outside of the media folder
            Directory.CreateSymbolicLink(Path.Combine(media, "snes", "covers", "Super Mario World (1990)"), outside);

            _esDe.MediaDirectory = media;
            GivenEsDeGamelistEntry("./Super Mario World (1990)/Super Mario World (USA).sfc");

            var disk = Mocker.GetMock<IDiskProvider>();
            disk.Setup(s => s.FileExists(It.IsAny<string>())).Returns<string>(p => _files.ContainsKey(p) || File.Exists(p));
            disk.Setup(s => s.DeleteFile(It.IsAny<string>())).Callback<string>(File.Delete);

            Subject.RemoveGame(_game, _esDe);

            File.Exists(secret).Should().BeTrue();
        }
    }
}
