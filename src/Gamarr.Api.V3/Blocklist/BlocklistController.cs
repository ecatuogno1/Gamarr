using System;
using System.Collections.Generic;
using System.Linq;
using Gamarr.Http;
using Gamarr.Http.Extensions;
using Gamarr.Http.REST.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;

namespace Gamarr.Api.V3.Blocklist
{
    [V3ApiController]
    public class BlocklistController : Controller
    {
        private readonly IBlocklistService _blocklistService;
        private readonly ICustomFormatCalculationService _formatCalculator;

        public BlocklistController(IBlocklistService blocklistService,
                                   ICustomFormatCalculationService formatCalculator)
        {
            _blocklistService = blocklistService;
            _formatCalculator = formatCalculator;
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<PagingResource<BlocklistResource>> GetBlocklist([FromQuery] PagingRequestResource paging, [FromQuery] int[] gameIds = null, [FromQuery] DownloadProtocol[] protocols = null)
        {
            var pagingResource = new PagingResource<BlocklistResource>(paging);
            var pagingSpec = pagingResource.MapToPagingSpec<BlocklistResource, NzbDrone.Core.Blocklisting.Blocklist>(
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "date",
                    "indexer",
                    "languages",
                    "gameMetadata.sortTitle",
                    "quality",
                    "sourceTitle"
                },
                "date",
                SortDirection.Descending);

            if (gameIds?.Any() == true)
            {
                pagingSpec.FilterExpressions.Add(b => gameIds.Contains(b.GameId));
            }

            if (protocols?.Any() == true)
            {
                pagingSpec.FilterExpressions.Add(b => protocols.Contains(b.Protocol));
            }

            return TypedResults.Ok(pagingSpec.ApplyToPage(b => _blocklistService.Paged(pagingSpec), b => BlocklistResourceMapper.MapToResource(b, _formatCalculator)));
        }

        [HttpGet("game")]
        [Produces("application/json")]
        public Ok<List<BlocklistResource>> GetGameBlocklist(int gameId)
        {
            return TypedResults.Ok(_blocklistService.GetByGameId(gameId).Select(h => BlocklistResourceMapper.MapToResource(h, _formatCalculator)).ToList());
        }

        [RestDeleteById]
        public Ok DeleteBlocklist(int id)
        {
            _blocklistService.Delete(id);

            return TypedResults.Ok();
        }

        [HttpDelete("bulk")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public Ok<object> Remove([FromBody] BlocklistBulkResource resource)
        {
            _blocklistService.Delete(resource.Ids);

            return TypedResults.Ok<object>(new { });
        }
    }
}
