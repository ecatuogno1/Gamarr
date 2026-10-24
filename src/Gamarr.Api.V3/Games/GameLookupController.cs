using System;
using System.Collections.Generic;
using System.Linq;
using Gamarr.Http;
using Gamarr.Http.REST;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Games;
using NzbDrone.Core.ImportLists.ImportExclusions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Platforms;

namespace Gamarr.Api.V3.Games
{
    [V3ApiController("game/lookup")]
    public class GameLookupController : RestController<GameResource>
    {
        private readonly ISearchForNewGame _searchProxy;
        private readonly IProvideGameInfo _gameInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly INamingConfigService _namingService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IConfigService _configService;
        private readonly IImportListExclusionService _importListExclusionService;
        private readonly IPlatformService _platformService;

        public GameLookupController(ISearchForNewGame searchProxy,
                                 IProvideGameInfo gameInfo,
                                 IBuildFileNames fileNameBuilder,
                                 INamingConfigService namingService,
                                 IMapCoversToLocal coverMapper,
                                 IConfigService configService,
                                 IImportListExclusionService importListExclusionService,
                                 IPlatformService platformService)
        {
            _platformService = platformService;
            _gameInfo = gameInfo;
            _searchProxy = searchProxy;
            _fileNameBuilder = fileNameBuilder;
            _namingService = namingService;
            _coverMapper = coverMapper;
            _configService = configService;
            _importListExclusionService = importListExclusionService;
        }

        [NonAction]
        public override Results<Ok<GameResource>, NotFound> GetResourceByIdWithErrorHandler(int id)
        {
            throw new NotImplementedException();
        }

        protected override GameResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("igdb")]
        [Produces("application/json")]
        public Ok<GameResource> SearchByIgdbId(int igdbId)
        {
            var result = new Game { GameMetadata = _gameInfo.GetGameInfo(igdbId).Item1 };

            return TypedResults.Ok(MapToResource(new[] { result }).First());
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<IEnumerable<GameResource>> Search([FromQuery] string term, [FromQuery] int? platformId = null)
        {
            // platformId is a Gamarr platform id, IGDB is filtered by the platform's IGDB id
            var igdbPlatformId = platformId.HasValue ? _platformService.Find(platformId.Value)?.IgdbPlatformId ?? 0 : 0;
            var searchResults = _searchProxy.SearchForNewGame(term, igdbPlatformId);

            return TypedResults.Ok(MapToResource(searchResults));
        }

        private IEnumerable<GameResource> MapToResource(IEnumerable<Game> games)
        {
            var gameInfoLanguage = (Language)_configService.GameInfoLanguage;
            var availDelay = _configService.AvailabilityDelay;
            var namingConfig = _namingService.GetConfig();

            var listExclusions = _importListExclusionService.All();

            var enabledPlatforms = _platformService.Enabled();

            foreach (var currentGame in games)
            {
                var availablePlatforms = _platformService.FindByIgdbPlatformIds(currentGame.GameMetadata.Value.AvailablePlatformIgdbIds)
                    .Where(p => p.Enabled)
                    .ToList();

                // Default to the first platform IGDB lists, the folder preview uses it
                if (currentGame.PlatformId == 0 && availablePlatforms.Any())
                {
                    currentGame.PlatformId = availablePlatforms.First().Id;
                    currentGame.Platform = availablePlatforms.First();
                }

                var translation = currentGame.GameMetadata.Value.Translations.FirstOrDefault(t => t.Language == gameInfoLanguage);
                var resource = currentGame.ToResource(availDelay, translation);

                _coverMapper.ConvertToLocalUrls(resource.Id, resource.Images, resource.Added);

                var poster = currentGame.GameMetadata.Value.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.RemoteUrl;
                }

                resource.Folder = _fileNameBuilder.GetGameFolder(currentGame, namingConfig);
                resource.AvailablePlatforms = availablePlatforms.Select(p => p.Id).ToList();

                // IGDB has no platforms for some entries, let the user pick any enabled platform
                if (!resource.AvailablePlatforms.Any())
                {
                    resource.AvailablePlatforms = enabledPlatforms.Select(p => p.Id).ToList();
                }

                resource.IsExcluded = listExclusions.Any(e => e.Matches(resource.IgdbId, 0));

                yield return resource;
            }
        }
    }
}
