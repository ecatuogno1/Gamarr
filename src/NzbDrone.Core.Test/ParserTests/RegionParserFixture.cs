using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class RegionParserFixture : CoreTest
    {
        [TestCase("Super Mario World (USA).sfc", new[] { Region.USA })]
        [TestCase("Pokemon - Emerald Version (USA, Europe).gba", new[] { Region.USA, Region.Europe })]
        [TestCase("Super Metroid (Japan, USA) (En,Ja).sfc", new[] { Region.Japan, Region.USA })]
        [TestCase("Tetris (World) (Rev 1).gb", new[] { Region.World })]
        [TestCase("Shenmue (Europe) (Disc 1).gdi", new[] { Region.Europe })]
        [TestCase("Final Fantasy VII (Japan) (Disc 1).chd", new[] { Region.Japan })]
        [TestCase("Ridge Racer (Korea).cue", new[] { Region.Korea })]
        [TestCase("Some Game (Brazil).md", new[] { Region.Brazil })]
        [TestCase("Some Game (Australia).sms", new[] { Region.Australia })]
        [TestCase("Some Game (Germany) (De).sfc", new[] { Region.Germany })]
        [TestCase("Super Mario World (U) [!].smc", new[] { Region.USA })]
        [TestCase("Street Fighter II (JUE) [!].smc", new[] { Region.Japan, Region.USA, Region.Europe })]
        [TestCase("Sonic the Hedgehog 2 (W) (REV01) [!].bin", new[] { Region.World })]
        [TestCase("Some Game (E) [!].sms", new[] { Region.Europe })]
        [TestCase("Turrican II - The Final Fight (1991)(Rainbow Arts)(DE)[!].adf", new[] { Region.Germany })]
        [TestCase("Some Game (1990)(Publisher)(US-EU)[!].d64", new[] { Region.USA, Region.Europe })]
        [TestCase("The Legend of Zelda Breath of the Wild [01007EF00011E000][v0][US].nsp", new[] { Region.USA })]
        [TestCase("Some Game [0100000000010000][v0][EU].nsp", new[] { Region.Europe })]
        [TestCase("Some Game [0100000000010000][v0][JP].nsp", new[] { Region.Japan })]
        [TestCase("Gears.of.War.3.PAL.XBOX360-COMPLEX", new[] { Region.Europe })]
        [TestCase("Monster.Hunter.Freedom.Unite.USA.PSP-PLAYASiA", new[] { Region.USA })]
        [TestCase("Persona.5.JPN.PS4-DUPLEX", new[] { Region.Japan })]
        [TestCase("Some.Game.EUR.NSW-VENOM", new[] { Region.Europe })]
        [TestCase("Shadow.of.the.Colossus.PS2.NTSC-U", new[] { Region.USA })]
        [TestCase("Some.Game.NTSC-J.PS2-GROUP", new[] { Region.Japan })]
        [TestCase("Elden.Ring-CODEX", new Region[0])]
        [TestCase("Metal Gear Solid (Konami) (Disc 1).cue", new Region[0])]
        [TestCase("Some Game (En,Fr,De).sfc", new Region[0])]
        [TestCase("Gangs.of.Usa.NSW-VENOM", new Region[0])]
        public void should_parse_regions(string postTitle, Region[] regions)
        {
            Parser.Parser.ParseGameTitle(postTitle).Regions.Should().BeEquivalentTo(regions);
        }
    }
}
