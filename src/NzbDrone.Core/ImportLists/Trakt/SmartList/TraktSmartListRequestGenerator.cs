using System;
using System.Collections.Generic;
using System.Net.Http;
using NzbDrone.Core.Notifications.Trakt;

namespace NzbDrone.Core.ImportLists.Trakt.SmartList;

public class TraktSmartListRequestGenerator : IImportListRequestGenerator
{
    private readonly ITraktProxy _traktProxy;
    public TraktSmartListSettings Settings { get; set; }

    public TraktSmartListRequestGenerator(ITraktProxy traktProxy)
    {
        _traktProxy = traktProxy;
    }

    public virtual ImportListPageableRequestChain GetGames()
    {
        var pageableRequests = new ImportListPageableRequestChain();

        pageableRequests.Add(GetGamesRequest());

        return pageableRequests;
    }

    private IEnumerable<ImportListRequest> GetGamesRequest()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Settings.SmartListId);

        var link = $"smart-lists/{Settings.SmartListId}/items/games?limit={Settings.Limit}";

        yield return new ImportListRequest(_traktProxy.BuildRequest(link, HttpMethod.Get, Settings.AccessToken));
    }
}
