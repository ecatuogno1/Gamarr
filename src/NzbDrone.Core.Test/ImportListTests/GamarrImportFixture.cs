using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.Gamarr;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests
{
    [TestFixture]
    public class GamarrImportFixture : CoreTest<GamarrImport>
    {
        private List<GamarrGame> _remoteGames;

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new ImportListDefinition { Id = 1, Settings = new GamarrSettings { BaseUrl = "http://remote:7171", ApiKey = "key" } };

            _remoteGames = new List<GamarrGame>();

            Mocker.GetMock<IGamarrV3Proxy>()
                  .Setup(s => s.GetGames(It.IsAny<GamarrSettings>()))
                  .Returns(_remoteGames);

            var snes = new Platform { Id = 4, Slug = "snes", IgdbPlatformId = 19 };
            var pc = new Platform { Id = 1, Slug = "win", IgdbPlatformId = 6 };

            Mocker.GetMock<IPlatformService>()
                  .Setup(s => s.FindBySlug(It.IsAny<string>()))
                  .Returns<string>(slug => new[] { snes, pc }.FirstOrDefault(p => p.Slug == slug));

            Mocker.GetMock<IPlatformService>()
                  .Setup(s => s.FindByIgdbPlatformId(It.IsAny<int>()))
                  .Returns<int>(id => new[] { snes, pc }.FirstOrDefault(p => p.IgdbPlatformId == id));
        }

        private void GivenRemoteGame(int igdbId, GamarrPlatform platform)
        {
            _remoteGames.Add(new GamarrGame { IgdbId = igdbId, Title = $"Game {igdbId}", Platform = platform, Tags = new HashSet<int>(), Path = "/games" });
        }

        [Test]
        public void should_map_remote_platform_by_slug()
        {
            GivenRemoteGame(1070, new GamarrPlatform { Id = 30, IgdbPlatformId = 999, Slug = "snes" });

            Subject.Fetch().Games.Single().PlatformId.Should().Be(4);
        }

        [Test]
        public void should_map_remote_platform_by_igdb_id_without_slug()
        {
            GivenRemoteGame(1070, new GamarrPlatform { Id = 30, IgdbPlatformId = 6 });

            Subject.Fetch().Games.Single().PlatformId.Should().Be(1);
        }

        [Test]
        public void should_leave_platform_empty_for_unknown_remote_platform()
        {
            GivenRemoteGame(1070, new GamarrPlatform { Id = 30, IgdbPlatformId = 12345, Slug = "unknown" });
            GivenRemoteGame(1071, null);

            Subject.Fetch().Games.Should().OnlyContain(g => g.PlatformId == 0);
        }
    }
}
