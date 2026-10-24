using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.CustomFormats.Specifications.GameRelease
{
    [TestFixture]
    public class GameReleaseSpecificationsFixture : CoreTest
    {
        private static CustomFormatInput Input(string title, NzbDrone.Core.Games.Game game = null)
        {
            return new CustomFormatInput
            {
                GameInfo = Parser.Parser.ParseGameTitle(title),
                Game = game
            };
        }

        [TestCase("Super Mario World (USA).sfc", Region.USA, true)]
        [TestCase("Pokemon - Emerald Version (USA, Europe).gba", Region.Europe, true)]
        [TestCase("Tetris (World).gb", Region.Japan, true)]
        [TestCase("Super Mario World (Japan).sfc", Region.USA, false)]
        [TestCase("Elden.Ring-CODEX", Region.USA, false)]
        public void region_should_match(string title, Region region, bool expected)
        {
            new RegionSpecification { Value = (int)region }.IsSatisfiedBy(Input(title)).Should().Be(expected);
        }

        [Test]
        public void region_should_not_match_world_when_excluded()
        {
            new RegionSpecification { Value = (int)Region.Japan, IncludeWorld = false }.IsSatisfiedBy(Input("Tetris (World).gb")).Should().BeFalse();
        }

        [Test]
        public void region_should_negate()
        {
            new RegionSpecification { Value = (int)Region.Japan, Negate = true }.IsSatisfiedBy(Input("Super Mario World (USA).sfc")).Should().BeTrue();
        }

        [TestCase("Baldurs.Gate.3.Update.v4.1.1-TENOKE", ReleaseType.Update, true)]
        [TestCase("Elden.Ring-CODEX", ReleaseType.Base, true)]
        [TestCase("Elden.Ring-CODEX", ReleaseType.Update, false)]
        [TestCase("Mother 3 (Japan) [T+Eng1.3_Tomato].gba", ReleaseType.Translation, true)]
        public void release_type_should_match(string title, ReleaseType releaseType, bool expected)
        {
            new ReleaseTypeSpecification { Value = (int)releaseType }.IsSatisfiedBy(Input(title)).Should().Be(expected);
        }

        [TestCase("Bloodborne_PS4-DUPLEX", PlatformParser.Ps4, true)]
        [TestCase("Bloodborne_PS4-DUPLEX", PlatformParser.Ps5, false)]
        [TestCase("Super Mario World (USA).sfc", PlatformParser.SuperFamicom, true)]
        public void platform_should_match_release_platform(string title, int platform, bool expected)
        {
            new PlatformSpecification { Value = platform }.IsSatisfiedBy(Input(title)).Should().Be(expected);
        }

        [Test]
        public void platform_should_fall_back_to_game_platform()
        {
            var game = new NzbDrone.Core.Games.Game { Platform = new Platform { IgdbPlatformId = PlatformParser.Ps1 } };

            new PlatformSpecification { Value = PlatformParser.Ps1 }.IsSatisfiedBy(Input("Final Fantasy VII (USA) (Disc 1).chd", game)).Should().BeTrue();
        }

        [TestCase("Super Mario World (USA).sfc", QualitySource.CARTRIDGE, true)]
        [TestCase("Final Fantasy VII (USA) (Disc 1).chd", QualitySource.DISC, true)]
        [TestCase("Bloodborne_PS4-DUPLEX", QualitySource.DIGITAL, true)]
        [TestCase("Elden.Ring-CODEX", QualitySource.PC, true)]
        [TestCase("Elden.Ring-CODEX", QualitySource.DISC, false)]
        public void source_should_match(string title, QualitySource source, bool expected)
        {
            new SourceSpecification { Value = (int)source }.IsSatisfiedBy(Input(title)).Should().Be(expected);
        }

        [Test]
        public void region_select_options_should_not_include_unknown()
        {
            new RegionFieldConverter().GetSelectOptions().Should().NotContain(o => o.Value == 0);
        }

        [Test]
        public void platform_select_options_should_use_igdb_ids()
        {
            new PlatformFieldConverter().GetSelectOptions().Should().Contain(o => o.Value == PlatformParser.Switch && o.Name == "Nintendo Switch");
        }

        [Test]
        public void invalid_region_should_fail_validation()
        {
            new RegionSpecification { Value = 999 }.Validate().IsValid.Should().BeFalse();
            new RegionSpecification { Value = (int)Region.USA }.Validate().IsValid.Should().BeTrue();
        }
    }
}
