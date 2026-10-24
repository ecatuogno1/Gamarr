using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Games;
using NzbDrone.Core.MediaFiles.GameImport;
using NzbDrone.Core.MediaFiles.GameImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.GameImport.Specifications
{
    [TestFixture]
    public class MatchesPlatformSpecificationFixture : CoreTest<MatchesPlatformSpecification>
    {
        private LocalGame _localGame;

        [SetUp]
        public void Setup()
        {
            _localGame = new LocalGame
            {
                Game = new Game { Platform = PlatformDefinitions.Get(19) }
            };
        }

        [TestCase(@"C:\Downloads\Super Mario World (USA).sfc")]
        [TestCase(@"C:\Downloads\Super Mario World (USA).smc")]
        [TestCase(@"C:\Downloads\Super Mario World (USA).zip")]
        [TestCase(@"C:\Downloads\Super Mario World (USA).7z")]
        public void should_accept_platform_and_generic_formats(string path)
        {
            _localGame.Path = path.AsOsAgnostic();

            Subject.IsSatisfiedBy(_localGame, null).Accepted.Should().BeTrue();
        }

        [TestCase(@"C:\Downloads\Super Mario Odyssey.nsp")]
        [TestCase(@"C:\Downloads\Pokemon Emerald (USA).gba")]
        public void should_reject_other_platform_formats(string path)
        {
            _localGame.Path = path.AsOsAgnostic();

            var decision = Subject.IsSatisfiedBy(_localGame, null);

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(ImportRejectionReason.WrongPlatformFile);
        }
    }

    [TestFixture]
    public class NotPackedReleaseSpecificationFixture : CoreTest<NotPackedReleaseSpecification>
    {
        private LocalGame _localGame;

        [SetUp]
        public void Setup()
        {
            _localGame = new LocalGame
            {
                Path = @"C:\Downloads\Game.Name-CODEX\codex-game.name.rar".AsOsAgnostic(),
                Game = new Game { Platform = PlatformDefinitions.Get(19) },
                CompanionFiles = new List<string>()
            };

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(true);
        }

        private void GivenFolderFiles(params string[] files)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFiles(It.IsAny<string>(), false))
                  .Returns(files);
        }

        [Test]
        public void should_reject_rar_set()
        {
            GivenFolderFiles(_localGame.Path, @"C:\Downloads\Game.Name-CODEX\codex-game.name.r00".AsOsAgnostic());

            var decision = Subject.IsSatisfiedBy(_localGame, null);

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(ImportRejectionReason.PackedRelease);
        }

        [Test]
        public void should_reject_multi_part_rar()
        {
            _localGame.Path = @"C:\Downloads\Game.Name-CODEX\codex-game.name.part1.rar".AsOsAgnostic();
            GivenFolderFiles(_localGame.Path, @"C:\Downloads\Game.Name-CODEX\codex-game.name.part2.rar".AsOsAgnostic());

            Subject.IsSatisfiedBy(_localGame, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_reject_single_rar_for_pc_game()
        {
            _localGame.Game.Platform = PlatformDefinitions.Get(6);
            GivenFolderFiles(_localGame.Path);

            Subject.IsSatisfiedBy(_localGame, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_accept_single_rar_rom()
        {
            GivenFolderFiles(_localGame.Path);

            Subject.IsSatisfiedBy(_localGame, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_zip_rom()
        {
            _localGame.Path = @"C:\Downloads\Super Mario World (USA).zip".AsOsAgnostic();

            Subject.IsSatisfiedBy(_localGame, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_existing_file()
        {
            _localGame.ExistingFile = true;

            Subject.IsSatisfiedBy(_localGame, null).Accepted.Should().BeTrue();
        }
    }
}
