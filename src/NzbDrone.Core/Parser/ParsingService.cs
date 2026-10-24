using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Games;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Parser.RomanNumerals;
using NzbDrone.Core.Platforms;

namespace NzbDrone.Core.Parser
{
    public interface IParsingService
    {
        Game GetGame(string title);
        RemoteGame Map(ParsedGameInfo parsedGameInfo, int igdbId, SearchCriteriaBase searchCriteria);
        RemoteGame Map(ParsedGameInfo parsedGameInfo, int gameId);
        ParsedGameInfo ParseMinimalPathGameInfo(string path);
        ParsedGameInfo ResolvePlatform(ParsedGameInfo parsedGameInfo);
    }

    // Maps parsed releases to library games. A title can be in the library once per platform, so games are
    // matched by title (and year) among the games on the release's platform when it's known.
    public class ParsingService : IParsingService
    {
        private static HashSet<ArabicRomanNumeral> _arabicRomanNumeralMappings;

        private readonly IGameService _gameService;
        private readonly IPlatformService _platformService;
        private readonly Logger _logger;

        public ParsingService(IGameService gameService,
                              IPlatformService platformService,
                              Logger logger)
        {
            _gameService = gameService;
            _platformService = platformService;
            _logger = logger;

            if (_arabicRomanNumeralMappings == null)
            {
                _arabicRomanNumeralMappings = RomanNumeralParser.GetArabicRomanNumeralsMapping();
            }
        }

        public ParsedGameInfo ParseMinimalPathGameInfo(string path)
        {
            var fileInfo = new FileInfo(path);

            var result = Parser.ParseGameTitle(fileInfo.Name, true);

            if (result == null)
            {
                _logger.Debug("Attempting to parse game info using directory name. '{0}'", fileInfo.Directory.Name);
                result = Parser.ParseGameTitle(fileInfo.Directory.Name + fileInfo.Extension);
            }

            return ResolvePlatform(result);
        }

        public ParsedGameInfo ResolvePlatform(ParsedGameInfo parsedGameInfo)
        {
            if (parsedGameInfo != null && parsedGameInfo.PlatformIgdbId > 0 && parsedGameInfo.PlatformId == 0)
            {
                parsedGameInfo.PlatformId = _platformService.FindByIgdbPlatformId(parsedGameInfo.PlatformIgdbId)?.Id ?? 0;
            }

            return parsedGameInfo;
        }

        public Game GetGame(string title)
        {
            var parsedGameInfo = ResolvePlatform(Parser.ParseGameTitle(title));

            if (parsedGameInfo == null)
            {
                try
                {
                    return _gameService.FindByTitle(title);
                }
                catch (MultipleGamesFoundException e)
                {
                    _logger.Debug(e, "'{0}' matches several games", title);
                    return null;
                }
            }

            return TryGetGameByTitleAndOrYear(parsedGameInfo)?.Game;
        }

        public RemoteGame Map(ParsedGameInfo parsedGameInfo, int igdbId, SearchCriteriaBase searchCriteria)
        {
            ResolvePlatform(parsedGameInfo);

            var remoteGame = new RemoteGame
            {
                ParsedGameInfo = parsedGameInfo
            };

            var gameMatch = FindGame(parsedGameInfo, igdbId, searchCriteria);

            if (gameMatch != null)
            {
                remoteGame.Game = gameMatch.Game;
                remoteGame.GameMatchType = gameMatch.MatchType;
            }

            remoteGame.Languages = parsedGameInfo.Languages;

            if (searchCriteria != null)
            {
                remoteGame.GameRequested = remoteGame.Game?.Id == searchCriteria.Game?.Id;
            }

            return remoteGame;
        }

        public RemoteGame Map(ParsedGameInfo parsedGameInfo, int gameId)
        {
            ResolvePlatform(parsedGameInfo);

            return new RemoteGame
            {
                ParsedGameInfo = parsedGameInfo,
                Game = _gameService.GetGame(gameId)
            };
        }

        private FindGameResult FindGame(ParsedGameInfo parsedGameInfo, int igdbId, SearchCriteriaBase searchCriteria)
        {
            FindGameResult result = null;

            if (searchCriteria != null)
            {
                result = TryGetGameBySearchCriteria(parsedGameInfo, igdbId, searchCriteria);
            }

            if (result == null && igdbId > 0)
            {
                result = TryGetGameByIgdbId(parsedGameInfo, igdbId);
            }

            if (result == null && searchCriteria == null)
            {
                result = TryGetGameByTitleAndOrYear(parsedGameInfo);
            }

            if (result == null)
            {
                _logger.Debug($"No matching game for titles '{string.Join(", ", parsedGameInfo.GameTitles)} ({parsedGameInfo.Year})'");
            }

            return result;
        }

