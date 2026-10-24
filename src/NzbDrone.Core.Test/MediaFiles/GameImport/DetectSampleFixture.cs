using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Games;
using NzbDrone.Core.MediaFiles.GameImport;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.GameImport
{
    [TestFixture]
    public class DetectSampleFixture : CoreTest<DetectSample>
    {
        private GameMetadata _game;

        [SetUp]
        public void Setup()
        {
            _game = Builder<GameMetadata>.CreateNew()
                                         .With(g => g.Title = "Super Mario World")
                                         .Build();
        }

        [TestCase(@"C:\Test\Super Mario World (USA).sfc")]
        [TestCase(@"C:\Test\Game.Name-CODEX\codex-game.name.iso")]
        [TestCase(@"C:\Test\Game Name (Europe)\Game Name (Europe) (Track 1).bin")]
        [TestCase(@"C:\Test\Game Name\setup_game_name_1.2.3_(12345).exe")]
        [TestCase(@"C:\Test\Demon Souls (USA)\Demon Souls (USA).iso")]
        public void should_not_be_sample(string path)
        {
            Subject.IsSample(_game, path.AsOsAgnostic()).Should().Be(DetectSampleResult.NotSample);
        }

        [TestCase(@"C:\Test\Game.Name-CODEX\sample.iso")]
        [TestCase(@"C:\Test\Game.Name-CODEX\game.name-sample.zip")]
        [TestCase(@"C:\Test\Game Name\Game Name (Demo).nds")]
        [TestCase(@"C:\Test\Game.Name-GROUP\Sample\game.name.iso")]
        public void should_be_sample(string path)
        {
            Subject.IsSample(_game, path.AsOsAgnostic()).Should().Be(DetectSampleResult.Sample);
        }

        [Test]
        public void should_not_be_sample_if_game_title_has_demo()
        {
            _game.Title = "Demo Game";

            Subject.IsSample(_game, @"C:\Test\Demo Game (USA).sfc".AsOsAgnostic()).Should().Be(DetectSampleResult.NotSample);
        }
    }
}
