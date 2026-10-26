using Gamarr.Http.REST;
using NzbDrone.Common.Extensions;
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
        // Sent instead of the client secret, like other secrets; sent back it keeps the stored secret
        public const string PrivateValue = "********";

        public static MetadataConfigResource ToResource(IConfigService model)
        {
            return new MetadataConfigResource
            {
                IgdbClientId = model.IgdbClientId,
                IgdbClientSecret = model.IgdbClientSecret.IsNullOrWhiteSpace() ? string.Empty : PrivateValue,
            };
        }
    }
}
