using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Companions;
using NzbDrone.Core.Games;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Companions
{
    [TestFixture]
    public class CompanionFileManagerFixture : CoreTest<CompanionFileManager>
    {
        private Game _game;
        private GameFile _gameFile;
        private LocalGame _localGame;
        private string _releaseFolder;

        [SetUp]
        public void Setup()
        {
            _releaseFolder = @"C:\Downloads\Game (USA)".AsOsAgnostic();

            _game = new Game { Id = 1, Path = @"C:\Games\psx\Game (1998)".AsOsAgnostic() };

            _gameFile = new GameFile { Id = 2, RelativePath = "Game (1998) (USA).m3u" };

            _localGame = new LocalGame
            {
                Game = _game,
                Path = Path.Combine(_releaseFolder, "Game (USA).m3u"),
                ReleaseRoot = _releaseFolder,
                CompanionFiles = new List<string>
                {
                    Path.Combine(_releaseFolder, "Disc 1", "Game (USA) (Disc 1).cue"),
                    Path.Combine(_releaseFolder, "Disc 1", "Game (USA) (Disc 1).bin"),
                    Path.Combine(_releaseFolder, "Game (USA) (Disc 2).chd")
                }
            };
        }

        [Test]
        public void should_keep_names_and_relative_structure()
        {
            var imported = Subject.ImportFiles(_localGame, _gameFile, _localGame.CompanionFiles, false).ToList();

            imported.Select(f => f.RelativePath).Should().BeEquivalentTo(new[]
            {
                Path.Combine("Disc 1", "Game (USA) (Disc 1).cue"),
                Path.Combine("Disc 1", "Game (USA) (Disc 1).bin"),
                "Game (USA) (Disc 2).chd"
            });

            imported.Should().OnlyContain(f => f.GameId == 1 && f.GameFileId == 2);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFile(Path.Combine(_releaseFolder, "Disc 1", "Game (USA) (Disc 1).bin"), Path.Combine(_game.Path, "Disc 1", "Game (USA) (Disc 1).bin"), TransferMode.Move, true), Times.Once());

            Mocker.GetMock<ICompanionFileService>()
                  .Verify(v => v.Upsert(It.Is<List<CompanionFile>>(l => l.Count == 3)), Times.Once());
        }

        [Test]
        public void should_copy_when_read_only()
        {
            Subject.ImportFiles(_localGame, _gameFile, _localGame.CompanionFiles, true).ToList();

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFile(It.IsAny<string>(), It.IsAny<string>(), TransferMode.Copy, true), Times.Exactly(3));
        }

        [Test]
        public void should_only_claim_companion_files()
        {
            Subject.CanImportFile(_localGame, _gameFile, _localGame.CompanionFiles[0], ".cue", false).Should().BeTrue();
            Subject.CanImportFile(_localGame, _gameFile, Path.Combine(_releaseFolder, "Game.nfo"), ".nfo", false).Should().BeFalse();
        }

        [Test]
        public void should_fail_when_a_companion_transfer_fails_keeping_the_imported_ones()
        {
            var failing = _localGame.CompanionFiles[1];

            Mocker.GetMock<IDiskTransferService>()
                  .Setup(s => s.TransferFile(failing, It.IsAny<string>(), It.IsAny<TransferMode>(), It.IsAny<bool>()))
                  .Throws(new IOException("Disk full"));

            Action import = () => Subject.ImportFiles(_localGame, _gameFile, _localGame.CompanionFiles, false).ToList();

            import.Should().Throw<Exception>().Where(e => e.GetType().Name == "CompanionImportException" && e.Message.Contains("Game (USA) (Disc 1).bin"));

            Mocker.GetMock<ICompanionFileService>()
                  .Verify(v => v.Upsert(It.Is<List<CompanionFile>>(l => l.Count == 2)), Times.Once());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_not_transfer_companion_outside_of_release()
        {
            var config = Path.Combine(@"C:\Config".AsOsAgnostic(), "gamarr.db");
            var otherGame = Path.Combine(Path.GetDirectoryName(_releaseFolder), "Other Game (USA)", "disc.chd");
            var traversal = Path.Combine(_releaseFolder, "..", "Other Game (USA)", "disc.chd");

            _localGame.CompanionFiles.AddRange(new[] { config, otherGame, traversal });

            Action import = () => Subject.ImportFiles(_localGame, _gameFile, _localGame.CompanionFiles, false).ToList();

            import.Should().Throw<Exception>().Where(e => e.GetType().Name == "CompanionImportException");

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFile(It.IsIn(config, otherGame, traversal), It.IsAny<string>(), It.IsAny<TransferMode>(), It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TransferMode>(), It.IsAny<bool>()), Times.Exactly(3));

            ExceptionVerification.ExpectedWarns(2);
        }
    }
}
