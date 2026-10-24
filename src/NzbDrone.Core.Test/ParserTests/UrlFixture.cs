using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class UrlFixture : CoreTest
    {
        [TestCase("[www.test.com] - Game.Title.v1.2-CODEX", "Game Title")]
        [TestCase("test.net - Game.Title.v1.2-CODEX", "Game Title")]
        [TestCase("[www.test-hyphen.com] - Game.Title.NSW-VENOM", "Game Title")]
        [TestCase("www.test123.org - Game.Title.NSW-VENOM", "Game Title")]
        [TestCase("[test.co.uk] - Game.Title.PS4-DUPLEX", "Game Title")]
        [TestCase("www.test-hyphen.net.au - Game.Title.PS4-DUPLEX", "Game Title")]
        [TestCase("[www.test123.co.nz] - Game Title (USA) (Disc 1)", "Game Title")]
        [TestCase("test-hyphen123.org.au - Game Title (Europe)", "Game Title")]
        [TestCase("www.test123.co.za - The Game Title Bros. (2023)", "The Game Title Bros.")]
        [TestCase("[www.test-hyphen.ca] - Game Title (2023)", "Game Title")]
        [TestCase("(gameawake.com) Game Title [FitGirl Repack]", "Game Title")]
        public void should_not_parse_url_in_name(string postTitle, string title)
        {
            var result = Parser.Parser.ParseGameTitle(postTitle).GameTitle.CleanGameTitle();
            result.Should().Be(title.CleanGameTitle());
        }

        [TestCase("Game.Title.v1.2-CODEX[www.abb.com]", "CODEX")]
        [TestCase("Game.Title.NSW-VENOM[www.alfaromeo.io]", "VENOM")]
        [TestCase("Game Title (USA)[www.academy.org]", null)]
        public void should_not_parse_url_in_group(string title, string expected)
        {
            ReleaseGroupParser.ParseReleaseGroup(title).Should().Be(expected);
        }
    }
}
