using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Games;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.GameImport;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.GameImport
{
    [TestFixture]
    public class GetSceneNameFixture : CoreTest
    {
        private LocalGame _localGame;
        private string _gameName = "Game.Title.Update.v1.2-CODEX";

        [SetUp]
        public void Setup()
        {
            var game = Builder<Game>.CreateNew()
                                        .With(e => e.QualityProfile = new QualityProfile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                        .With(s => s.Path = @"C:\Test\Games\Game Title".AsOsAgnostic())
                                        .Build();

            _localGame = new LocalGame
            {
                Game = game,
                Path = Path.Combine(game.Path, "Game Title - 2022 - Episode Title.iso"),
                Quality = new QualityModel(Quality.Pkg),
                ReleaseGroup = "DRONE"
            };
        }

        private void GivenExistingFileOnDisk()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesWithRelativePath(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns(new List<GameFile>());
        }

        [Test]
        public void should_use_download_client_item_title_as_scene_name()
        {
            _localGame.DownloadClientGameInfo = new ParsedGameInfo
            {
                ReleaseTitle = _gameName
            };

            SceneNameCalculator.GetSceneName(_localGame).Should()
                               .Be(_gameName);
        }

        [Test]
        public void should_use_file_name_as_scenename_only_if_it_looks_like_scenename()
        {
            _localGame.Path = Path.Combine(@"C:\Test\Unsorted Games", _gameName + ".iso")
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localGame).Should()
                               .Be(_gameName);
        }

        [Test]
        public void should_not_use_file_name_as_scenename_if_it_doesnt_look_like_scenename()
        {
            _localGame.Path = Path.Combine(@"C:\Test\Unsorted Games", _gameName, "aaaaa.iso")
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localGame).Should()
                               .BeNull();
        }

        [Test]
        public void should_use_folder_name_as_scenename_only_if_it_looks_like_scenename()
        {
            _localGame.FolderGameInfo = new ParsedGameInfo
            {
                ReleaseTitle = _gameName
            };

            SceneNameCalculator.GetSceneName(_localGame).Should()
                               .Be(_gameName);
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_it_doesnt_look_like_scenename()
        {
            _localGame.Path = Path.Combine(@"C:\Test\Unsorted Games", _gameName, "aaaaa.iso")
                                     .AsOsAgnostic();

            _localGame.FolderGameInfo = new ParsedGameInfo
            {
                ReleaseTitle = "aaaaa"
            };

            SceneNameCalculator.GetSceneName(_localGame).Should()
                               .BeNull();
        }

        [TestCase(".iso")]
        [TestCase(".par2")]
        [TestCase(".nzb")]
        public void should_remove_extension_from_nzb_title_for_scene_name(string extension)
        {
            _localGame.DownloadClientGameInfo = new ParsedGameInfo
            {
                ReleaseTitle = _gameName + extension
            };

            SceneNameCalculator.GetSceneName(_localGame).Should()
                               .Be(_gameName);
        }
    }
}
