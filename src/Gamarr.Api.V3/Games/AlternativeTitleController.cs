using System.Collections.Generic;
using Gamarr.Http;
using Gamarr.Http.REST;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.AlternativeTitles;

namespace Gamarr.Api.V3.Games
{
    [V3ApiController("alttitle")]
    public class AlternativeTitleController : RestController<AlternativeTitleResource>
    {
        private readonly IAlternativeTitleService _altTitleService;
        private readonly IGameService _gameService;

        public AlternativeTitleController(IAlternativeTitleService altTitleService, IGameService gameService)
        {
            _altTitleService = altTitleService;
            _gameService = gameService;
        }

        protected override AlternativeTitleResource GetResourceById(int id)
        {
            return _altTitleService.GetById(id).ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<List<AlternativeTitleResource>> GetAltTitles(int? gameId, int? gameMetadataId)
        {
            if (gameMetadataId.HasValue)
            {
                return TypedResults.Ok(_altTitleService.GetAllTitlesForGameMetadata(gameMetadataId.Value).ToResource());
            }

            if (gameId.HasValue)
            {
                var game = _gameService.GetGame(gameId.Value);
                return TypedResults.Ok(_altTitleService.GetAllTitlesForGameMetadata(game.GameMetadataId).ToResource());
            }

            return TypedResults.Ok(_altTitleService.GetAllTitles().ToResource());
        }
    }
}
