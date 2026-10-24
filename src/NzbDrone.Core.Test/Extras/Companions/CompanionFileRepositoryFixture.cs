using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Companions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Extras.Companions
{
    [TestFixture]
    public class CompanionFileRepositoryFixture : DbTest<CompanionFileRepository, CompanionFile>
    {
        [Test]
        public void should_insert_and_get_companion_files_by_game_file()
        {
            var files = Builder<CompanionFile>.CreateListOfSize(3)
                                              .All()
                                              .With(f => f.Id = 0)
                                              .With(f => f.GameId = 1)
                                              .TheFirst(2)
                                              .With(f => f.GameFileId = 5)
                                              .TheLast(1)
                                              .With(f => f.GameFileId = 6)
                                              .BuildListOfNew();

            Subject.InsertMany(files.ToList());

            Subject.GetFilesByGameFile(5).Should().HaveCount(2);
            Subject.GetFilesByGame(1).Should().HaveCount(3);

            Subject.DeleteForGameFile(5);

            Subject.GetFilesByGame(1).Should().HaveCount(1);
        }
    }
}
