using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.ImportLists.ImportListGames;

namespace NzbDrone.Core.ImportLists.Simkl
{
    public class SimklParser : IParseImportListResponse
    {
        private ImportListResponse _importResponse;
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(SimklParser));

        public virtual IList<ImportListGame> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var games = new List<ImportListGame>();

            if (!PreProcess(_importResponse))
            {
                return games;
            }

            var jsonResponse = Json.Deserialize<SimklResponse>(_importResponse.Content);

            // no games were returned
            if (jsonResponse == null)
            {
                return games;
            }

            if (jsonResponse.Anime != null)
            {
                foreach (var game in jsonResponse.Anime)
                {
                    if (int.TryParse(game.Show.Ids.Igdb, out var igdbId) && igdbId > 0 && game.AnimeType is SimklAnimeType.Game)
                    {
                        games.AddIfNotNull(new ImportListGame
                        {
                            Title = game.Show.Title,
                            ImdbId = game.Show.Ids.Imdb,
                            IgdbId = igdbId,
                        });
                    }
                    else
                    {
                        Logger.Warn("Skipping info grabbing for '{0}' because it is an unsupported content type.", game.Show.Title);
                    }
                }
            }

            if (jsonResponse.Games != null)
            {
                foreach (var game in jsonResponse.Games)
                {
                    games.AddIfNotNull(new ImportListGame
                    {
                        Title = game.Game.Title,
                        IgdbId = int.TryParse(game.Game.Ids.Igdb, out var igdbId) ? igdbId : 0,
                        ImdbId = game.Game.Ids.Imdb,
                    });
                }
            }

            return games;
        }

        protected virtual bool PreProcess(ImportListResponse netImportResponse)
        {
            if (netImportResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(netImportResponse, "Simkl API call resulted in an unexpected StatusCode [{0}]", netImportResponse.HttpResponse.StatusCode);
            }

            if (netImportResponse.HttpResponse.Headers.ContentType != null && netImportResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                netImportResponse.HttpRequest.Headers.Accept != null && !netImportResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new ImportListException(netImportResponse, "Simkl API responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
