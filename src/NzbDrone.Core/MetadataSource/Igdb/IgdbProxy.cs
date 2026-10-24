using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.AlternativeTitles;
using NzbDrone.Core.Games.Collections;
using NzbDrone.Core.Games.Credits;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.Igdb.Resource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.MetadataSource.Igdb
{
    public class IgdbProxy : IProvideGameInfo, ISearchForNewGame
    {
        // Requests both the pre-2025 fields (status, category, collection, websites.category, age_ratings.category/rating)
        // and their replacements, the mapper uses whichever is present.
        public const string GameFields =
            "fields name,slug,summary,storyline,first_release_date,updated_at," +
            "cover.image_id,artworks.image_id,screenshots.image_id," +
            "genres.name,themes.name,game_modes.name,keywords.name,platforms," +
            "release_dates.date,release_dates.platform," +
            "involved_companies.company.name,involved_companies.company.logo.image_id," +
            "involved_companies.developer,involved_companies.publisher,involved_companies.porting,involved_companies.supporting," +
            "collection.name,collections.name,franchises.name,alternative_names.name,alternative_names.comment," +
            "age_ratings.category,age_ratings.rating,age_ratings.organization.name,age_ratings.rating_category.rating," +
            "websites.url,websites.category,websites.type.type,videos.name,videos.video_id,similar_games," +
            "rating,rating_count,aggregated_rating,aggregated_rating_count,total_rating_count,hypes," +
            "status,game_status.status,category,game_type.type,version_parent,parent_game;";

        // Used if IGDB has removed the deprecated fields and rejects the query above
        public const string ModernGameFields =
            "fields name,slug,summary,storyline,first_release_date,updated_at," +
            "cover.image_id,artworks.image_id,screenshots.image_id," +
            "genres.name,themes.name,game_modes.name,keywords.name,platforms," +
            "release_dates.date,release_dates.platform," +
            "involved_companies.company.name,involved_companies.company.logo.image_id," +
            "involved_companies.developer,involved_companies.publisher,involved_companies.porting,involved_companies.supporting," +
            "collections.name,franchises.name,alternative_names.name,alternative_names.comment," +
            "age_ratings.organization.name,age_ratings.rating_category.rating," +
            "websites.url,websites.type.type,videos.name,videos.video_id,similar_games," +
            "rating,rating_count,aggregated_rating,aggregated_rating_count,total_rating_count,hypes," +
            "game_status.status,game_type.type,version_parent,parent_game;";

        private const int SearchLimit = 30;
        private const int BulkLimit = 500;

        private static readonly Regex IgdbUrlRegex = new(@"\bigdb\.com/games/(?<slug>[a-z0-9-]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex IgdbIdRegex = new(@"^\s*igdb(?:id)?\s*:\s*(?<id>\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex TrailingYearRegex = new(@"^(?<title>.+?)\s*\((?<year>(?:19|20)\d{2})\)\s*$", RegexOptions.Compiled);

        private readonly IIgdbClient _client;
        private readonly IGameMetadataService _gameMetadataService;
        private readonly Logger _logger;

        private bool _useModernFields;

        public IgdbProxy(IIgdbClient client, IGameMetadataService gameMetadataService, Logger logger)
        {
            _client = client;
            _gameMetadataService = gameMetadataService;
            _logger = logger;
        }

        public Tuple<GameMetadata, List<Credit>> GetGameInfo(int igdbId)
        {
            var resource = QueryGames($"where id = {igdbId}; limit 1;").FirstOrDefault();

            if (resource == null)
            {
                throw new GameNotFoundException(igdbId);
            }

            return new Tuple<GameMetadata, List<Credit>>(MapGame(resource), MapCredits(resource));
        }

        public List<GameMetadata> GetBulkGameInfo(List<int> igdbIds)
        {
            var results = new List<GameMetadata>();

            foreach (var chunk in igdbIds.Distinct().Chunk(BulkLimit))
            {
                results.AddRange(QueryGames($"where id = ({string.Join(",", chunk)}); limit {BulkLimit};").Select(MapGame));
            }

            return results;
        }

        public GameCollection GetCollectionInfo(int igdbCollectionId)
        {
            var collection = Query<IgdbCollectionResource>("collections", $"fields name,slug,games; where id = {igdbCollectionId}; limit 1;").FirstOrDefault();

            if (collection == null)
            {
                throw new GameNotFoundException(igdbCollectionId, "Collection with IGDB id {0} was not found, it may have been removed from IGDB.", igdbCollectionId);
            }

            var gameIds = collection.Games?.Select(g => g.Id).Where(id => id > 0).ToList() ?? new List<int>();
            var games = gameIds.Any() ? GetBulkGameInfo(gameIds) : new List<GameMetadata>();

            // Only keep the main entries of the series, not versions/editions of them
            games = games.OrderBy(g => g.ReleaseDate ?? DateTime.MaxValue).ToList();

            var poster = games.SelectMany(g => g.Images).FirstOrDefault(i => i.CoverType == MediaCoverTypes.Poster);

            return new GameCollection
            {
                IgdbId = collection.Id,
                Title = collection.Name,
                CleanTitle = collection.Name.CleanGameTitle(),
                SortTitle = Parser.Parser.NormalizeTitle(collection.Name),
                Images = poster == null ? new List<MediaCover.MediaCover>() : new List<MediaCover.MediaCover> { new(MediaCoverTypes.Poster, poster.RemoteUrl) },
                Games = games
            };
        }

        public List<GameMetadata> GetTrendingGames()
        {
            // Upcoming games with the most hype
            var now = IgdbMappings.ToUnixTime(DateTime.UtcNow);

            return QueryGames($"where version_parent = null & hypes > 0 & first_release_date > {now}; sort hypes desc; limit 50;")
                .Select(MapGame)
                .ToList();
        }

        public List<GameMetadata> GetPopularGames()
        {
            // Recently released games with the most ratings
            var since = IgdbMappings.ToUnixTime(DateTime.UtcNow.AddYears(-1));
            var now = IgdbMappings.ToUnixTime(DateTime.UtcNow);

            return QueryGames($"where version_parent = null & first_release_date > {since} & first_release_date <= {now} & total_rating_count > 5; sort total_rating_count desc; limit 50;")
                .Select(MapGame)
                .ToList();
        }

        public HashSet<int> GetChangedGames(DateTime startTime, IEnumerable<int> igdbIds)
        {
            var changed = new HashSet<int>();
            var since = IgdbMappings.ToUnixTime(startTime.AddMinutes(-15));

            foreach (var chunk in igdbIds.Distinct().Chunk(BulkLimit))
            {
                var results = Query<IgdbReference>("games", $"fields id; where id = ({string.Join(",", chunk)}) & updated_at > {since}; limit {BulkLimit};");
                changed.UnionWith(results.Select(r => r.Id));
            }

            return changed;
        }

        public List<Game> SearchForNewGame(string title, int igdbPlatformId = 0)
        {
            try
            {
                var idMatch = IgdbIdRegex.Match(title);

                if (idMatch.Success)
                {
                    return LookupById(int.Parse(idMatch.Groups["id"].Value));
                }

                var urlMatch = IgdbUrlRegex.Match(title);

                if (urlMatch.Success)
                {
                    var slug = urlMatch.Groups["slug"].Value.ToLowerInvariant();

                    return QueryGames($"where slug = \"{slug}\"; limit 1;").Select(MapSearchResult).ToList();
                }

                var term = title.Trim();
                int? year = null;

                var yearMatch = TrailingYearRegex.Match(term);

                if (yearMatch.Success)
                {
                    term = yearMatch.Groups["title"].Value;
                    year = int.Parse(yearMatch.Groups["year"].Value);
                }

                if (term.IsNullOrWhiteSpace())
                {
                    return new List<Game>();
                }

                var where = "where version_parent = null";

                if (igdbPlatformId > 0)
                {
                    where += $" & platforms = ({igdbPlatformId})";
                }

                var results = QueryGames($"search \"{EscapeSearchTerm(term)}\"; {where}; limit {SearchLimit};")
                    .Select(MapSearchResult)
                    .ToList();

                if (year.HasValue)
                {
                    // Prefer matches from the requested year, but keep the rest in case the year is wrong
                    results = results.OrderByDescending(g => g.Year == year.Value).ToList();
                }

                return results;
            }
            catch (IgdbException)
            {
                throw;
            }
            catch (HttpException ex)
            {
                _logger.Warn(ex);
                throw new IgdbException("Search for '{0}' failed. Unable to communicate with IGDB. {1}", ex, title, ex.Message);
            }
            catch (WebException ex)
            {
                _logger.Warn(ex);
                throw new IgdbException("Search for '{0}' failed. Unable to communicate with IGDB. {1}", ex, title, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                throw new IgdbException("Search for '{0}' failed. Invalid response received from IGDB. {1}", ex, title, ex.Message);
            }
        }

        public GameMetadata MapGameToIgdbGame(GameMetadata game)
        {
            try
            {
                if (game.IgdbId > 0)
                {
                    return _gameMetadataService.FindByIgdbId(game.IgdbId) ?? GetGameInfo(game.IgdbId).Item1;
                }

                var searchTerm = game.Year > 1950 ? $"{game.Title} ({game.Year})" : game.Title;
                var result = SearchForNewGame(searchTerm).FirstOrDefault();

                if (result == null)
                {
                    _logger.Warn("Couldn't map game {0} to a game on IGDB. It will not be added :(", game.Title);
                    return null;
                }

                return result.GameMetadata.Value;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Couldn't map game {0} to a game on IGDB. It will not be added :(", game.Title);
                return null;
            }
        }

        public GameMetadata MapGame(IgdbGameResource resource)
        {
            var now = DateTime.UtcNow;
            var game = new GameMetadata
            {
                IgdbId = resource.Id,
                IgdbSlug = resource.Slug,
                Title = resource.Name,
                OriginalTitle = resource.Name,
                CleanTitle = resource.Name.CleanGameTitle(),
                CleanOriginalTitle = resource.Name.CleanGameTitle(),
                SortTitle = GameTitleNormalizer.Normalize(resource.Name, resource.Id),
                Overview = resource.Summary,
                Storyline = resource.Storyline,
                ReleaseDate = IgdbMappings.FromUnixTime(resource.FirstReleaseDate),
                Status = IgdbMappings.MapStatus(resource, now),
                Genres = Names(resource.Genres),
                Themes = Names(resource.Themes),
                GameModes = Names(resource.GameModes),
                Keywords = Names(resource.Keywords),
                Recommendations = resource.SimilarGames?.Select(g => g.Id).Where(id => id > 0).ToList() ?? new List<int>(),
                Popularity = resource.TotalRatingCount ?? 0
            };

            game.Year = game.ReleaseDate?.Year ?? 0;

            game.PlatformReleaseDates = (resource.ReleaseDates ?? new List<IgdbReleaseDate>())
                .Where(d => d.Platform != null && d.Platform.Id > 0)
                .GroupBy(d => d.Platform.Id)
                .Select(g => new PlatformReleaseDate
                {
                    IgdbPlatformId = g.Key,
                    Date = g.Select(d => IgdbMappings.FromUnixTime(d.Date)).Where(d => d.HasValue).Min()
                })
                .ToList();

            game.AvailablePlatformIgdbIds = (resource.Platforms ?? new List<IgdbReference>())
                .Select(p => p.Id)
                .Concat(game.PlatformReleaseDates.Select(d => d.IgdbPlatformId))
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var companies = resource.InvolvedCompanies?.Where(c => c.Company?.Name != null).ToList() ?? new List<IgdbInvolvedCompany>();
            game.Developers = companies.Where(c => c.Developer).Select(c => c.Company.Name).Distinct().ToList();
            game.Publishers = companies.Where(c => c.Publisher).Select(c => c.Company.Name).Distinct().ToList();

            var collection = resource.Collections?.FirstOrDefault(c => c.Name.IsNotNullOrWhiteSpace()) ??
                             (resource.Collection?.Name.IsNotNullOrWhiteSpace() == true ? resource.Collection : null);

            if (collection != null)
            {
                game.CollectionIgdbId = collection.Id;
                game.CollectionTitle = collection.Name;
            }

            game.AlternativeTitles = (resource.AlternativeNames ?? new List<IgdbAlternativeName>())
                .Where(a => a.Name.IsNotNullOrWhiteSpace() && a.Name != resource.Name)
                .DistinctBy(a => a.Name)
                .Select(a => new AlternativeTitle
                {
                    Title = a.Name,
                    SourceType = SourceType.Igdb,
                    CleanTitle = a.Name.CleanGameTitle()
                })
                .ToList();

            game.AgeRatings = (resource.AgeRatings ?? new List<IgdbAgeRating>())
                .Select(IgdbMappings.MapAgeRating)
                .Where(r => r != null)
                .Distinct()
                .ToList();

            game.Websites = (resource.Websites ?? new List<IgdbWebsite>())
                .Select(IgdbMappings.MapWebsite)
                .Where(w => w != null)
                .ToList();

            game.Website = game.Websites.FirstOrDefault(w => w.Type == "Official")?.Url;

            var videos = resource.Videos?.Where(v => v.VideoId.IsNotNullOrWhiteSpace()).ToList() ?? new List<IgdbVideo>();
            game.YouTubeTrailerId = (videos.FirstOrDefault(v => v.Name.ContainsIgnoreCase("trailer")) ?? videos.FirstOrDefault())?.VideoId;

            game.Ratings = new Ratings
            {
                Igdb = resource.Rating.HasValue
                    ? new RatingChild { Value = Math.Round((decimal)resource.Rating.Value, 1), Votes = resource.RatingCount ?? 0, Type = RatingType.User }
                    : null,
                Critic = resource.AggregatedRating.HasValue
                    ? new RatingChild { Value = Math.Round((decimal)resource.AggregatedRating.Value, 1), Votes = resource.AggregatedRatingCount ?? 0, Type = RatingType.Critic }
                    : null
            };

            game.Images = MapImages(resource);

            return game;
        }

        public List<Credit> MapCredits(IgdbGameResource resource)
        {
            var credits = new List<Credit>();
            var order = 0;

            foreach (var involved in resource.InvolvedCompanies ?? new List<IgdbInvolvedCompany>())
            {
                if (involved.Company?.Name == null)
                {
                    continue;
                }

                var roles = new List<CreditType>();

                if (involved.Developer)
                {
                    roles.Add(CreditType.Developer);
                }

                if (involved.Publisher)
                {
                    roles.Add(CreditType.Publisher);
                }

                if (involved.Porting)
                {
                    roles.Add(CreditType.Porting);
                }

                if (involved.Supporting)
                {
                    roles.Add(CreditType.Supporting);
                }

                var logo = IgdbMappings.ImageUrl(involved.Company.Logo?.ImageId, "logo_med");

                foreach (var role in roles)
                {
                    credits.Add(new Credit
                    {
                        Name = involved.Company.Name,
                        CreditIgdbId = $"{involved.Id}-{role.ToString().ToLowerInvariant()}",
                        CompanyIgdbId = involved.Company.Id,
                        Type = role,
                        Order = order++,
                        Images = logo == null ? new List<MediaCover.MediaCover>() : new List<MediaCover.MediaCover> { new(MediaCoverTypes.Clearlogo, logo) }
                    });
                }
            }

            return credits;
        }

        private static List<MediaCover.MediaCover> MapImages(IgdbGameResource resource)
        {
            var images = new List<MediaCover.MediaCover>();

            var cover = IgdbMappings.ImageUrl(resource.Cover?.ImageId, "original");

            if (cover != null)
            {
                images.Add(new MediaCover.MediaCover(MediaCoverTypes.Poster, cover));
            }

            var artworks = resource.Artworks?.Where(a => a.ImageId.IsNotNullOrWhiteSpace()).ToList() ?? new List<IgdbImage>();
            var screenshots = resource.Screenshots?.Where(a => a.ImageId.IsNotNullOrWhiteSpace()).ToList() ?? new List<IgdbImage>();

            // Fanart is the first artwork, or the first screenshot when there is no artwork
            var fanart = artworks.FirstOrDefault() ?? screenshots.FirstOrDefault();

            if (fanart != null)
            {
                images.Add(new MediaCover.MediaCover(MediaCoverTypes.Fanart, IgdbMappings.ImageUrl(fanart.ImageId, "1080p")));
            }

            foreach (var screenshot in screenshots)
            {
                images.Add(new MediaCover.MediaCover(MediaCoverTypes.Screenshot, IgdbMappings.ImageUrl(screenshot.ImageId, "1080p")));
            }

            return images;
        }

        private static List<string> Names(List<IgdbReference> references)
        {
            return references?.Select(r => r.Name).Where(n => n.IsNotNullOrWhiteSpace()).Distinct().ToList() ?? new List<string>();
        }

        private static string EscapeSearchTerm(string term)
        {
            return term.Replace("\\", " ").Replace("\"", "\\\"");
        }

        private List<Game> LookupById(int igdbId)
        {
            try
            {
                return new List<Game> { new() { GameMetadata = GetGameInfo(igdbId).Item1 } };
            }
            catch (GameNotFoundException)
            {
                return new List<Game>();
            }
        }

        private Game MapSearchResult(IgdbGameResource resource)
        {
            return new Game { GameMetadata = MapGame(resource) };
        }

        private List<IgdbGameResource> QueryGames(string query)
        {
            if (!_useModernFields)
            {
                try
                {
                    return Query<IgdbGameResource>("games", $"{GameFields} {query}");
                }
                catch (IgdbException ex) when (ex.Message.ContainsIgnoreCase("Invalid Field"))
                {
                    _logger.Debug("IGDB rejected the deprecated fields, switching to the current field names");
                    _useModernFields = true;
                }
            }

            return Query<IgdbGameResource>("games", $"{ModernGameFields} {query}");
        }

        private List<T> Query<T>(string endpoint, string query)
        {
            return _client.Query<T>(endpoint, query);
        }
    }
}
