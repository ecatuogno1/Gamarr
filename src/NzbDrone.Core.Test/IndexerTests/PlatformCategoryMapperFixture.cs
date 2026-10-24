using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests
{
    [TestFixture]
    public class PlatformCategoryMapperFixture : CoreTest<PlatformCategoryMapper>
    {
        private List<Platform> _platforms;

        [SetUp]
        public void Setup()
        {
            var id = 1;

            _platforms = PlatformDefinitions.All.Select(PlatformDefinitions.Copy).ToList();
            _platforms.ForEach(p => p.Id = id++);

            Mocker.GetMock<IPlatformService>()
                .Setup(s => s.All())
                .Returns(_platforms);
        }

        private Platform Platform(string shortName)
        {
            return _platforms.Single(p => p.ShortName == shortName);
        }

        [Test]
        public void should_map_pc_games_to_pc_windows()
        {
            var platforms = Subject.GetPlatforms(new[] { 4000, 4050 });

            platforms.Should().ContainSingle();
            platforms.Single().IgdbPlatformId.Should().Be(PlatformCategoryMapper.PcWindowsIgdbPlatformId);
        }

        [Test]
        public void should_map_ps3_category_to_ps3()
        {
            Subject.GetPlatformIds(new[] { 1000, 1080 }).Should().BeEquivalentTo(new[] { Platform("PS3").Id });
        }

        [Test]
        public void should_map_wiiware_category_to_wii()
        {
            Subject.GetPlatformIds(new[] { 1060 }).Should().BeEquivalentTo(new[] { Platform("Wii").Id });
        }

        [Test]
        public void should_map_xbox_360_dlc_category_to_xbox_360()
        {
            Subject.GetPlatformIds(new[] { 1070 }).Should().BeEquivalentTo(new[] { Platform("X360").Id });
        }

        [Test]
        public void should_map_shared_category_to_all_platforms_using_it()
        {
            Subject.GetPlatformIds(new[] { 1010 }).Should().BeEquivalentTo(new[] { Platform("NDS").Id, Platform("DSi").Id });
        }

        [Test]
        public void should_not_map_generic_console_category()
        {
            Subject.GetPlatforms(new[] { 1000 }).Should().BeEmpty();
        }

        [Test]
        public void should_not_map_console_other_category()
        {
            Subject.GetPlatforms(new[] { 1000, 1090 }).Should().BeEmpty();
        }

        [Test]
        public void should_not_map_non_game_or_custom_categories()
        {
            Subject.GetPlatforms(new[] { 2000, 2040, 5000, 100045 }).Should().BeEmpty();
        }

        [Test]
        public void should_not_map_empty_categories()
        {
            Subject.GetPlatforms(new List<int>()).Should().BeEmpty();
            Subject.GetPlatforms(null).Should().BeEmpty();
        }

        [Test]
        public void should_be_compatible_without_categories()
        {
            Subject.IsCompatible(Platform("SNES"), new List<int>()).Should().BeTrue();
        }

        [Test]
        public void should_be_compatible_with_generic_console_category()
        {
            Subject.IsCompatible(Platform("SNES"), new[] { 1000 }).Should().BeTrue();
        }

        [Test]
        public void should_not_be_compatible_with_other_console_subcategory()
        {
            Subject.IsCompatible(Platform("SNES"), new[] { 1000, 1080 }).Should().BeFalse();
        }

        [Test]
        public void should_be_compatible_with_own_subcategory()
        {
            Subject.IsCompatible(Platform("PS3"), new[] { 1000, 1080 }).Should().BeTrue();
        }

        [Test]
        public void should_not_be_compatible_with_pc_category_for_console()
        {
            Subject.IsCompatible(Platform("SNES"), new[] { 4000, 4050 }).Should().BeFalse();
        }

        [Test]
        public void should_not_be_compatible_with_console_category_for_pc()
        {
            Subject.IsCompatible(Platform("PC"), new[] { 1000 }).Should().BeFalse();
        }

        [Test]
        public void should_be_compatible_with_pc_games_for_other_computer_platforms()
        {
            Subject.IsCompatible(Platform("Linux"), new[] { 4050 }).Should().BeTrue();
        }
    }
}
