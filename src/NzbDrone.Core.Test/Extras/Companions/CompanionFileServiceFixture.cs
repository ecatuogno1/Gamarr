using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Companions;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Games;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Companions
{
    [TestFixture]
    public class CompanionFileServiceFixture : CoreTest<CompanionFileService>
    {
        private Game _game;
        private GameFile _oldFile;
        private string _trackPath;

        [SetUp]
        public void Setup()
        {
            _game = new Game { Id = 1, Path = @"C:\Games\psx\Game (1998)".AsOsAgnostic() };
            _oldFile = new GameFile { Id = 10, GameId = 1, RelativePath = "Game (1998).cue" };
            _trackPath = Path.Combine(_game.Path, "track.bin");

            Mocker.GetMock<IGameService>().Setup(s => s.GetGame(1)).Returns(_game);

            Mocker.GetMock<IExtraFileRepository<CompanionFile>>()
                  .Setup(s => s.GetFilesByGameFile(10))
                  .Returns(new List<CompanionFile> { new CompanionFile { Id = 100, GameId = 1, GameFileId = 10, RelativePath = "track.bin" } });

            Mocker.GetMock<IDiskProvider>().Setup(s => s.FileExists(_trackPath)).Returns(true);
            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetParentFolder(It.IsAny<string>())).Returns<string>(p => Path.GetDirectoryName(p));
        }

        private void HandleUpgrade()
        {
            // Synchronously, before the new release's files are put in place
            ((IHandle<GameFileDeletedEvent>)Subject).Handle(new GameFileDeletedEvent(_oldFile, DeleteMediaFileReason.Upgrade));
        }

        [Test]
        public void should_remove_old_companions_while_the_game_file_is_deleted()
        {
            Subject.Should().BeAssignableTo<IHandle<GameFileDeletedEvent>>();
            Subject.Should().NotBeAssignableTo<IHandleAsync<GameFileDeletedEvent>>();

            Mocker.GetMock<IExtraFileRepository<CompanionFile>>()
                  .Setup(s => s.GetFilesByGame(1))
                  .Returns(new List<CompanionFile> { new CompanionFile { Id = 100, GameId = 1, GameFileId = 10, RelativePath = "track.bin" } });

            HandleUpgrade();

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_trackPath, It.IsAny<string>()), Times.Once());
            Mocker.GetMock<IExtraFileRepository<CompanionFile>>().Verify(v => v.DeleteForGameFile(10), Times.Once());
        }

        [Test]
        public void should_not_delete_a_companion_path_that_belongs_to_the_new_game_file()
        {
            Mocker.GetMock<IExtraFileRepository<CompanionFile>>()
                  .Setup(s => s.GetFilesByGame(1))
                  .Returns(new List<CompanionFile>
                  {
                      new CompanionFile { Id = 100, GameId = 1, GameFileId = 10, RelativePath = "track.bin" },
                      new CompanionFile { Id = 200, GameId = 1, GameFileId = 11, RelativePath = "track.bin" }
                  });

            HandleUpgrade();

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            Mocker.GetMock<IExtraFileRepository<CompanionFile>>().Verify(v => v.DeleteForGameFile(10), Times.Once());
        }
    }
}
