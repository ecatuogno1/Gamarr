using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Gamarr.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.Commands;
using NzbDrone.Core.Games.Translations;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Platforms;

namespace Gamarr.Api.V3.Games
{
    [V3ApiController("game/editor")]
    public class GameEditorController : Controller
    {
        private readonly IGameService _gameService;
        private readonly IGameTranslationService _gameTranslationService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IConfigService _configService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly GameEditorValidator _gameEditorValidator;
        private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly IPlatformService _platformService;

        public GameEditorController(IGameService gameService,
            IGameTranslationService gameTranslationService,
            IMapCoversToLocal coverMapper,
            IConfigService configService,
            IManageCommandQueue commandQueueManager,
            GameEditorValidator gameEditorValidator,
            IUpgradableSpecification upgradableSpecification,
            IPlatformService platformService)
        {
            _platformService = platformService;
            _gameService = gameService;
            _gameTranslationService = gameTranslationService;
            _coverMapper = coverMapper;
            _configService = configService;
            _commandQueueManager = commandQueueManager;
            _gameEditorValidator = gameEditorValidator;
            _upgradableSpecification = upgradableSpecification;
        }

        [HttpPut]
        [Consumes("application/json")]
        [Produces("application/json")]
        public Results<Ok<List<GameResource>>, BadRequest> SaveAll([FromBody] GameEditorResource resource)
        {
            var gamesToUpdate = _gameService.GetGames(resource.GameIds);
            var gamesToMove = new List<BulkMoveGame>();
            var platform = resource.PlatformId is > 0 ? _platformService.Find(resource.PlatformId.Value) : null;

            if (resource.PlatformId is > 0)
            {
                ValidatePlatformChange(gamesToUpdate, platform, resource.PlatformId.Value);
            }

            foreach (var game in gamesToUpdate)
            {
                if (resource.Monitored.HasValue)
                {
                    game.Monitored = resource.Monitored.Value;
                }

                if (resource.QualityProfileId.HasValue)
                {
                    game.QualityProfileId = resource.QualityProfileId.Value;
                }

                if (platform != null)
                {
                    game.PlatformId = platform.Id;
                    game.Platform = platform;
                }

                if (resource.MinimumAvailability.HasValue)
                {
                    game.MinimumAvailability = resource.MinimumAvailability.Value;
                }

                if (resource.RootFolderPath.IsNotNullOrWhiteSpace())
                {
                    game.RootFolderPath = resource.RootFolderPath;
                    gamesToMove.Add(new BulkMoveGame
                    {
                        GameId = game.Id,
                        SourcePath = game.Path
                    });
                }

                if (resource.Tags != null)
                {
                    var newTags = resource.Tags;
                    var applyTags = resource.ApplyTags;

                    switch (applyTags)
                    {
                        case ApplyTags.Add:
                            newTags.ForEach(t => game.Tags.Add(t));
                            break;
                        case ApplyTags.Remove:
                            newTags.ForEach(t => game.Tags.Remove(t));
                            break;
                        case ApplyTags.Replace:
                            game.Tags = new HashSet<int>(newTags);
                            break;
                    }
                }

                var validationResult = _gameEditorValidator.Validate(game);

                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
            }

            if (resource.MoveFiles && gamesToMove.Any())
            {
                _commandQueueManager.Push(new BulkMoveGameCommand
                {
                    DestinationRootFolder = resource.RootFolderPath,
                    Games = gamesToMove
                });
            }

            var configLanguage = (Language)_configService.GameInfoLanguage;
            var availabilityDelay = _configService.AvailabilityDelay;

            var translations = _gameTranslationService.GetAllTranslationsForLanguage(configLanguage);
            var tdict = translations.ToDictionaryIgnoreDuplicates(x => x.GameMetadataId);

            var updatedGames = _gameService.UpdateGame(gamesToUpdate, !resource.MoveFiles);

            var gamesResources = new List<GameResource>(updatedGames.Count);

            foreach (var game in updatedGames)
            {
                var translation = GetTranslationFromDict(tdict, game.GameMetadata, configLanguage);
                var gameResource = game.ToResource(availabilityDelay, translation, _upgradableSpecification);

                MapCoversToLocal(gameResource);

                gamesResources.Add(gameResource);
            }

            return TypedResults.Ok(gamesResources);
        }

        [HttpDelete]
        [Consumes("application/json")]
        public Ok<object> DeleteGames([FromBody] GameEditorResource resource)
        {
            _gameService.DeleteGames(resource.GameIds, resource.DeleteFiles, resource.AddImportExclusion);

            return TypedResults.Ok<object>(new { });
        }

        private GameTranslation GetTranslationFromDict(Dictionary<int, GameTranslation> translations, GameMetadata game, Language configLanguage)
        {
            if (configLanguage == Language.Original)
            {
                return new GameTranslation
                {
                    Title = game.OriginalTitle,
                    Overview = game.Overview
                };
            }

            if (!translations.TryGetValue(game.Id, out var translation))
            {
                translation = new GameTranslation
                {
                    Title = game.Title,
                    Language = Language.English
                };
            }

            return translation;
        }

        private void MapCoversToLocal(GameResource game)
        {
            _coverMapper.ConvertToLocalUrls(game.Id, game.Images, game.Added);
        }

        // A game (IGDB id) can only be in the library once per platform
        private void ValidatePlatformChange(List<Game> games, Platform platform, int platformId)
        {
            if (platform == null)
            {
                throw new ValidationException(new[] { new ValidationFailure("PlatformId", $"Platform {platformId} doesn't exist") });
            }

            var selectedIds = games.Select(g => g.Id).ToHashSet();
            var failures = new List<ValidationFailure>();

            foreach (var group in games.GroupBy(g => g.IgdbId))
            {
                if (group.Count() > 1)
                {
                    failures.Add(new ValidationFailure("PlatformId", $"{group.First().Title} is selected on several platforms, only one of them can be moved to {platform.Name}"));
                    continue;
                }

                var existing = _gameService.FindByIgdbIdAndPlatform(group.Key, platform.Id);

                if (existing != null && !selectedIds.Contains(existing.Id))
                {
                    failures.Add(new ValidationFailure("PlatformId", $"{existing.Title} is already in the library on {platform.Name}"));
                }
            }

            if (failures.Any())
            {
                throw new ValidationException(failures);
            }
        }
    }
}
