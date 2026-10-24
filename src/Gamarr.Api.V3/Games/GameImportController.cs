using System;
using System.Collections.Generic;
using Gamarr.Http;
using Gamarr.Http.REST;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Games;

namespace Gamarr.Api.V3.Games
{
    [V3ApiController("game/import")]
    public class GameImportController : RestController<GameResource>
    {
        private readonly IAddGameService _addGameService;

        public GameImportController(IAddGameService addGameService)
        {
            _addGameService = addGameService;
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

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        public Ok<List<GameResource>> Import([FromBody] List<GameResource> resource)
        {
            var newGames = resource.ToModel();

            return TypedResults.Ok(_addGameService.AddGames(newGames).ToResource(0));
        }
    }
}
