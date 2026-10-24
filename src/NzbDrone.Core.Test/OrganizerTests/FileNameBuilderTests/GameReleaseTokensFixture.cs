using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Games;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class GameReleaseTokensFixture : CoreTest<FileNameBuilder>
    {
        private Game _game;
        private GameFile _gameFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _game = Builder<Game>
                    .CreateNew()
                    .With(s => s.Title = "Super Mario World")
                    .With(s => s.Year = 1990)
                    .With(s => s.Platform = PlatformDefinitions.Get(19))
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameGames = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _gameFile = new GameFile
            {
                Quality = new QualityModel(Quality.RomVerified),
                RelativePath = "Super Mario World (USA, Europe) (Rev 1).sfc",
                Regions = new List<Region> { Region.USA, Region.Europe },
                Languages = new List<Language> { Language.English, Language.French },
                Version = "1.1",
                ReleaseType = ReleaseType.Base
            };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                .Setup(v => v.All())
                .Returns(new List<CustomFormat>());
        }

        [Test]
        public void should_use_default_format()
        {
            Subject.BuildFileName(_game, _gameFile).Should().Be("Super Mario World (1990) (USA, Europe) [v1.1]");
        }

        [Test]
        public void should_skip_empty_region_and_version()
        {
            _gameFile.Regions = new List<Region>();
            _gameFile.Version = null;

            Subject.BuildFileName(_game, _gameFile).Should().Be("Super Mario World (1990)");
        }

        [Test]
        public void should_replace_release_type_disc_and_languages()
        {
            _namingConfig.StandardGameFormat = "{Game Title}{ (Languages)}{ [Release Type]}{ (Disc)}";
            _gameFile.ReleaseType = ReleaseType.Update;
            _gameFile.DiscNumber = 2;

            Subject.BuildFileName(_game, _gameFile).Should().Be("Super Mario World (En,Fr) [Update] (Disc 2)");
        }

        [Test]
        public void should_leave_release_type_empty_for_base_games()
        {
            _namingConfig.StandardGameFormat = "{Game Title}{ [Release Type]}";

            Subject.BuildFileName(_game, _gameFile).Should().Be("Super Mario World");
        }

        [Test]
        public void should_replace_platform_and_quality_tokens()
        {
            _namingConfig.StandardGameFormat = "{Game Title} [{Platform Short}] [{Quality Title}]";

            Subject.BuildFileName(_game, _gameFile).Should().Be("Super Mario World [SNES] [ROM-Verified]");
        }

        [Test]
        public void should_format_build_version()
        {
            _namingConfig.StandardGameFormat = "{Game Title} {Version}";
            _gameFile.Version = "Build 1234567";

            Subject.BuildFileName(_game, _gameFile).Should().Be("Super Mario World Build 1234567");
        }

        [TestCase("setup_super_mario_world_1.0_(1234).exe")]
        [TestCase("Super Mario World.ccd")]
        public void should_keep_original_name_of_installers_and_clonecd_images(string fileName)
        {
            _gameFile.RelativePath = fileName;

            Subject.BuildFileName(_game, _gameFile).Should().Be(System.IO.Path.GetFileNameWithoutExtension(fileName));
        }

        [Test]
        public void should_not_allow_release_tokens_in_folder_format()
        {
            FileNameValidation.DeprecatedGameFolderTokensRegex.IsMatch("{Game Title} ({Region})").Should().BeTrue();
            FileNameValidation.DeprecatedGameFolderTokensRegex.IsMatch("{Platform Folder}/{Game Title} ({Release Year})").Should().BeFalse();
        }
    }
}
