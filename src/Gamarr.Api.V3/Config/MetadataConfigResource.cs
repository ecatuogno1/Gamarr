using Gamarr.Http.REST;
using NzbDrone.Core.Configuration;

namespace Gamarr.Api.V3.Config
{
    public class MetadataConfigResource : RestResource
    {
        public string IgdbClientId { get; set; }
        public string IgdbClientSecret { get; set; }
    }

    public static class MetadataConfigResourceMapper
    {
        public static MetadataConfigResource ToResource(IConfigService model)
        {
            return new MetadataConfigResource
            {
                IgdbClientId = model.IgdbClientId,
                IgdbClientSecret = model.IgdbClientSecret,
            };
        }
    }
}
