using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Games;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetGameFixture : CoreTest<ParsingService>
    {
        [Test]
        public void should_use_passed_in_title_when_it_cannot_be_parsed()
        {
            const string title = "#$%";

            Subject.GetGame(title);

            Mocker.GetMock<IGameService>()
                  .Verify(s => s.FindByTitle(title), Times.Once());
        }

        [Test]
        public void should_use_parsed_game_title()
        {
            const string title = "Elden.Ring-CODEX";

            Subject.GetGame(title);

            Mocker.GetMock<IGameService>()
                .Verify(s => s.FindByTitle(It.Is<List<string>>(t => t.SequenceEqual(new[] { "Elden Ring" })), null, It.IsAny<List<string>>(), It.IsAny<List<Game>>()), Times.Once());
        }

        [Test]
        public void should_use_parsed_year()
        {
            const string title = "Doom (2016) [FitGirl Repack]";

            Subject.GetGame(title);

            Mocker.GetMock<IGameService>()
                .Verify(s => s.FindByTitle(It.IsAny<List<string>>(), 2016, It.IsAny<List<string>>(), It.IsAny<List<Game>>()), Times.Once());
        }
    }
}
