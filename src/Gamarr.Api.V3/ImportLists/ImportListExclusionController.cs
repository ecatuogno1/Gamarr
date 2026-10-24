using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Gamarr.Http;
using Gamarr.Http.Extensions;
using Gamarr.Http.REST;
using Gamarr.Http.REST.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.ImportLists.ImportExclusions;

namespace Gamarr.Api.V3.ImportLists
{
    [V3ApiController("exclusions")]
    public class ImportListExclusionController : RestController<ImportListExclusionResource>
    {
        private readonly IImportListExclusionService _importListExclusionService;

        public ImportListExclusionController(IImportListExclusionService importListExclusionService,
                                             ImportListExclusionExistsValidator importListExclusionExistsValidator)
        {
            _importListExclusionService = importListExclusionService;

            SharedValidator.RuleFor(c => c.IgdbId).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .SetValidator(importListExclusionExistsValidator);

            SharedValidator.RuleFor(c => c.GameTitle).NotEmpty();
            SharedValidator.RuleFor(c => c.GameYear).GreaterThanOrEqualTo(0);
        }

        [HttpGet]
        [Produces("application/json")]
        [Obsolete("Deprecated")]
        public Ok<List<ImportListExclusionResource>> GetImportListExclusions()
        {
            return TypedResults.Ok(_importListExclusionService.All().ToResource());
        }

        protected override ImportListExclusionResource GetResourceById(int id)
        {
            return _importListExclusionService.Get(id).ToResource();
        }

        [HttpGet("paged")]
        [Produces("application/json")]
        public Ok<PagingResource<ImportListExclusionResource>> GetImportListExclusionsPaged([FromQuery] PagingRequestResource paging)
        {
            var pagingResource = new PagingResource<ImportListExclusionResource>(paging);
            var pageSpec = pagingResource.MapToPagingSpec<ImportListExclusionResource, ImportListExclusion>(
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "id",
                    "gameTitle",
                    "gameYear",
                    "igdbId"
                },
                "id",
                SortDirection.Descending);

            return TypedResults.Ok(pageSpec.ApplyToPage(_importListExclusionService.Paged, ImportListExclusionResourceMapper.ToResource));
        }

        [RestPostById]
        [Consumes("application/json")]
        public Results<Created<ImportListExclusionResource>, NotFound> AddImportListExclusion([FromBody] ImportListExclusionResource resource)
        {
            var importListExclusion = _importListExclusionService.Add(resource.ToModel());

            return Created(importListExclusion.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        public Results<Accepted<ImportListExclusionResource>, NotFound> UpdateImportListExclusion([FromBody] ImportListExclusionResource resource)
        {
            _importListExclusionService.Update(resource.ToModel());
            return Accepted(resource.Id);
        }

        [HttpPost("bulk")]
        [Consumes("application/json")]
        public Ok<List<ImportListExclusionResource>> AddImportListExclusions([FromBody] List<ImportListExclusionResource> resources)
        {
            var importListExclusions = _importListExclusionService.Add(resources.ToModel());

            return TypedResults.Ok(importListExclusions.ToResource());
        }

        [RestDeleteById]
        public Ok DeleteImportListExclusion(int id)
        {
            _importListExclusionService.Delete(id);

            return TypedResults.Ok();
        }

        [HttpDelete("bulk")]
        [Consumes("application/json")]
        public Ok<object> DeleteImportListExclusions([FromBody] ImportListExclusionBulkResource resource)
        {
            _importListExclusionService.Delete(resource.Ids.ToList());

            return TypedResults.Ok<object>(new { });
        }
    }
}
