using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource.Igdb.Resource;

namespace NzbDrone.Core.MetadataSource.Igdb
{
    public interface IIgdbClient
    {
        bool HasCredentials { get; }
        List<T> Query<T>(string endpoint, string query);
        void ValidateCredentials();
    }

    public class IgdbClient : IIgdbClient
    {
        public const string TokenUrl = "https://id.twitch.tv/oauth2/token";
        public const string ApiUrl = "https://api.igdb.com/v4";

        // IGDB allows 4 requests per second
        private const double RateLimitSeconds = 0.25;

        private readonly IHttpClient _httpClient;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        private readonly object _tokenLock = new();
        private string _accessToken;
        private string _accessTokenCredentials;
        private DateTime _accessTokenExpiry;

        public IgdbClient(IHttpClient httpClient, IConfigService configService, Logger logger)
        {
            _httpClient = httpClient;
            _configService = configService;
            _logger = logger;
        }

        public bool HasCredentials => _configService.IgdbClientId.IsNotNullOrWhiteSpace() && _configService.IgdbClientSecret.IsNotNullOrWhiteSpace();

        // Gets a token if there's no valid cached one for the current credentials, throws IgdbAuthenticationException when Twitch rejects them
        public void ValidateCredentials()
        {
            GetAccessToken(false);
        }

        public List<T> Query<T>(string endpoint, string query)
        {
            var token = GetAccessToken(false);
            var response = Execute(endpoint, query, token);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // The token was revoked or expired early, get a new one and try once more
                _logger.Debug("IGDB returned 401, refreshing the Twitch access token");
                token = GetAccessToken(true);
                response = Execute(endpoint, query, token);
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                _logger.Debug("IGDB rate limit hit, retrying in 1 second");
                Thread.Sleep(1000);
                response = Execute(endpoint, query, token);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new IgdbAuthenticationException("IGDB rejected the Twitch access token. Check the Twitch Client ID and Client Secret in Settings > Metadata.");
            }

            if (response.HasHttpError)
            {
                throw new IgdbException("IGDB request to '{0}' failed with HTTP {1}: {2}", endpoint, (int)response.StatusCode, response.Content);
            }

            return JsonConvert.DeserializeObject<List<T>>(response.Content) ?? new List<T>();
        }

        private HttpResponse Execute(string endpoint, string query, string token)
        {
            var request = new HttpRequestBuilder($"{ApiUrl}/{endpoint}")
                .Post()
                .Accept(HttpAccept.Json)
                .SetHeader("Client-ID", _configService.IgdbClientId)
                .SetHeader("Authorization", $"Bearer {token}")
                .WithRateLimit(RateLimitSeconds)
                .Build();

            request.RateLimitKey = "igdb";
            request.SuppressHttpError = true;
            request.Headers.ContentType = "text/plain";
            request.SetContent(query);
            request.ContentSummary = query;

            return _httpClient.Execute(request);
        }

        private string GetAccessToken(bool forceRefresh)
        {
            var clientId = _configService.IgdbClientId;
            var clientSecret = _configService.IgdbClientSecret;

            if (clientId.IsNullOrWhiteSpace() || clientSecret.IsNullOrWhiteSpace())
            {
                throw new IgdbCredentialsMissingException();
            }

            var credentials = $"{clientId}:{clientSecret}";

            lock (_tokenLock)
            {
                if (!forceRefresh &&
                    _accessToken.IsNotNullOrWhiteSpace() &&
                    _accessTokenCredentials == credentials &&
                    _accessTokenExpiry > DateTime.UtcNow.AddMinutes(1))
                {
                    return _accessToken;
                }

                var request = new HttpRequestBuilder(TokenUrl)
                    .Post()
                    .Accept(HttpAccept.Json)
                    .AddFormParameter("client_id", clientId)
                    .AddFormParameter("client_secret", clientSecret)
                    .AddFormParameter("grant_type", "client_credentials")
                    .Build();

                // Sent as a form body (not query params) so the secret doesn't end up in logged URLs
                request.SuppressHttpError = true;
                request.ContentSummary = "client_credentials";

                var response = _httpClient.Execute(request);

                if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    _accessToken = null;
                    throw new IgdbAuthenticationException("Twitch rejected the IGDB Client ID / Client Secret (HTTP {0})", (int)response.StatusCode);
                }

                if (response.HasHttpError)
                {
                    throw new IgdbException("Unable to get a Twitch access token for IGDB, HTTP {0}", (int)response.StatusCode);
                }

                var token = JsonConvert.DeserializeObject<TwitchTokenResource>(response.Content);

                if (token?.AccessToken == null)
                {
                    throw new IgdbException("Twitch returned an invalid access token response");
                }

                _accessToken = token.AccessToken;
                _accessTokenCredentials = credentials;
                _accessTokenExpiry = DateTime.UtcNow.AddSeconds(token.ExpiresIn);

                return _accessToken;
            }
        }
    }
}