        private FindGameResult TryGetGameByIgdbId(ParsedGameInfo parsedGameInfo, int igdbId)
        {
            // A title can be in the library once per platform, only an unambiguous match counts
            var games = FilterByPlatform(_gameService.FindByIgdbId(igdbId), parsedGameInfo);
            var game = games.Count == 1 ? games[0] : null;

            if (game != null && (parsedGameInfo.Year < 1800 || System.Math.Abs(game.GameMetadata.Value.Year - parsedGameInfo.Year) <= 1 || game.GameMetadata.Value.SecondaryYear == parsedGameInfo.Year))
            {
                return new FindGameResult(game, GameMatchType.Id);
            }

            return null;
        }

        private FindGameResult TryGetGameByTitleAndOrYear(ParsedGameInfo parsedGameInfo)
        {
            var titles = GetTitles(parsedGameInfo);
            var candidates = FilterByPlatform(_gameService.FindByTitleCandidates(titles, out var otherTitles), parsedGameInfo);

            try
            {
                var game = _gameService.FindByTitle(titles, parsedGameInfo.Year > 1800 ? parsedGameInfo.Year : null, otherTitles, candidates);

                if (game != null)
                {
                    return new FindGameResult(game, GameMatchType.Title);
                }
            }
            catch (MultipleGamesFoundException e)
            {
                // The same title on several platforms and the release doesn't say which one
                _logger.Debug(e, "Release matches several games: '{0}'", parsedGameInfo.ReleaseTitle);
            }

            return null;
        }

        private FindGameResult TryGetGameBySearchCriteria(ParsedGameInfo parsedGameInfo, int igdbId, SearchCriteriaBase searchCriteria)
        {
            var game = searchCriteria.Game;

            if (game == null)
            {
                return null;
            }

            var possibleTitles = new List<string>
            {
                game.GameMetadata.Value.CleanTitle
            };

            possibleTitles.AddIfNotNull(game.GameMetadata.Value.CleanOriginalTitle);
            possibleTitles.AddRange(game.GameMetadata.Value.AlternativeTitles.Select(t => t.CleanTitle));
            possibleTitles.AddRange(game.GameMetadata.Value.Translations.Select(t => t.CleanTitle));

            var cleanTitles = GetTitles(parsedGameInfo).Select(t => t.CleanGameTitle()).ToArray();

            if (possibleTitles.Any(pt =>
                cleanTitles.Contains(pt)
                || _arabicRomanNumeralMappings.Any(mn =>
                    cleanTitles.Contains(pt.Replace(mn.ArabicNumeralAsString, mn.RomanNumeralLowerCase))
                    || cleanTitles.Any(t => t.Replace(mn.ArabicNumeralAsString, mn.RomanNumeralLowerCase) == pt))))
            {
                if (parsedGameInfo.Year < 1800 || System.Math.Abs(game.GameMetadata.Value.Year - parsedGameInfo.Year) <= 1 || game.GameMetadata.Value.SecondaryYear == parsedGameInfo.Year)
                {
                    return new FindGameResult(game, GameMatchType.Title);
                }
            }

            if (igdbId > 0 && igdbId == game.IgdbId)
            {
                return new FindGameResult(game, GameMatchType.Id);
            }

            return null;
        }

        // The parsed titles plus the title with the edition ("Mass Effect" + "Legendary Edition"), IGDB often has
        // the edition in the title
        private static List<string> GetTitles(ParsedGameInfo parsedGameInfo)
        {
            var titles = parsedGameInfo.GameTitles.ToList();

            if (parsedGameInfo.Edition.IsNotNullOrWhiteSpace() && parsedGameInfo.PrimaryGameTitle.IsNotNullOrWhiteSpace())
            {
                titles.Add($"{parsedGameInfo.PrimaryGameTitle} {parsedGameInfo.Edition}");
            }

            return titles;
        }

        private List<Game> FilterByPlatform(List<Game> games, ParsedGameInfo parsedGameInfo)
        {
            games ??= new List<Game>();

            if (parsedGameInfo.PlatformIgdbId == 0)
            {
                return games;
            }

            return games.Where(g =>
            {
                var platform = g.Platform ?? _platformService.Find(g.PlatformId);

                return platform == null || PlatformParser.AreCompatible(platform.IgdbPlatformId, parsedGameInfo.PlatformIgdbId);
            }).ToList();
        }
    }
}
