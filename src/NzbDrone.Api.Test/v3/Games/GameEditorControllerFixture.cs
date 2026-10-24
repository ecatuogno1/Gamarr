using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using FluentValidation;
using Gamarr.Api.V3.Games;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.Commands;
using NzbDrone.Core.Games.Translations;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.Test.Common;

namespace NzbDrone.Api.Test.v3.Games
{
    [TestFixture]
    public class GameEditorControllerFixture : TestBase<GameEditorController>
    {
        private static readonly string Root = @"C:\Games".AsOsAgnostic();

        private Platform _snes;
        private Platform _sfc;
        private Platform _pc;
        private List<Game> _games;

        [SetUp]
        public void Setup()
        {
            _snes = new Platform { Id = 1, Name = "SNES", FolderName = "snes" };
            _sfc = new Platform { Id = 2, Name = "Super Famicom", FolderName = "sfc" };
            _pc = new Platform { Id = 3, Name = "PC", FolderName = "pc" };

            _games = new List<Game>
            {
                GivenGame(1, 100, "Super Mario World", _snes),
                GivenGame(2, 200, "Chrono Trigger", _snes)
            };

            var platforms = new[] { _snes, _sfc, _pc };

            Mocker.GetMock<IPlatformService>()
                  .Setup(s => s.Find(It.IsAny<int>()))
                  .Returns<int>(id => platforms.SingleOrDefault(p => p.Id == id));

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GetGames(It.IsAny<IEnumerable<int>>()))
                  .Returns<IEnumerable<int>>(ids => _games.Where(g => ids.Contains(g.Id)).ToList());

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.UpdateGame(It.IsAny<List<Game>>(), It.IsAny<bool>()))
                  .Returns<List<Game>, bool>((games, _) => games);

