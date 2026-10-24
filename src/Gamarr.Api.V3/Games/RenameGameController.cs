using System.Collections.Generic;
using Gamarr.Http;
using Gamarr.Http.REST;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.MediaFiles;

namespace Gamarr.Api.V3.Games
{
    [V3ApiController("rename")]
    public class RenameGameController : Controller
    {
        private readonly IRenameGameFileService _renameGameFileService;

        public RenameGameController(IRenameGameFileService renameGameFileService)
        {
            _renameGameFileService = renameGameFileService;
        }

        [HttpGet]
        [Produces("application/json")]
        public Results<Ok<List<RenameGameResource>>, BadRequest> GetGames([FromQuery(Name = "gameId")] List<int> gameIds)
        {
            if (gameIds is not { Count: not 0 })
            {
                throw new BadRequestException("gameId must be provided");
            }

            return TypedResults.Ok(_renameGameFileService.GetRenamePreviews(gameIds).ToResource());
        }
    }
}
