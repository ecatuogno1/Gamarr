using Gamarr.Http.REST;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;

namespace Gamarr.Api.V3.Config
{
    public class MetadataConfigResource : RestResource
    {
        public IGDBCountryCode CertificationCountry { get; set; }
    }

    public static class MetadataConfigResourceMapper
    {
        public static MetadataConfigResource ToResource(IConfigService model)
        {
            return new MetadataConfigResource
            {
                CertificationCountry = model.CertificationCountry,
            };
        }
    }
}
