using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Consumers.EmulationStation;
using NzbDrone.Core.Games;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Metadata.Consumers.EmulationStation
{
    [TestFixture]
    public class FindMetadataFileFixture : CoreTest<EmulationStationMetadata>
    {
        private Game _game;

        [SetUp]
        public void Setup()
        {
            _game = new Game { Path = @"C:\Games\snes\Super Mario World (1990)".AsOsAgnostic() };
        }

        [TestCase("cover.jpg")]
        [TestCase("screenshot.png")]
        [TestCase("fanart.jpg")]
        public void should_return_image_for_media_files(string fileName)
        {
            var path = Path.Combine(_game.Path, "media", fileName);

            var result = Subject.FindMetadataFile(_game, path);

            result.Should().NotBeNull();
            result.Type.Should().Be(MetadataType.GameImage);
        }

        [TestCase("cover.jpg")]
        [TestCase("media/notes.txt")]
        [TestCase("Super Mario World (1990).sfc")]
        public void should_return_null_for_other_files(string relativePath)
        {
            var path = Path.Combine(_game.Path, relativePath);

            Subject.FindMetadataFile(_game, path).Should().BeNull();
        }
    }
}
