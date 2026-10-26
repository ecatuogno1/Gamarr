using Gamarr.Http;
using Gamarr.Http.REST.Attributes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;

namespace Gamarr.Api.V3.Config
{
    [V3ApiController("config/metadata")]
    public class MetadataConfigController : ConfigController<MetadataConfigResource>
    {
        public MetadataConfigController(IConfigService configService)
            : base(configService)
        {
        }

        [RestPutById]
        [Consumes("application/json")]
        public override Results<Accepted<MetadataConfigResource>, NotFound> SaveConfig([FromBody] MetadataConfigResource resource)
        {
            // The client only has the masked secret, sent back unchanged it keeps the stored one
            if (resource.IgdbClientSecret == MetadataConfigResourceMapper.PrivateValue)
            {
                resource.IgdbClientSecret = _configService.IgdbClientSecret;
            }

            return base.SaveConfig(resource);
        }

        protected override MetadataConfigResource ToResource(IConfigService model)
        {
            return MetadataConfigResourceMapper.ToResource(model);
        }
    }
}
