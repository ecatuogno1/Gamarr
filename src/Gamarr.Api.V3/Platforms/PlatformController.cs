using System.Collections.Generic;
using Gamarr.Http;
using Gamarr.Http.REST;
using Gamarr.Http.REST.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Platforms;

namespace Gamarr.Api.V3.Platforms
{
    [V3ApiController]
    public class PlatformController : RestController<PlatformResource>
    {
        private readonly IPlatformService _platformService;

        public PlatformController(IPlatformService platformService)
        {
            _platformService = platformService;
        }

        protected override PlatformResource GetResourceById(int id)
        {
            return _platformService.Get(id).ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<List<PlatformResource>> GetAll()
        {
            return TypedResults.Ok(_platformService.All().ToResource());
        }

        // Only the Enabled flag can be changed, everything else is seeded reference data
        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public Results<Accepted<PlatformResource>, NotFound> Update([FromBody] PlatformResource resource)
        {
            _platformService.Get(resource.Id);
            _platformService.Update(new Platform { Id = resource.Id, Enabled = resource.Enabled });

            return Accepted(resource.Id);
        }
    }
}
