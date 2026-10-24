using Gamarr.Http.REST;
using NzbDrone.Core.Configuration;

namespace Gamarr.Api.V3.Config
{
    public class ImportListConfigResource : RestResource
    {
        public string ListSyncLevel { get; set; }
    }

    public static class ImportListConfigResourceMapper
    {
        public static ImportListConfigResource ToResource(IConfigService model)
        {
            return new ImportListConfigResource
            {
                ListSyncLevel = model.ListSyncLevel,
            };
        }
    }
}