            Mocker.GetMock<IGameTranslationService>()
                  .Setup(s => s.GetAllTranslationsForLanguage(It.IsAny<Language>()))
                  .Returns(new List<GameTranslation>());

            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>(), null))
                  .Returns(Root);

            Mocker.GetMock<IQualityProfileService>()
                  .Setup(s => s.Exists(It.IsAny<int>()))
                  .Returns(true);

            Mocker.SetConstant<IBuildGamePaths>(Mocker.Resolve<GamePathBuilder>());
            Mocker.SetConstant(new GameEditorValidator(Mocker.Resolve<RootFolderExistsValidator>(), Mocker.Resolve<QualityProfileExistsValidator>()));
        }

        private static Game GivenGame(int id, int igdbId, string title, Platform platform)
        {
            return new Game
            {
                Id = id,
                Title = title,
                IgdbId = igdbId,
                PlatformId = platform.Id,
                Platform = platform,
                QualityProfileId = 1,
                Path = Path.Combine(Root, platform.FolderName, $"{title} (1990)")
            };
        }

        private void GivenInLibrary(Game game)
        {
            Mocker.GetMock<IGameService>()
                  .Setup(s => s.FindByIgdbIdAndPlatform(game.IgdbId, game.PlatformId))
                  .Returns(game);
        }

        private List<string> SaveExpectingErrors(GameEditorResource resource)
        {
            var exception = Assert.Throws<ValidationException>(() => Subject.SaveAll(resource));

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateGame(It.IsAny<List<Game>>(), It.IsAny<bool>()), Times.Never());

            return exception.Errors.Select(e => e.ErrorMessage).ToList();
        }

        [Test]
        public void should_reject_platform_change_listing_every_game_already_on_that_platform()
        {
            GivenInLibrary(GivenGame(10, 100, "Super Mario World", _sfc));
            GivenInLibrary(GivenGame(11, 200, "Chrono Trigger", _sfc));

            var errors = SaveExpectingErrors(new GameEditorResource { GameIds = new List<int> { 1, 2 }, PlatformId = _sfc.Id, MoveFiles = true });

            errors.Should().HaveCount(2);
            errors.Should().Contain(e => e.Contains("Super Mario World") && e.Contains("Super Famicom"));
            errors.Should().Contain(e => e.Contains("Chrono Trigger") && e.Contains("Super Famicom"));
        }

        [Test]
        public void should_reject_platform_change_of_the_same_game_selected_on_several_platforms()
        {
            _games.Add(GivenGame(3, 100, "Super Mario World", _pc));

            var errors = SaveExpectingErrors(new GameEditorResource { GameIds = new List<int> { 1, 3 }, PlatformId = _sfc.Id });

            errors.Should().ContainSingle().Which.Should().Contain("several platforms (SNES, PC)");
        }

        [Test]
        public void should_reject_unknown_platform()
        {
            var errors = SaveExpectingErrors(new GameEditorResource { GameIds = new List<int> { 1 }, PlatformId = 99 });

            errors.Should().ContainSingle().Which.Should().Contain("99");
        }

        [Test]
        public void should_list_per_game_validation_errors_with_the_game_title()
        {
            Mocker.GetMock<IQualityProfileService>()
                  .Setup(s => s.Exists(It.IsAny<int>()))
                  .Returns(false);

            var errors = SaveExpectingErrors(new GameEditorResource { GameIds = new List<int> { 1, 2 }, QualityProfileId = 5 });

            errors.Should().HaveCount(2);
            errors.Should().Contain(e => e.StartsWith("Super Mario World:"));
        }

        [Test]
        public void should_move_game_folders_to_new_platform_folder()
        {
            Subject.SaveAll(new GameEditorResource { GameIds = new List<int> { 1, 2 }, PlatformId = _sfc.Id, MoveFiles = true });

            _games.Should().OnlyContain(g => g.PlatformId == _sfc.Id);
            _games[0].Path.Should().Be(Path.Combine(Root, "sfc", "Super Mario World (1990)"));

            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.Push(It.Is<BulkMoveGameCommand>(c => c.Games.Count == 2 &&
                                                                      c.Games[0].SourcePath == Path.Combine(Root, "snes", "Super Mario World (1990)") &&
                                                                      c.Games[0].DestinationPath == Path.Combine(Root, "sfc", "Super Mario World (1990)")),
                                       It.IsAny<CommandPriority>(),
                                       It.IsAny<CommandTrigger>()),
                          Times.Once());
        }

        [Test]
        public void should_change_path_without_moving_when_files_are_not_moved()
        {
            Subject.SaveAll(new GameEditorResource { GameIds = new List<int> { 1 }, PlatformId = _sfc.Id, MoveFiles = false });

            _games[0].Path.Should().Be(Path.Combine(Root, "sfc", "Super Mario World (1990)"));

            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.Push(It.IsAny<BulkMoveGameCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Never());
        }

        [Test]
        public void should_not_change_path_of_game_outside_its_platform_folder()
        {
            var customPath = Path.Combine(Root, "Super Mario World");
            _games[0].Path = customPath;

            Subject.SaveAll(new GameEditorResource { GameIds = new List<int> { 1 }, PlatformId = _sfc.Id, MoveFiles = true });

            _games[0].PlatformId.Should().Be(_sfc.Id);
            _games[0].Path.Should().Be(customPath);

            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.Push(It.IsAny<BulkMoveGameCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Never());
        }

        [Test]
        public void should_reject_move_into_folder_another_game_uses()
        {
            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GamePathExists(Path.Combine(Root, "sfc", "Super Mario World (1990)")))
                  .Returns(true);

            var errors = SaveExpectingErrors(new GameEditorResource { GameIds = new List<int> { 1 }, PlatformId = _sfc.Id, MoveFiles = true });

            errors.Should().ContainSingle().Which.Should().Contain("another game already uses");
        }

        [Test]
        public void should_only_report_the_platform_conflict_of_a_game()
        {
            GivenInLibrary(GivenGame(10, 100, "Super Mario World", _sfc));

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GamePathExists(It.IsAny<string>()))
                  .Returns(true);

            var errors = SaveExpectingErrors(new GameEditorResource { GameIds = new List<int> { 1 }, PlatformId = _sfc.Id });

            errors.Should().ContainSingle().Which.Should().Contain("already in the library on Super Famicom");
        }
    }
}
