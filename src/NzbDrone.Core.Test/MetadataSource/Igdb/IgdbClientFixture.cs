using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource.Igdb;
using NzbDrone.Core.MetadataSource.Igdb.Resource;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MetadataSource.Igdb
{
    [TestFixture]
    public class IgdbClientFixture : CoreTest<IgdbClient>
    {
        private List<HttpRequest> _tokenRequests;
        private List<HttpRequest> _apiRequests;
        private Queue<HttpStatusCode> _apiStatusCodes;
        private HttpStatusCode _tokenStatusCode;

        [SetUp]
        public void Setup()
        {
            _tokenRequests = new List<HttpRequest>();
            _apiRequests = new List<HttpRequest>();
            _apiStatusCodes = new Queue<HttpStatusCode>();
            _tokenStatusCode = HttpStatusCode.OK;

            GivenCredentials("client-id", "client-secret");

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Execute(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(request =>
                  {
                      if (request.Url.FullUri.StartsWith(IgdbClient.TokenUrl))
                      {
                          _tokenRequests.Add(request);

                          var tokenContent = _tokenStatusCode == HttpStatusCode.OK ? ReadAllText("Files/Igdb/twitch_token.json") : "{\"status\":400,\"message\":\"invalid client secret\"}";

                          return new HttpResponse(request, new HttpHeader(), tokenContent, _tokenStatusCode);
                      }

                      _apiRequests.Add(request);

                      var statusCode = _apiStatusCodes.Count > 0 ? _apiStatusCodes.Dequeue() : HttpStatusCode.OK;
                      var content = statusCode == HttpStatusCode.OK ? ReadAllText("Files/Igdb/game_legacy_fields.json") : "{\"message\":\"Authorization Failure\"}";

                      return new HttpResponse(request, new HttpHeader(), content, statusCode);
                  });
        }

        private void GivenCredentials(string clientId, string clientSecret)
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.IgdbClientId).Returns(clientId);
            Mocker.GetMock<IConfigService>().SetupGet(s => s.IgdbClientSecret).Returns(clientSecret);
        }

        [Test]
        public void should_throw_when_credentials_are_missing()
        {
            GivenCredentials("", "");

            Subject.HasCredentials.Should().BeFalse();
            Assert.Throws<IgdbCredentialsMissingException>(() => Subject.Query<IgdbGameResource>("games", "fields name;"));
            _tokenRequests.Should().BeEmpty();
        }

        [Test]
        public void should_post_apicalypse_query_with_twitch_headers()
        {
            var results = Subject.Query<IgdbGameResource>("games", "fields name; where id = 1103;");

            results.Should().HaveCount(1);
            results.First().Name.Should().Be("Super Metroid");

            var request = _apiRequests.Single();
            request.Method.Should().Be(System.Net.Http.HttpMethod.Post);
            request.Url.FullUri.Should().Be("https://api.igdb.com/v4/games");
            request.Headers.Get("Client-ID").Should().Be("client-id");
            request.Headers.Get("Authorization").Should().Be("Bearer abcdefghijklmnopqrstuvwxyz0123");
            Encoding.UTF8.GetString(request.ContentData).Should().Be("fields name; where id = 1103;");
            request.RateLimit.TotalMilliseconds.Should().Be(250);
        }

        [Test]
        public void should_send_client_credentials_in_body_not_url()
        {
            Subject.Query<IgdbGameResource>("games", "fields name;");

            var tokenRequest = _tokenRequests.Single();
            tokenRequest.Method.Should().Be(System.Net.Http.HttpMethod.Post);
            tokenRequest.Url.FullUri.Should().NotContain("client-secret");

            var body = Encoding.UTF8.GetString(tokenRequest.ContentData);
            body.Should().Contain("client_id=client-id");
            body.Should().Contain("client_secret=client-secret");
            body.Should().Contain("grant_type=client_credentials");
        }

        [Test]
        public void should_cache_token_between_requests()
        {
            Subject.Query<IgdbGameResource>("games", "fields name;");
            Subject.Query<IgdbGameResource>("games", "fields name;");

            _tokenRequests.Should().HaveCount(1);
            _apiRequests.Should().HaveCount(2);
        }

        [Test]
        public void should_get_new_token_when_credentials_change()
        {
            Subject.Query<IgdbGameResource>("games", "fields name;");

            GivenCredentials("client-id", "new-secret");

            Subject.Query<IgdbGameResource>("games", "fields name;");

            _tokenRequests.Should().HaveCount(2);
        }

        [Test]
        public void should_refresh_token_and_retry_on_401()
        {
            _apiStatusCodes.Enqueue(HttpStatusCode.Unauthorized);

            var results = Subject.Query<IgdbGameResource>("games", "fields name;");

            results.Should().HaveCount(1);
            _tokenRequests.Should().HaveCount(2);
            _apiRequests.Should().HaveCount(2);
        }

        [Test]
        public void should_throw_authentication_exception_when_still_unauthorized()
        {
            _apiStatusCodes.Enqueue(HttpStatusCode.Unauthorized);
            _apiStatusCodes.Enqueue(HttpStatusCode.Unauthorized);

            Assert.Throws<IgdbAuthenticationException>(() => Subject.Query<IgdbGameResource>("games", "fields name;"));
        }

        [Test]
        public void should_throw_authentication_exception_when_twitch_rejects_credentials()
        {
            _tokenStatusCode = HttpStatusCode.BadRequest;

            Assert.Throws<IgdbAuthenticationException>(() => Subject.ValidateCredentials());
        }

        [Test]
        public void should_throw_igdb_exception_on_server_error()
        {
            _apiStatusCodes.Enqueue(HttpStatusCode.InternalServerError);

            Assert.Throws<IgdbException>(() => Subject.Query<IgdbGameResource>("games", "fields name;"));
        }
    }
}
