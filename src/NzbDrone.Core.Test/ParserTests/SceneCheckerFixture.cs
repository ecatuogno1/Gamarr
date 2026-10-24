using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class SceneCheckerFixture
    {
        [TestCase("Elden.Ring-CODEX")]
        [TestCase("The.Legend.of.Zelda.Tears.of.the.Kingdom.NSW-VENOM")]
        [TestCase("Bloodborne_PS4-DUPLEX")]
        public void should_return_true_for_scene_names(string title)
        {
            SceneChecker.IsSceneTitle(title).Should().BeTrue();
        }

        [TestCase("Super Mario World (USA)")]
        [TestCase("Something")]
        [TestCase("86de66b7ef385e2fa56a3e41b98481ea1658bfab")]
        [TestCase("Elden.Ring-", Description = "no group")]
        [TestCase("Some.Game-GROUP", Description = "no quality")]
        public void should_return_false_for_non_scene_names(string title)
        {
            SceneChecker.IsSceneTitle(title).Should().BeFalse();
        }

        [TestCase("Elden.Ring-CODEX_", "Elden.Ring-CODEX", Description = "underscore at the end")]
        [TestCase("Bloodborne_PS4-DUPLEX.nzb", "Bloodborne_PS4-DUPLEX", Description = "file extension")]
        [TestCase("The.Legend.of.Zelda.【NSW】-VENOM", "The.Legend.of.Zelda.[NSW]-VENOM", Description = "brackets")]
        public void should_correctly_parse_scene_names(string title, string result)
        {
            SceneChecker.GetSceneTitle(title).Should().Be(result);
        }
    }
}
