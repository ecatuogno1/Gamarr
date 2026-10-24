using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Platforms;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.PlatformTests
{
    [TestFixture]
    public class PlatformDefinitionsFixture : TestBase
    {
        [Test]
        public void should_have_unique_igdb_platform_ids()
        {
            PlatformDefinitions.All.Select(p => p.IgdbPlatformId).Should().OnlyHaveUniqueItems();
        }

        [Test]
        public void should_have_a_thorough_list()
        {
            PlatformDefinitions.All.Count.Should().BeInRange(60, 100);
        }

        [Test]
        public void should_seed_pc_first()
        {
            PlatformDefinitions.All[0].IgdbPlatformId.Should().Be(6);
            PlatformDefinitions.All[0].FolderName.Should().Be("pc");
        }

        [Test]
        public void should_have_names_folders_and_categories()
        {
            foreach (var platform in PlatformDefinitions.All)
            {
                platform.Name.Should().NotBeNullOrWhiteSpace();
                platform.ShortName.Should().NotBeNullOrWhiteSpace();
                platform.FolderName.Should().NotBeNullOrWhiteSpace().And.Be(platform.FolderName.ToLowerInvariant());
                platform.NewznabCategories.Should().NotBeEmpty();
                platform.FileExtensions.Where(e => !e.StartsWith(".")).Should().BeEmpty();
                platform.Enabled.Should().BeTrue();
            }
        }

        [TestCase(6, "pc", 4050)]
        [TestCase(18, "nes", 1000)]
        [TestCase(19, "snes", 1000)]
        [TestCase(4, "n64", 1000)]
        [TestCase(21, "gc", 1000)]
        [TestCase(5, "wii", 1030)]
        [TestCase(41, "wiiu", 1130)]
        [TestCase(130, "switch", 1000)]
        [TestCase(508, "switch2", 1000)]
        [TestCase(33, "gb", 1000)]
        [TestCase(22, "gbc", 1000)]
        [TestCase(24, "gba", 1000)]
        [TestCase(20, "nds", 1010)]
        [TestCase(37, "n3ds", 1110)]
        [TestCase(7, "psx", 1000)]
        [TestCase(8, "ps2", 1000)]
        [TestCase(9, "ps3", 1080)]
        [TestCase(48, "ps4", 1180)]
        [TestCase(167, "ps5", 1000)]
        [TestCase(38, "psp", 1020)]
        [TestCase(46, "psvita", 1120)]
        [TestCase(11, "xbox", 1040)]
        [TestCase(12, "xbox360", 1050)]
        [TestCase(49, "xboxone", 1140)]
        [TestCase(29, "megadrive", 1000)]
        [TestCase(78, "segacd", 1000)]
        [TestCase(30, "sega32x", 1000)]
        [TestCase(32, "saturn", 1000)]
        [TestCase(23, "dreamcast", 1000)]
        [TestCase(35, "gamegear", 1000)]
        [TestCase(64, "mastersystem", 1000)]
        [TestCase(52, "arcade", 1000)]
        [TestCase(80, "neogeo", 1000)]
        [TestCase(59, "atari2600", 1000)]
        [TestCase(86, "pcengine", 1000)]
        public void should_have_expected_platform(int igdbPlatformId, string folderName, int newznabCategory)
        {
            var platform = PlatformDefinitions.Get(igdbPlatformId);

            platform.Should().NotBeNull();
            platform.FolderName.Should().Be(folderName);
            platform.NewznabCategories.Should().Contain(newznabCategory);
        }

        [Test]
        public void get_should_return_a_copy()
        {
            var platform = PlatformDefinitions.Get(6);
            platform.Enabled = false;
            platform.FileExtensions.Add(".test");

            PlatformDefinitions.Get(6).Enabled.Should().BeTrue();
            PlatformDefinitions.Get(6).FileExtensions.Should().NotContain(".test");
        }
    }
}
