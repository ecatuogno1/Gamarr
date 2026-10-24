using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.MetadataSource.Igdb;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.PlatformTests
{
    // Runs against the seeded platforms with IgdbClient on a mocked HTTP client
    [TestFixture]
    public class PlatformIgdbSyncServiceFixture : DbTest<PlatformIgdbSyncService, Platform>
    {
        private List<object> _remotePlatforms;
        private List<HttpRequest> _apiRequests;

        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IPlatformRepository>(Mocker.Resolve<PlatformRepository>());
            Mocker.SetConstant<NzbDrone.Common.Cache.ICacheManager>(Mocker.Resolve<NzbDrone.Common.Cache.CacheManager>());
            Mocker.SetConstant<IPlatformService>(Mocker.Resolve<PlatformService>());
            Mocker.SetConstant<IIgdbClient>(Mocker.Resolve<IgdbClient>());

            GivenCredentials("client-id", "client-secret");

            // IGDB's platforms as they are, from the seeded definitions
            _remotePlatforms = PlatformDefinitions.All.Select(p => (object)new { id = p.IgdbPlatformId, slug = p.Slug, name = p.Name }).ToList();
            _apiRequests = new List<HttpRequest>();

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Execute(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(request =>
                  {
                      if (request.Url.FullUri.StartsWith(IgdbClient.TokenUrl))
                      {
                          return new HttpResponse(request, new HttpHeader(), "{\"access_token\":\"token\",\"expires_in\":5000000,\"token_type\":\"bearer\"}", HttpStatusCode.OK);
                      }

                      _apiRequests.Add(request);

                      return new HttpResponse(request, new HttpHeader(), JsonConvert.SerializeObject(_remotePlatforms), HttpStatusCode.OK);
                  });
        }

        private void GivenCredentials(string clientId, string clientSecret)
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.IgdbClientId).Returns(clientId);
            Mocker.GetMock<IConfigService>().SetupGet(s => s.IgdbClientSecret).Returns(clientSecret);
        }

        private void GivenRemote(string slug, int id)
        {
            _remotePlatforms.RemoveAll(p => JsonConvert.SerializeObject(p).Contains($"\"slug\":\"{slug}\""));
            _remotePlatforms.Add(new { id, slug, name = slug });
        }

        private Platform Stored(string slug)
        {
            return Db.All<Platform>().Single(p => p.Slug == slug);
        }

        [Test]
        public void should_query_igdb_platforms_with_id_slug_and_name()
        {
            Subject.Sync();

            var request = _apiRequests.Single();
            request.Url.FullUri.Should().Be("https://api.igdb.com/v4/platforms");
            Encoding.UTF8.GetString(request.ContentData).Should().Be("fields id,slug,name; limit 500;");
        }

        [Test]
        public void should_not_change_anything_when_ids_match()
        {
            var before = Db.All<Platform>().Select(p => (p.Id, p.IgdbPlatformId, p.Slug)).ToList();

            Subject.Sync();

            Db.All<Platform>().Select(p => (p.Id, p.IgdbPlatformId, p.Slug)).Should().BeEquivalentTo(before);
        }

        [Test]
        public void should_correct_igdb_id_by_slug()
        {
            GivenRemote("satellaview", 9306);

            Subject.Sync();

            Stored("satellaview").IgdbPlatformId.Should().Be(9306);
            Mocker.Resolve<IPlatformService>().FindByIgdbPlatformId(9306).Slug.Should().Be("satellaview");
        }

        [Test]
        public void should_update_slug_renamed_on_igdb()
        {
            _remotePlatforms.RemoveAll(p => JsonConvert.SerializeObject(p).Contains("\"slug\":\"sega-cd\""));
            _remotePlatforms.Add(new { id = 78, slug = "mega-cd", name = "Sega CD" });

            Subject.Sync();

            var segaCd = Db.All<Platform>().Single(p => p.IgdbPlatformId == 78);
            segaCd.Slug.Should().Be("mega-cd");
        }

        [Test]
        public void should_not_take_an_igdb_id_another_platform_has()
        {
            GivenRemote("satellaview", 19);

            Subject.Sync();

            Stored("satellaview").IgdbPlatformId.Should().Be(306);
            Stored("snes").IgdbPlatformId.Should().Be(19);
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_warn_about_platforms_igdb_does_not_know()
        {
            _remotePlatforms.RemoveAll(p => JsonConvert.SerializeObject(p).Contains("\"slug\":\"pc-fx\""));

            Subject.Sync();

            Stored("pc-fx").IgdbPlatformId.Should().Be(274);
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_do_nothing_without_credentials()
        {
            GivenCredentials("", "");

            Subject.Sync();
            Subject.Handle(new ApplicationStartedEvent());

            _apiRequests.Should().BeEmpty();
            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.Push(It.IsAny<SyncPlatformsWithIgdbCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Never());
        }

        [Test]
        public void should_queue_sync_on_startup_with_credentials()
        {
            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.Push(It.IsAny<SyncPlatformsWithIgdbCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Once());
        }

        [Test]
        public void should_log_warning_not_error_when_credentials_are_rejected()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Execute(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(request => new HttpResponse(request, new HttpHeader(), "{}", HttpStatusCode.BadRequest));

            Subject.Execute(new SyncPlatformsWithIgdbCommand());

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
