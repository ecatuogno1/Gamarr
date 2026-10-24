using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.IGDB.User
{
    public class IGDBUserRequestGenerator : IImportListRequestGenerator
    {
        public IGDBUserSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public IHttpRequestBuilderFactory RequestBuilder { get; set; }
        public Logger Logger { get; set; }

        public int MaxPages { get; set; }

        public virtual ImportListPageableRequestChain GetGames()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetGamesRequests());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetGamesRequests()
        {
            var requestBuilder = RequestBuilder.Create()
                .SetHeader("Authorization", $"Bearer {Settings.AccessToken}")
                .SetSegment("api", "4")
                .SetSegment("route", "account")
                .SetSegment("id", Settings.AccountId);

            switch (Settings.ListType)
            {
                case (int)IGDBUserListType.Watchlist:
                    requestBuilder.SetSegment("secondaryRoute", "/game/watchlist");
                    break;
                case (int)IGDBUserListType.Recommendations:
                    requestBuilder.SetSegment("secondaryRoute", "/game/recommendations");
                    break;
                case (int)IGDBUserListType.Rated:
                    requestBuilder.SetSegment("secondaryRoute", "/game/rated");
                    break;
                case (int)IGDBUserListType.Favorite:
                    requestBuilder.SetSegment("secondaryRoute", "/game/favorites");
                    break;
            }

            requestBuilder.Accept(HttpAccept.Json);

            requestBuilder.Method = HttpMethod.Get;

            Logger.Trace("IGDB User {0}: Getting total pages", (IGDBUserListType)Settings.ListType);

            var jsonResponse = JsonConvert.DeserializeObject<GameSearchResource>(HttpClient.Execute(requestBuilder.Build()).Content);

            MaxPages = jsonResponse.TotalPages;

            if (jsonResponse.TotalPages > 1)
            {
                Logger.Debug("IGDB User {0}: processing {1} pages", (IGDBUserListType)Settings.ListType, MaxPages);
            }

            for (var pageNumber = 1; pageNumber <= MaxPages; pageNumber++)
            {
                requestBuilder.AddQueryParam("page", pageNumber, true);

                var request = requestBuilder.Build();

                Logger.Debug("IGDB User {0}: Processing page {1} of {2}", (IGDBUserListType)Settings.ListType, pageNumber, MaxPages);
                Logger.Trace("IGDB User {0}: Request URL: {1}", (IGDBUserListType)Settings.ListType, request.Url);

                yield return new ImportListRequest(request);
            }
        }
    }
}
