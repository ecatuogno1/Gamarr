using System;
using System.IO;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Gamarr.Http.ErrorManagement;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource.Igdb;
using NzbDrone.Test.Common;

namespace NzbDrone.Api.Test.Http.ErrorManagement
{
    [TestFixture]
    public class GamarrErrorPipelineFixture : TestBase<GamarrErrorPipeline>
    {
        private class ExceptionFeature : IExceptionHandlerPathFeature
        {
            public Exception Error { get; set; }
            public string Path { get; set; }
        }

        private (int StatusCode, JsonElement Body) Handle(Exception exception)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/v3/game/lookup";
            context.Response.Body = new MemoryStream();
            context.Features.Set<IExceptionHandlerPathFeature>(new ExceptionFeature { Error = exception, Path = "/api/v3/game/lookup" });

            Subject.HandleException(context).GetAwaiter().GetResult();

            context.Response.Body.Position = 0;
            var body = JsonDocument.Parse(context.Response.Body).RootElement;

            return (context.Response.StatusCode, body);
        }

        [Test]
        public void should_return_503_with_message_when_igdb_credentials_are_missing()
        {
            var (statusCode, body) = Handle(new IgdbCredentialsMissingException());

            statusCode.Should().Be((int)HttpStatusCode.ServiceUnavailable);
            body.GetProperty("message").GetString().Should().Contain("Settings > Metadata");

            // No stack trace for a configuration problem
            if (body.TryGetProperty("description", out var description))
            {
                description.ValueKind.Should().Be(JsonValueKind.Null);
            }

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_503_when_igdb_rejects_the_credentials()
        {
            var (statusCode, body) = Handle(new IgdbAuthenticationException("Twitch rejected the IGDB Client ID / Client Secret (HTTP {0})", 400));

            statusCode.Should().Be((int)HttpStatusCode.ServiceUnavailable);
            body.GetProperty("message").GetString().Should().Contain("Twitch rejected");

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_503_when_igdb_is_unreachable()
        {
            var (statusCode, _) = Handle(new IgdbException("Search for '{0}' failed. Unable to communicate with IGDB.", "zelda"));

            statusCode.Should().Be((int)HttpStatusCode.ServiceUnavailable);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_404_when_igdb_game_is_not_found()
        {
            var (statusCode, _) = Handle(new GameNotFoundException(1234));

            statusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public void should_return_500_and_log_fatal_for_unexpected_exceptions()
        {
            var (statusCode, _) = Handle(new InvalidOperationException("boom"));

            statusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            ExceptionVerification.ExpectedFatals(1);
        }
    }
}
