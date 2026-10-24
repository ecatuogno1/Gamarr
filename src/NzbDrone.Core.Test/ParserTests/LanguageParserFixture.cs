using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class LanguageParserFixture : CoreTest
    {
        [TestCase("Metal Gear Solid (Europe) (En,Fr,De,Es,It) (Disc 2).cue", new[] { 1, 2, 4, 3, 5 })]
        [TestCase("Super Metroid (Japan, USA) (En,Ja).sfc", new[] { 1, 8 })]
        [TestCase("Some Game (Europe) (En,Fr,De,Es,It,Nl,Sv).gba", new[] { 1, 2, 4, 3, 5, 7, 14 })]
        [TestCase("Some Game (Germany) (De).sfc", new[] { 4 })]
        [TestCase("Some Game (1992)(Publisher)(DE)(de)[!].adf", new[] { 4 })]
        [TestCase("Some Game (1992)(Publisher)(en-de).adf", new[] { 1, 4 })]
        [TestCase("Mother 3 (Japan) [T+Eng1.3_Tomato].gba", new[] { 1 })]
        [TestCase("Seiken Densetsu 3 (Japan) [T-Spa].sfc", new[] { 3 })]
        [TestCase("Some.Game.GERMAN.NSW-VENOM", new[] { 4 })]
        [TestCase("Some.Game.FRENCH-GROUP", new[] { 2 })]
        [TestCase("Some.Game.ENG.PS2-GROUP", new[] { 1 })]
        [TestCase("Some.Game.Polish-GROUP", new[] { 12 })]
        [TestCase("Some.Game.Japanese.PS4-DUPLEX", new[] { 8 })]
        [TestCase("Some.Game.RUS-GROUP", new[] { 11 })]
        [TestCase("Some.Game.ITA.PS3-GROUP", new[] { 5 })]
        [TestCase("Some Game (USA) (En,Fr,Es,Pt-BR).iso", new[] { 1, 2, 3, 30 })]
        [TestCase("Some Game [CHS] (Asia).nsp", new[] { 10 })]
        [TestCase("Elden.Ring-CODEX", new[] { 0 })]
        [TestCase("Super Mario World (USA).sfc", new[] { 0 })]
        [TestCase("Doom.Eternal.MULTi10-ElAmigos", new[] { 0 })]
        [TestCase("Some Game (USA) (Rev 1).sfc", new[] { 0 })]
        [TestCase("Some Game (U) [b1].nes", new[] { 0 })]
        public void should_parse_languages(string postTitle, int[] languages)
        {
            var result = Parser.Parser.ParseGameTitle(postTitle);

            result.Languages.Should().BeEquivalentTo(languages.Select(l => (Language)l));
        }

        [Test]
        public void should_parse_language_list_directly()
        {
            LanguageParser.ParseLanguages("(En,Fr)").Should().BeEquivalentTo(new[] { Language.English, Language.French });
        }

        [TestCase("Doom.Eternal.MULTi10-ElAmigos", true)]
        [TestCase("Some Game (v1.2, MULTi18) [FitGirl Repack]", true)]
        [TestCase("Some.Game.MULTI.NSW-VENOM", true)]
        [TestCase("Multiversus-CODEX", false)]
        [TestCase("Elden.Ring-CODEX", false)]
        public void should_detect_multiple_languages(string postTitle, bool multi)
        {
            Parser.Parser.HasMultipleLanguages(postTitle).Should().Be(multi);
        }
    }
}
