using Gamarr.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Games;
using NzbDrone.Core.Organizer;

namespace Gamarr.Api.V3.Games;

[V3ApiController("game")]
public class GameFolderController : Controller
{
    private readonly IGameService _gameService;
    private readonly IBuildFileNames _fileNameBuilder;

    public GameFolderController(IGameService gameService, IBuildFileNames fileNameBuilder)
    {
        _gameService = gameService;
        _fileNameBuilder = fileNameBuilder;
    }

    [HttpGet("{id:int}/folder")]
    [Produces("application/json")]
    public Ok<GameFolderResource> GetFolder([FromRoute] int id)
    {
        var series = _gameService.GetGame(id);
        var folder = _fileNameBuilder.GetGameFolder(series);

        return TypedResults.Ok(new GameFolderResource
        {
            Folder = folder
        });
    }
}
