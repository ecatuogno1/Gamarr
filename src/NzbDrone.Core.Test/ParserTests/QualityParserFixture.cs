using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class QualityParserFixture : CoreTest
    {
        // ROMs
        [TestCase("Super Mario World (USA).sfc", 101)]
        [TestCase("Pokemon - Emerald Version (USA, Europe).gba", 101)]
        [TestCase("Super Mario World (U) [!].smc", 101)]
        [TestCase("Sonic the Hedgehog 2 (W) (REV01) [!].bin", 101)]
        [TestCase("Some Game (No-Intro).nes", 101)]
        [TestCase("Super Mario World (U).smc", 100)]
        [TestCase("Contra (U) [b1].nes", 100)]
        [TestCase("Super Mario World [h1C].smc", 100)]
        [TestCase("Mother 3 (Japan) [T+Eng1.3_Tomato].gba", 100)]
        [TestCase("Some Game (USA) (Hack).sfc", 100)]
        [TestCase("Pokemon.Black.Version.2.NDS-EXiMiUS", 100)]
        [TestCase("Super Mario World [SNES]", 100)]

        // Disc images
        [TestCase("Final Fantasy VII (USA) (Disc 1).chd", 112)]
        [TestCase("Super Smash Bros. Melee (USA) (En,Ja) (Rev 2).rvz", 112)]
        [TestCase("Metroid Prime (USA).gcz", 112)]
        [TestCase("Metal Gear Solid (Europe) (Disc 2).cue", 111)]
        [TestCase("Resident Evil 4 (USA) (Disc 1).iso", 111)]
        [TestCase("Shenmue (Europe) (Disc 1).gdi", 111)]
        [TestCase("Some Game (USA).cdi", 111)]
        [TestCase("Monster Hunter Freedom Unite (USA).cso", 110)]
        [TestCase("Super Mario Galaxy (USA).wbfs", 110)]
        [TestCase("Final Fantasy VII (USA).pbp", 110)]
        [TestCase("Final Fantasy VII (USA) [Redump]", 113)]
        [TestCase("Shadow.of.the.Colossus.USA.PS2.Redump-GROUP", 113)]
        [TestCase("Halo.Reach.XBOX360-COMPLEX", 111)]
        [TestCase("Red.Dead.Redemption.PS3-iMARS", 111)]
        [TestCase("Super.Mario.Galaxy.PAL.WII-SUSHi", 111)]
        [TestCase("Monster.Hunter.Freedom.Unite.USA.PSP-PLAYASiA", 111)]
        [TestCase("Super.Mario.Galaxy.PAL.WBFS.WII-SUSHi", 110)]
        [TestCase("Some.Game.PS2.CHD-GROUP", 112)]

        // Console digital
        [TestCase("The.Legend.of.Zelda.Tears.of.the.Kingdom.NSW-VENOM", 120)]
        [TestCase("Super Mario Odyssey [0100000000010000][v0].xci", 120)]
        [TestCase("The Legend of Zelda Breath of the Wild [01007EF00011E000][v0][US].nsp", 120)]
        [TestCase("Some Game [0100000000010000][v0].nsz", 120)]
        [TestCase("Bloodborne_PS4-DUPLEX", 121)]
        [TestCase("Gran_Turismo_7_PS5-DUPLEX", 121)]
        [TestCase("Some Game (USA).pkg", 121)]
        [TestCase("Some.Game.PSN.PS3-DUPLEX", 121)]
        [TestCase("Persona.4.Golden.PSV-iMARS", 121)]
        [TestCase("Xenoblade.Chronicles.X.WiiU-VENOM", 122)]
        [TestCase("Xenoblade Chronicles X (USA).wua", 122)]
        [TestCase("Pokemon.Ultra.Sun.3DS-CONTRAST", 123)]
        [TestCase("Pokemon X (USA).cia", 123)]
        [TestCase("Pokemon X (USA).3ds", 123)]
        [TestCase("Shadow.Complex.XBLA.XBOX360-GROUP", 124)]
        [TestCase("Some Game [GOD] [XBOX 360]", 124)]

        // PC
        [TestCase("Elden.Ring-CODEX", 133)]
        [TestCase("Baldurs.Gate.3.Update.v4.1.1-TENOKE", 133)]
        [TestCase("Hitman.3.REPACK-SKIDROW", 133)]
        [TestCase("Red.Dead.Redemption.2-EMPRESS", 133)]
        [TestCase("Cyberpunk 2077: Ultimate Edition (v2.12 + All DLCs + Bonus Content, MULTi18) [FitGirl Repack]", 132)]
        [TestCase("Hollow Knight - [DODI Repack]", 132)]
        [TestCase("Doom.Eternal.MULTi10-ElAmigos", 132)]
        [TestCase("Starfield [KaOs Repack]", 132)]
        [TestCase("Sifu [Repack by xatab]", 132)]
        [TestCase("Hollow Knight v1.5.78.11833 [GOG]", 134)]
        [TestCase("The.Witcher.3.Wild.Hunt.Game.of.the.Year.Edition-GOG", 134)]
        [TestCase("setup_cyberpunk_2077_2.12_(73404).exe", 134)]
        [TestCase("gog_stardew_valley_2.0.0.2.sh", 134)]
        [TestCase("Celeste [Steam Rip]", 131)]
        [TestCase("Celeste.SteamRip-GROUP", 131)]
        [TestCase("Terraria v1.4.4.9 [Portable]", 130)]
        [TestCase("Some Game Pre-Installed", 130)]

        // Unknown
        [TestCase("Some Game (USA).zip", 0)]
        [TestCase("Some Game", 0)]
        public void should_parse_quality(string postTitle, int qualityId)
        {
            var result = Parser.Parser.ParseGameTitle(postTitle);

            result.Quality.Quality.Should().Be(Quality.FindById(qualityId));
        }

        [TestCase("Some Game (USA).zip", PlatformParser.Snes, 101)]
        [TestCase("Some Game (U) [b1].zip", PlatformParser.Snes, 100)]
        [TestCase("Some Game (Europe).bin", PlatformParser.Genesis, 101)]
        [TestCase("Some Game (USA).zip", PlatformParser.Ps1, 111)]
        [TestCase("Some Game", PlatformParser.Switch, 120)]
        [TestCase("Some Game", PlatformParser.PcWindows, 0)]
        public void should_parse_quality_with_platform(string postTitle, int platform, int qualityId)
        {
            QualityParser.ParseQuality(postTitle, platform).Quality.Should().Be(Quality.FindById(qualityId));
        }

        [TestCase("Hitman.3.REPACK-SKIDROW", 2, true)]
        [TestCase("Hitman.3.PROPER-SKIDROW", 2, false)]
        [TestCase("Chrono Trigger (USA) (Rev 1).sfc", 2, false)]
        [TestCase("Some Game (USA) (Rev A).sfc", 2, false)]
        [TestCase("Some Game (USA) (Rev B).sfc", 3, false)]
        [TestCase("Super Mario Bros. 3 (USA) (Rev 2).nes", 3, false)]
        [TestCase("Cyberpunk 2077 [FitGirl Repack]", 1, false)]
        [TestCase("Elden.Ring-CODEX", 1, false)]
        public void should_parse_revision(string postTitle, int version, bool isRepack)
        {
            var result = QualityParser.ParseQuality(postTitle);

            result.Revision.Version.Should().Be(version);
            result.Revision.IsRepack.Should().Be(isRepack);
        }

        [TestCase("Elden.Ring.REAL.PROPER-CODEX", 1)]
        [TestCase("Elden.Ring-CODEX", 0)]
        public void should_parse_real(string postTitle, int real)
        {
            QualityParser.ParseQuality(postTitle).Revision.Real.Should().Be(real);
        }

        [Test]
        public void should_detect_quality_from_name_with_name_source()
        {
            var result = QualityParser.ParseQuality("Hollow Knight v1.5.78.11833 [GOG]");

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Name);
        }

        [Test]
        public void should_detect_quality_from_extension_with_extension_source()
        {
            var result = QualityParser.ParseQuality("Final Fantasy VII (USA) (Disc 1).chd");

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Extension);
        }
    }
}
