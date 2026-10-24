using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ReleaseDetailsParserFixture : CoreTest
    {
        [TestCase("Elden.Ring-CODEX", ReleaseType.Base)]
        [TestCase("Super Mario World (USA).sfc", ReleaseType.Base)]
        [TestCase("Baldurs.Gate.3.Update.v4.1.1-TENOKE", ReleaseType.Update)]
        [TestCase("Mario.Kart.8.Deluxe.Update.v3.0.1.NSW-VENOM", ReleaseType.Update)]
        [TestCase("Lies.of.P.Crackfix-RUNE", ReleaseType.Update)]
        [TestCase("Cyberpunk.2077.Patch.v2.1-GOG", ReleaseType.Update)]
        [TestCase("Animal Crossing New Horizons [01006F8002326800][v1966080][UPDATE].nsp", ReleaseType.Update)]
        [TestCase("Animal Crossing New Horizons [01006F8002326800][v1966080].nsp", ReleaseType.Update)]
        [TestCase("Factorio.Space.Age.DLC-TENOKE", ReleaseType.Dlc)]
        [TestCase("Mario Kart 8 Deluxe Booster Course Pass [0100152000023001][v0][DLC].nsp", ReleaseType.Dlc)]
        [TestCase("Mario Kart 8 Deluxe Booster Course Pass [0100152000023001][v0].nsp", ReleaseType.Dlc)]
        [TestCase("Cyberpunk 2077: Ultimate Edition (v2.12 + All DLCs + Bonus Content, MULTi18) [FitGirl Repack]", ReleaseType.Bundle)]
        [TestCase("Baldur's Gate 3 - Digital Deluxe Edition (v4.1.1 + 3 DLCs) [DODI Repack]", ReleaseType.Bundle)]
        [TestCase("Stellaris.Galaxy.Edition.incl.DLC-ElAmigos", ReleaseType.Bundle)]
        [TestCase("Elden Ring (v1.10 + Update 1.10.1) [FitGirl Repack]", ReleaseType.Base)]
        [TestCase("Super Mario World (USA) (Hack).sfc", ReleaseType.Hack)]
        [TestCase("Super Mario World [h1C].smc", ReleaseType.Hack)]
        [TestCase("Kaizo Mario World (Super Mario World Hack) (USA).sfc", ReleaseType.Hack)]
        [TestCase("Mother 3 (Japan) [T+Eng1.3_Tomato].gba", ReleaseType.Translation)]
        [TestCase("Seiken Densetsu 3 (Japan) [T-Eng].sfc", ReleaseType.Translation)]
        [TestCase("Some Homebrew Game (World) (Homebrew).gba", ReleaseType.Homebrew)]
        [TestCase("Some Game (PD).nes", ReleaseType.Homebrew)]
        [TestCase("Super Mario 64 (USA) (Demo).z64", ReleaseType.Demo)]
        [TestCase("Some Game (Europe) (Kiosk).iso", ReleaseType.Demo)]
        [TestCase("Some Game (USA) (Sample).gba", ReleaseType.Demo)]
        [TestCase("Some.Game.Demo-GROUP", ReleaseType.Demo)]
        [TestCase("Star Fox 2 (Japan) (Proto).sfc", ReleaseType.Prototype)]
        [TestCase("Some Game (USA) (Beta 2).gba", ReleaseType.Prototype)]
        [TestCase("Some Game (Europe) (Proto 1).md", ReleaseType.Prototype)]
        public void should_parse_release_type(string postTitle, ReleaseType releaseType)
        {
            Parser.Parser.ParseGameTitle(postTitle).ReleaseType.Should().Be(releaseType);
        }

        [TestCase("Cyberpunk.2077.v1.5-CODEX", "1.5")]
        [TestCase("Baldurs.Gate.3.Update.v4.1.1-TENOKE", "4.1.1")]
        [TestCase("Hades.II.v0.92596-TENOKE", "0.92596")]
        [TestCase("Frostpunk.2.Build.15634672-TENOKE", "Build 15634672")]
        [TestCase("Stardew_Valley_v1.6.8-DINOByTES", "1.6.8")]
        [TestCase("Hollow Knight v1.5.78.11833 [GOG]", "1.5.78.11833")]
        [TestCase("Cyberpunk 2077: Ultimate Edition (v2.12 + All DLCs + Bonus Content, MULTi18) [FitGirl Repack]", "2.12")]
        [TestCase("setup_cyberpunk_2077_2.12_(73404).exe", "2.12")]
        [TestCase("setup_the_witcher_3_wild_hunt_goty_1.32_(a)_(10706).exe", "1.32")]
        [TestCase("Some Game (USA) (v1.1).gba", "1.1")]
        [TestCase("Animal Crossing New Horizons [01006F8002326800][v1966080][UPDATE].nsp", "1966080")]
        [TestCase("The Legend of Zelda Breath of the Wild [01007EF00011E000][v0][US].nsp", null)]
        [TestCase("Elden.Ring-CODEX", null)]
        [TestCase("Super Mario World (USA).sfc", null)]
        public void should_parse_version(string postTitle, string version)
        {
            Parser.Parser.ParseGameTitle(postTitle).Version.Should().Be(version);
        }

        [TestCase("Final Fantasy VII (USA) (Disc 1).chd", 1, 0)]
        [TestCase("Metal Gear Solid (Europe) (En,Fr,De,Es,It) (Disc 2).cue", 2, 0)]
        [TestCase("Some Game (1995)(Publisher)(Disc 2 of 3).iso", 2, 3)]
        [TestCase("Some.Game.CD1-GROUP", 1, 0)]
        [TestCase("Some Game (USA) (2 Discs).zip", 0, 2)]
        [TestCase("Super Mario World (USA).sfc", 0, 0)]
        public void should_parse_disc(string postTitle, int disc, int count)
        {
            var result = Parser.Parser.ParseGameTitle(postTitle);

            result.DiscNumber.Should().Be(disc);
            result.DiscCount.Should().Be(count);
        }

        [TestCase("Hogwarts.Legacy.Deluxe.Edition-RUNE", "Deluxe Edition")]
        [TestCase("Sekiro.Shadows.Die.Twice.GOTY.Edition-FLT", "GOTY Edition")]
        [TestCase("The.Witcher.3.Wild.Hunt.Game.of.the.Year.Edition-GOG", "Game of the Year Edition")]
        [TestCase("Dead.Space.Digital.Deluxe.Edition.READ.NFO-RUNE", "Digital Deluxe Edition")]
        [TestCase("Elden Ring - Deluxe Edition [FitGirl Repack]", "Deluxe Edition")]
        [TestCase("Cyberpunk 2077: Ultimate Edition (v2.12 + All DLCs) [FitGirl Repack]", "Ultimate Edition")]
        [TestCase("Death.Stranding.Directors.Cut-FLT", "Directors Cut")]
        [TestCase("Disco.Elysium.The.Final.Cut-PROPHET", "The Final Cut")]
        [TestCase("Skyrim.Special.Edition-CODEX", "Special Edition")]
        [TestCase("Some.Game.Complete.Edition.NSW-VENOM", "Complete Edition")]
        [TestCase("Some.Game.10th.Anniversary.Edition-GOG", "10th Anniversary Edition")]
        [TestCase("Elden.Ring-CODEX", null)]
        [TestCase("Super Mario World (USA).sfc", null)]
        public void should_parse_edition(string postTitle, string edition)
        {
            Parser.Parser.ParseGameTitle(postTitle).Edition.Should().Be(edition);
        }
    }
}
