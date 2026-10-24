using System.Collections.Generic;
using Gamarr.Http;
using Gamarr.Http.REST;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.Credits;
using NzbDrone.Core.MediaCover;

namespace Gamarr.Api.V3.Credits
{
    [V3ApiController]
    public class CreditController : RestController<CreditResource>
    {
        private readonly ICreditService _creditService;
        private readonly IGameService _gameService;
        private readonly IMapCoversToLocal _coverMapper;

        public CreditController(ICreditService creditService, IGameService gameService, IMapCoversToLocal coverMapper)
        {
            _creditService = creditService;
            _gameService = gameService;
            _coverMapper = coverMapper;
        }

        protected override CreditResource GetResourceById(int id)
        {
            return _creditService.GetById(id).ToResource();
        }

        [HttpGet]
        public Ok<IEnumerable<CreditResource>> GetCredits(int? gameId, int? gameMetadataId)
        {
            if (gameMetadataId.HasValue)
            {
                return TypedResults.Ok(MapToResource(_creditService.GetAllCreditsForGameMetadata(gameMetadataId.Value)));
            }

            if (gameId.HasValue)
            {
                var game = _gameService.GetGame(gameId.Value);

                return TypedResults.Ok(MapToResource(_creditService.GetAllCreditsForGameMetadata(game.GameMetadataId)));
            }

            return TypedResults.Ok(MapToResource(_creditService.GetAllCredits()));
        }

        private IEnumerable<CreditResource> MapToResource(IEnumerable<Credit> credits)
        {
            foreach (var currentCredits in credits)
            {
                var resource = currentCredits.ToResource();
                _coverMapper.ConvertToLocalUrls(0, resource.Images);

                yield return resource;
            }
        }
    }
}
