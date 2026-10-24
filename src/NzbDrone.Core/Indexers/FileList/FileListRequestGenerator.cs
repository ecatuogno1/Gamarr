using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.FileList
{
    public class FileListRequestGenerator : IIndexerRequestGenerator
    {
        public FileListSettings Settings { get; set; }
        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();
            pageableRequests.Add(GetRequest("latest-torrents", Settings.Categories, ""));
            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(GameSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var categories = FileListCategoryMapping.GetSearchCategories(Settings.Categories, searchCriteria.Platform?.NewznabCategories);

            foreach (var queryTitle in searchCriteria.CleanSceneTitles)
            {
                pageableRequests.Add(GetRequest("search-torrents", categories, $"&type=name&query={queryTitle.Trim()}"));
            }

            // Fall back to title + platform if the title alone found nothing
            var platformQueryTitles = searchCriteria.GetPlatformQueryTitles(searchCriteria.CleanSceneTitles, true);

            if (platformQueryTitles.Any())
            {
                pageableRequests.AddTier();

                foreach (var queryTitle in platformQueryTitles)
                {
                    pageableRequests.Add(GetRequest("search-torrents", categories, $"&type=name&query={queryTitle.Trim()}"));
                }
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetRequest(string searchType, IEnumerable<int> categories, string parameters)
        {
            if (categories.Empty())
            {
                yield break;
            }

            var categoriesQuery = string.Join(",", categories.Distinct());

            var baseUrl = $"{Settings.BaseUrl.TrimEnd('/')}/api.php?action={searchType}&category={categoriesQuery}{parameters}";

            var request = new IndexerRequest(baseUrl, HttpAccept.Json);
            request.HttpRequest.Credentials = new BasicNetworkCredential(Settings.Username.Trim(), Settings.Passkey.Trim());

            yield return request;
        }
    }
}
