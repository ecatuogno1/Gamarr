using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabRequestGenerator : IIndexerRequestGenerator
    {
        private readonly INewznabCapabilitiesProvider _capabilitiesProvider;
        public int MaxPages { get; set; }
        public int PageSize { get; set; }
        public NewznabSettings Settings { get; set; }

        public NewznabRequestGenerator(INewznabCapabilitiesProvider capabilitiesProvider)
        {
            _capabilitiesProvider = capabilitiesProvider;

            MaxPages = 30;
            PageSize = 100;
        }

        private bool SupportsSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedSearchParameters != null &&
                       capabilities.SupportedSearchParameters.Contains("q");
            }
        }

        private string TextSearchEngine
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.TextSearchEngine;
            }
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            // Gamarr only uses t=search, the RSS feed is a search without a query
            pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "search", ""));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(GameSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (!SupportsSearch)
            {
                return pageableRequests;
            }

            // Newznab has no game id parameters, so games are searched by title, restricted to the platform's categories
            var categories = NewznabStandardCategory.GetSearchCategories(Settings.Categories, searchCriteria.Platform?.NewznabCategories);
            var raw = TextSearchEngine == "raw";
            var queryTitles = raw ? searchCriteria.SceneTitles : searchCriteria.CleanSceneTitles;

            foreach (var queryTitle in queryTitles)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages,
                    categories,
                    "search",
                    $"&q={NewsnabifyTitle(queryTitle)}"));
            }

            // Fall back to title + platform (e.g. "Super Metroid SNES") if the title alone found nothing
            var platformQueryTitles = searchCriteria.GetPlatformQueryTitles(queryTitles, !raw);

            if (platformQueryTitles.Any())
            {
                pageableRequests.AddTier();

                foreach (var queryTitle in platformQueryTitles)
                {
                    pageableRequests.Add(GetPagedRequests(MaxPages,
                        categories,
                        "search",
                        $"&q={NewsnabifyTitle(queryTitle)}"));
                }
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, IEnumerable<int> categories, string searchType, string parameters)
        {
            if (categories.Empty())
            {
                yield break;
            }

            var categoriesQuery = string.Join(",", categories.Distinct());

            var baseUrl = $"{Settings.BaseUrl.TrimEnd('/')}{Settings.ApiPath.TrimEnd('/')}?t={searchType}&cat={categoriesQuery}&extended=1{Settings.AdditionalParameters}";

            if (Settings.ApiKey.IsNotNullOrWhiteSpace())
            {
                baseUrl += "&apikey=" + Settings.ApiKey;
            }

            if (PageSize == 0)
            {
                yield return new IndexerRequest($"{baseUrl}{parameters}", HttpAccept.Rss);
            }
            else
            {
                for (var page = 0; page < maxPages; page++)
                {
                    yield return new IndexerRequest($"{baseUrl}&offset={page * PageSize}&limit={PageSize}{parameters}", HttpAccept.Rss);
                }
            }
        }

        private static string NewsnabifyTitle(string title)
        {
            var newtitle = title.Replace("+", " ");
            return Uri.EscapeDataString(newtitle);
        }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }
}
