using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class GameTitleParserFixture : CoreTest
    {
        // Scene PC releases
        [TestCase("Cyberpunk.2077.v1.5-CODEX", "Cyberpunk 2077")]
        [TestCase("Elden.Ring-CODEX", "Elden Ring")]
        [TestCase("Hogwarts.Legacy.Deluxe.Edition-RUNE", "Hogwarts Legacy")]
        [TestCase("Baldurs.Gate.3.Update.v4.1.1-TENOKE", "Baldurs Gate 3")]
        [TestCase("Red.Dead.Redemption.2-EMPRESS", "Red Dead Redemption 2")]
        [TestCase("Sekiro.Shadows.Die.Twice.GOTY.Edition-FLT", "Sekiro Shadows Die Twice")]
        [TestCase("The.Witcher.3.Wild.Hunt.Game.of.the.Year.Edition-GOG", "The Witcher 3 Wild Hunt")]
        [TestCase("S.T.A.L.K.E.R.2.Heart.of.Chornobyl-RUNE", "S.T.A.L.K.E.R. 2 Heart of Chornobyl")]
        [TestCase("Doom.Eternal.MULTi10-ElAmigos", "Doom Eternal")]
        [TestCase("Hitman.3.REPACK-SKIDROW", "Hitman 3")]
        [TestCase("Disco.Elysium.The.Final.Cut-PROPHET", "Disco Elysium")]
        [TestCase("Half-Life.2-RAZOR1911", "Half-Life 2")]
        [TestCase("Marvels.Spider-Man.Remastered-FLT", "Marvels Spider-Man Remastered")]
        [TestCase("Frostpunk.2.Build.15634672-TENOKE", "Frostpunk 2")]
        [TestCase("Hades.II.v0.92596-TENOKE", "Hades II")]
        [TestCase("Celeste.Linux-DINOByTES", "Celeste")]
        [TestCase("Stardew_Valley_v1.6.8-DINOByTES", "Stardew Valley")]
        [TestCase("Dead.Space.Digital.Deluxe.Edition.READ.NFO-RUNE", "Dead Space")]
        [TestCase("Lies.of.P.Crackfix-RUNE", "Lies of P")]
        [TestCase("Factorio.Space.Age.DLC-TENOKE", "Factorio Space Age")]

        // Console scene releases
        [TestCase("The.Legend.of.Zelda.Tears.of.the.Kingdom.NSW-VENOM", "The Legend of Zelda Tears of the Kingdom")]
        [TestCase("Super.Mario.Bros.Wonder.NSW-SUXXORS", "Super Mario Bros Wonder")]
        [TestCase("Bloodborne_PS4-DUPLEX", "Bloodborne")]
        [TestCase("God_of_War_Ragnarok_PS4-DUPLEX", "God of War Ragnarok")]
        [TestCase("Red.Dead.Redemption.PS3-iMARS", "Red Dead Redemption")]
        [TestCase("Halo.Reach.XBOX360-COMPLEX", "Halo Reach")]
        [TestCase("Gears.of.War.3.PAL.XBOX360-COMPLEX", "Gears of War 3")]
        [TestCase("Pokemon.Black.Version.2.NDS-EXiMiUS", "Pokemon Black Version 2")]
        [TestCase("Monster.Hunter.Freedom.Unite.USA.PSP-PLAYASiA", "Monster Hunter Freedom Unite")]
        [TestCase("Super.Mario.Galaxy.PAL.WII-SUSHi", "Super Mario Galaxy")]
        [TestCase("Wii.Sports.Resort.USA.WII-WiiERD", "Wii Sports Resort")]
        [TestCase("Nintendo.Switch.Sports.NSW-VENOM", "Nintendo Switch Sports")]
        [TestCase("Xenoblade.Chronicles.X.WiiU-VENOM", "Xenoblade Chronicles X")]
        [TestCase("Pokemon.Ultra.Sun.3DS-CONTRAST", "Pokemon Ultra Sun")]
        [TestCase("Persona.4.Golden.PSV-iMARS", "Persona 4 Golden")]
        [TestCase("Metroid.Prime.NGC-TLS", "Metroid Prime")]
        [TestCase("Gran_Turismo_7_PS5-DUPLEX", "Gran Turismo 7")]
        [TestCase("Mario.Kart.8.Deluxe.Update.v3.0.1.NSW-VENOM", "Mario Kart 8 Deluxe")]
        [TestCase("Shadow.of.the.Colossus.PS2.NTSC-U", "Shadow of the Colossus")]

        // No-Intro / Redump / GoodTools / TOSEC
        [TestCase("Super Mario World (USA).sfc", "Super Mario World")]
        [TestCase("Legend of Zelda, The - A Link to the Past (USA).sfc", "The Legend of Zelda - A Link to the Past")]
        [TestCase("Pokemon - Emerald Version (USA, Europe).gba", "Pokemon - Emerald Version")]
        [TestCase("Sonic the Hedgehog (USA, Europe).md", "Sonic the Hedgehog")]
        [TestCase("Final Fantasy VII (USA) (Disc 1).chd", "Final Fantasy VII")]
        [TestCase("Metal Gear Solid (Europe) (En,Fr,De,Es,It) (Disc 2).cue", "Metal Gear Solid")]
        [TestCase("Chrono Trigger (USA) (Rev 1).sfc", "Chrono Trigger")]
        [TestCase("Super Mario Bros. 3 (USA) (Rev 1).nes", "Super Mario Bros. 3")]
        [TestCase("GoldenEye 007 (USA).z64", "GoldenEye 007")]
        [TestCase("Castlevania - Symphony of the Night (USA).cue", "Castlevania - Symphony of the Night")]
        [TestCase("Tekken 3 (Europe) (En,Fr,De).bin", "Tekken 3")]
        [TestCase("Shenmue (Europe) (Disc 1).gdi", "Shenmue")]
        [TestCase("Street Fighter II' - Special Champion Edition (USA).md", "Street Fighter II' - Special Champion Edition")]
        [TestCase("Super Metroid (Japan, USA) (En,Ja).sfc", "Super Metroid")]
        [TestCase("Mother 3 (Japan) [T+Eng1.3_Tomato].gba", "Mother 3")]
        [TestCase("Super Mario World (U) [!].smc", "Super Mario World")]
        [TestCase("Sonic the Hedgehog 2 (W) (REV01) [!].bin", "Sonic the Hedgehog 2")]
        [TestCase("Contra (U) [b1].nes", "Contra")]
        [TestCase("Turrican II - The Final Fight (1991)(Rainbow Arts)(DE)[!].adf", "Turrican II - The Final Fight")]
        [TestCase("Elite (1984)(Acornsoft)[!].ssd", "Elite")]
        [TestCase("Gran Turismo 2 (USA) (Disc 1) (Arcade Mode).chd", "Gran Turismo 2")]
        [TestCase("Resident Evil 4 (USA) (Disc 1).iso", "Resident Evil 4")]
        [TestCase("Super Smash Bros. Melee (USA) (En,Ja) (Rev 2).rvz", "Super Smash Bros. Melee")]
        [TestCase("Zelda no Densetsu - Kamigami no Triforce (Japan).sfc", "Zelda no Densetsu - Kamigami no Triforce")]
        [TestCase("Tetris (World) (Rev 1).gb", "Tetris")]

        // Repacks and store releases
        [TestCase("Cyberpunk 2077: Ultimate Edition (v2.12 + All DLCs + Bonus Content, MULTi18) [FitGirl Repack]", "Cyberpunk 2077")]
        [TestCase("Elden Ring - Deluxe Edition [FitGirl Repack]", "Elden Ring")]
        [TestCase("Hollow Knight - [DODI Repack]", "Hollow Knight")]
        [TestCase("Hollow Knight v1.5.78.11833 [GOG]", "Hollow Knight")]
        [TestCase("Baldur's Gate 3 - Digital Deluxe Edition (v4.1.1 + 3 DLCs) [DODI Repack]", "Baldur's Gate 3")]
        [TestCase("Doom (2016) [FitGirl Repack]", "Doom")]
        [TestCase("The Witcher 3: Wild Hunt - Game of the Year Edition [GOG]", "The Witcher 3: Wild Hunt")]
        [TestCase("Starfield [KaOs Repack]", "Starfield")]
        [TestCase("Sifu [Repack by xatab]", "Sifu")]
        [TestCase("Dying Light 2 Stay Human [ElAmigos]", "Dying Light 2 Stay Human")]
        [TestCase("Terraria v1.4.4.9 [Portable]", "Terraria")]
        [TestCase("Celeste [Steam Rip]", "Celeste")]

        // GOG installers
        [TestCase("setup_the_witcher_3_wild_hunt_goty_1.32_(a)_(10706).exe", "The Witcher 3 Wild Hunt")]
        [TestCase("setup_cyberpunk_2077_2.12_(73404).exe", "Cyberpunk 2077")]
        [TestCase("setup_hollow_knight_1.5.78.11833_(64-bit)_(44711).exe", "Hollow Knight")]
        [TestCase("gog_stardew_valley_2.0.0.2.sh", "Stardew Valley")]

        // Switch dumps
        [TestCase("The Legend of Zelda Breath of the Wild [01007EF00011E000][v0][US].nsp", "The Legend of Zelda Breath of the Wild")]
        [TestCase("Super Mario Odyssey [0100000000010000][v0].xci", "Super Mario Odyssey")]
        [TestCase("Animal Crossing New Horizons [01006F8002326800][v1966080][UPDATE].nsp", "Animal Crossing New Horizons")]
        [TestCase("Mario Kart 8 Deluxe [0100152000022000][v0][US](nsw2u.com).nsp", "Mario Kart 8 Deluxe")]

        // Gamarr's own naming
        [TestCase("Super Mario World (1990) (USA) [v1.1].sfc", "Super Mario World")]
        [TestCase("Half-Life 2 (2004)", "Half-Life 2")]
        public void should_parse_game_title(string postTitle, string title)
        {
            var result = Parser.Parser.ParseGameTitle(postTitle);

            result.Should().NotBeNull();
            result.PrimaryGameTitle.Should().Be(title);
        }

        [TestCase("Turrican II - The Final Fight (1991)(Rainbow Arts)(DE)[!].adf", 1991)]
        [TestCase("Doom (2016) [FitGirl Repack]", 2016)]
        [TestCase("Half-Life 2 (2004)", 2004)]
        [TestCase("Super Mario World (1990) (USA) [v1.1].sfc", 1990)]
        [TestCase("Hitman.2016.v1.0-GROUP", 2016)]
        [TestCase("Super Mario World (USA).sfc", 0)]
        [TestCase("Cyberpunk.2077.v1.5-CODEX", 0)]
        public void should_parse_year(string postTitle, int year)
        {
            Parser.Parser.ParseGameTitle(postTitle).Year.Should().Be(year);
        }

        [Test]
        public void should_add_title_without_scene_year()
        {
            var result = Parser.Parser.ParseGameTitle("Hitman.2016.v1.0-GROUP");

            result.GameTitles.Should().BeEquivalentTo(new[] { "Hitman 2016", "Hitman" });
        }

        [TestCase("Super Mario World (USA).sfc", "Super Mario World (USA)")]
        [TestCase("Elden.Ring-CODEX", "Elden.Ring-CODEX")]
        public void should_keep_release_title_without_extension(string postTitle, string releaseTitle)
        {
            Parser.Parser.ParseGameTitle(postTitle).ReleaseTitle.Should().Be(releaseTitle);
        }

        [TestCase("Game.Name.igdb-1234.NSW-VENOM", 1234)]
        [TestCase("Game Name {igdbid-5678} (USA)", 5678)]
        [TestCase("Game Name (USA)", 0)]
        public void should_parse_igdb_id(string postTitle, int igdbId)
        {
            Parser.Parser.ParseGameTitle(postTitle).IgdbId.Should().Be(igdbId);
        }

        [TestCase("The Legend of Zelda Breath of the Wild [01007EF00011E000][v0][US].nsp", "01007EF00011E000")]
        [TestCase("Super Mario World (USA).sfc", null)]
        public void should_parse_switch_title_id(string postTitle, string titleId)
        {
            Parser.Parser.ParseGameTitle(postTitle).TitleId.Should().Be(titleId);
        }

        [TestCase(@"C:\Games\Downloads\Game.Name.NSW-VENOM\venom-game.name.nsp", "Game Name")]
        [TestCase(@"C:\Games\Downloads\Elden.Ring-CODEX\setup.exe", "Elden Ring")]
        [TestCase(@"C:\Games\Downloads\Final Fantasy VII (USA)\Final Fantasy VII (USA) (Disc 1).cue", "Final Fantasy VII")]
        [TestCase(@"C:\Games\Downloads\Hollow Knight [GOG]\setup_hollow_knight_1.5.78.11833_(44711).exe", "Hollow Knight")]
        [TestCase(@"C:\Games\Downloads\Metroid Prime (USA)\0e895c37245186812cb08aab1529cf8ee389dd05.iso", "Metroid Prime")]
        public void should_parse_game_path(string path, string title)
        {
            Parser.Parser.ParseGamePath(path.AsOsAgnostic()).PrimaryGameTitle.Should().Be(title);
        }
    }
}
