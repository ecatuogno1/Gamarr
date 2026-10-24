using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class HashedReleaseFixture : CoreTest
    {
        public static object[] HashedReleaseParserCases =
        {
            new object[]
            {
                @"C:\Test\Elden.Ring-CODEX\0e895c37245186812cb08aab1529cf8ee389dd05.iso".AsOsAgnostic(),
                "Elden Ring",
                Quality.PcScene,
                "CODEX"
            },
            new object[]
            {
                @"C:\Test\Some.Game.NSW-VENOM\AHFMZXGHEWD660.nsp".AsOsAgnostic(),
                "Some Game",
                Quality.NspXci,
                "VENOM"
            },
            new object[]
            {
                @"C:\Test\Bloodborne_PS4-DUPLEX\Backup_72023S02-12.pkg".AsOsAgnostic(),
                "Bloodborne",
                Quality.Pkg,
                "DUPLEX"
            },
            new object[]
            {
                @"C:\Test\Final Fantasy VII (USA)\123.chd".AsOsAgnostic(),
                "Final Fantasy VII",
                Quality.DiscChdRvz,
                null
            },
            new object[]
            {
                @"C:\Test\Halo.Reach.XBOX360-COMPLEX\abc.iso".AsOsAgnostic(),
                "Halo Reach",
                Quality.DiscIso,
                "COMPLEX"
            }
        };

        [Test]
        [TestCaseSource(nameof(HashedReleaseParserCases))]
        public void should_properly_parse_hashed_releases(string path, string title, Quality quality, string releaseGroup)
        {
            var result = Parser.Parser.ParseGamePath(path);
            result.PrimaryGameTitle.Should().Be(title);
            result.Quality.Quality.Should().Be(quality);
            result.ReleaseGroup.Should().Be(releaseGroup);
        }
    }
}
