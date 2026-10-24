using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ReleaseGroupParserFixture : CoreTest
    {
        [TestCase("Elden.Ring-CODEX", "CODEX")]
        [TestCase("Hogwarts.Legacy.Deluxe.Edition-RUNE", "RUNE")]
        [TestCase("Baldurs.Gate.3.Update.v4.1.1-TENOKE", "TENOKE")]
        [TestCase("Half-Life.2-RAZOR1911", "RAZOR1911")]
        [TestCase("Marvels.Spider-Man.Remastered-FLT", "FLT")]
        [TestCase("Doom.Eternal.MULTi10-ElAmigos", "ElAmigos")]
        [TestCase("Stardew_Valley_v1.6.8-DINOByTES", "DINOByTES")]
        [TestCase("The.Legend.of.Zelda.Tears.of.the.Kingdom.NSW-VENOM", "VENOM")]
        [TestCase("Bloodborne_PS4-DUPLEX", "DUPLEX")]
        [TestCase("Red.Dead.Redemption.PS3-iMARS", "iMARS")]
        [TestCase("Halo.Reach.XBOX360-COMPLEX", "COMPLEX")]
        [TestCase("Some.Game.v1.0-I_KnoW", "I_KnoW")]
        [TestCase("Cyberpunk 2077: Ultimate Edition (v2.12 + All DLCs + Bonus Content, MULTi18) [FitGirl Repack]", "FitGirl")]
        [TestCase("Hollow Knight - [DODI Repack]", "DODI")]
        [TestCase("Starfield [KaOs Repack]", "KaOs")]
        [TestCase("Sifu [Repack by xatab]", "xatab")]
        [TestCase("Dying Light 2 Stay Human [ElAmigos]", "ElAmigos")]
        [TestCase("Hollow Knight v1.5.78.11833 [GOG]", "GOG")]
        [TestCase("setup_cyberpunk_2077_2.12_(73404).exe", "GOG")]
        [TestCase("Super Mario World (USA).sfc", null)]
        [TestCase("Super Mario World (U) [!].smc", null)]
        [TestCase("Spider-Man (USA).iso", null)]
        [TestCase("Marvel's Spider-Man", null)]
        [TestCase("Half-Life", null)]
        [TestCase("Some.Game.v1.2-3", null)]
        [TestCase("Game.Name.NSW-USA", null)]
        public void should_parse_release_group(string title, string expected)
        {
            ReleaseGroupParser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase(@"C:\Downloads\Elden.Ring-CODEX\codex-eldenring.iso", "CODEX")]
        [TestCase(@"C:\Downloads\Some.Game.NSW-VENOM\Some.Game.NSW-VENOM.nsp", "VENOM")]
        public void should_parse_release_group_from_path(string path, string expected)
        {
            ReleaseGroupParser.ParseReleaseGroup(path.AsOsAgnostic()).Should().Be(expected);
        }
    }
}
