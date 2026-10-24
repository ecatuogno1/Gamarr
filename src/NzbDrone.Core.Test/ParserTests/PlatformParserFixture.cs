using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class PlatformParserFixture : CoreTest
    {
        [TestCase("The.Legend.of.Zelda.Tears.of.the.Kingdom.NSW-VENOM", PlatformParser.Switch)]
        [TestCase("Bloodborne_PS4-DUPLEX", PlatformParser.Ps4)]
        [TestCase("Gran_Turismo_7_PS5-DUPLEX", PlatformParser.Ps5)]
        [TestCase("Red.Dead.Redemption.PS3-iMARS", PlatformParser.Ps3)]
        [TestCase("Shadow.of.the.Colossus.PS2.NTSC-U", PlatformParser.Ps2)]
        [TestCase("Halo.Reach.XBOX360-COMPLEX", PlatformParser.Xbox360)]
        [TestCase("Forza.Horizon.Xbox.360-GROUP", PlatformParser.Xbox360)]
        [TestCase("Pokemon.Black.Version.2.NDS-EXiMiUS", PlatformParser.Nds)]
        [TestCase("Monster.Hunter.Freedom.Unite.USA.PSP-PLAYASiA", PlatformParser.Psp)]
        [TestCase("Super.Mario.Galaxy.PAL.WII-SUSHi", PlatformParser.Wii)]
        [TestCase("Wii.Sports.Resort.USA.WII-WiiERD", PlatformParser.Wii)]
        [TestCase("Xenoblade.Chronicles.X.WiiU-VENOM", PlatformParser.WiiU)]
        [TestCase("Xenoblade.Chronicles.X.Wii.U-VENOM", PlatformParser.WiiU)]
        [TestCase("Pokemon.Ultra.Sun.3DS-CONTRAST", PlatformParser.N3ds)]
        [TestCase("Persona.4.Golden.PSV-iMARS", PlatformParser.PsVita)]
        [TestCase("Metroid.Prime.NGC-TLS", PlatformParser.GameCube)]
        [TestCase("Elden.Ring-CODEX", PlatformParser.PcWindows)]
        [TestCase("Hollow Knight - [DODI Repack]", PlatformParser.PcWindows)]
        [TestCase("Hollow Knight v1.5.78.11833 [GOG]", PlatformParser.PcWindows)]
        [TestCase("Celeste.Linux-DINOByTES", PlatformParser.Linux)]
        [TestCase("setup_cyberpunk_2077_2.12_(73404).exe", PlatformParser.PcWindows)]
        [TestCase("gog_stardew_valley_2.0.0.2.sh", PlatformParser.Linux)]
        [TestCase("Super Mario World (USA).sfc", PlatformParser.Snes)]
        [TestCase("Super Mario World (U) [!].smc", PlatformParser.Snes)]
        [TestCase("Super Mario Bros. 3 (USA) (Rev 1).nes", PlatformParser.Nes)]
        [TestCase("GoldenEye 007 (USA).z64", PlatformParser.N64)]
        [TestCase("Pokemon - Emerald Version (USA, Europe).gba", PlatformParser.GameBoyAdvance)]
        [TestCase("Tetris (World) (Rev 1).gb", PlatformParser.GameBoy)]
        [TestCase("Sonic the Hedgehog (USA, Europe).md", PlatformParser.Genesis)]
        [TestCase("Shenmue (Europe) (Disc 1).gdi", PlatformParser.Dreamcast)]
        [TestCase("Super Mario Odyssey [0100000000010000][v0].xci", PlatformParser.Switch)]
        [TestCase("Xenoblade Chronicles X (USA).wua", PlatformParser.WiiU)]
        [TestCase("Pokemon X (USA).3ds", PlatformParser.N3ds)]
        [TestCase("Persona 4 Golden (USA) [PS Vita]", PlatformParser.PsVita)]
        [TestCase("Halo 3 [XBOX 360]", PlatformParser.Xbox360)]
        [TestCase("Spyro the Dragon (USA) [PS1]", PlatformParser.Ps1)]
        [TestCase("Metroid Prime (USA) (GameCube)", PlatformParser.GameCube)]
        [TestCase("Chrono Trigger [SNES]", PlatformParser.Snes)]
        [TestCase("Final Fantasy VII (USA) (Disc 1).chd", 0)]
        [TestCase("Resident Evil 4 (USA) (Disc 1).iso", 0)]
        [TestCase("Some Game (USA).zip", 0)]
        public void should_parse_platform(string postTitle, int platform)
        {
            Parser.Parser.ParseGameTitle(postTitle).PlatformIgdbId.Should().Be(platform);
        }

        [TestCase("NSW", PlatformParser.Switch)]
        [TestCase("ps4", PlatformParser.Ps4)]
        [TestCase("PSX", PlatformParser.Ps1)]
        [TestCase("PS1", PlatformParser.Ps1)]
        [TestCase("GC", PlatformParser.GameCube)]
        [TestCase("NGC", PlatformParser.GameCube)]
        [TestCase("N64", PlatformParser.N64)]
        [TestCase("SNES", PlatformParser.Snes)]
        [TestCase("SFC", PlatformParser.Snes)]
        [TestCase("MD", PlatformParser.Genesis)]
        [TestCase("Genesis", PlatformParser.Genesis)]
        [TestCase("Mega Drive", PlatformParser.Genesis)]
        [TestCase("Something", 0)]
        public void should_parse_platform_alias(string alias, int platform)
        {
            PlatformParser.ParseAlias(alias).Should().Be(platform);
        }

        [TestCase(".nsp", PlatformParser.Switch)]
        [TestCase(".sfc", PlatformParser.Snes)]
        [TestCase(".gba", PlatformParser.GameBoyAdvance)]
        [TestCase(".cia", PlatformParser.N3ds)]
        [TestCase(".iso", 0)]
        [TestCase(".chd", 0)]
        [TestCase(".zip", 0)]
        public void should_parse_platform_from_extension(string extension, int platform)
        {
            PlatformParser.ParseExtension(extension).Should().Be(platform);
        }

        [TestCase(PlatformParser.Snes, PlatformParser.SuperFamicom, true)]
        [TestCase(PlatformParser.Nes, PlatformParser.Famicom, true)]
        [TestCase(PlatformParser.Nds, PlatformParser.Dsi, true)]
        [TestCase(PlatformParser.N3ds, PlatformParser.New3ds, true)]
        [TestCase(PlatformParser.Snes, PlatformParser.Nes, false)]
        [TestCase(PlatformParser.Ps4, PlatformParser.Ps5, false)]
        public void should_know_compatible_platforms(int platform, int otherPlatform, bool compatible)
        {
            PlatformParser.AreCompatible(platform, otherPlatform).Should().Be(compatible);
        }
    }
}
