using System.Collections.Generic;
using System.Linq;
using Gamarr.Http.REST;
using NzbDrone.Core.Platforms;

namespace Gamarr.Api.V3.Platforms
{
    public class PlatformResource : RestResource
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int IgdbPlatformId { get; set; }
        public PlatformFamily Family { get; set; }
        public int Generation { get; set; }
        public string FolderName { get; set; }
        public List<string> FileExtensions { get; set; }
        public List<int> NewznabCategories { get; set; }
        public bool Enabled { get; set; }
    }

    public static class PlatformResourceMapper
    {
        public static PlatformResource ToResource(this Platform model)
        {
            if (model == null)
            {
                return null;
            }

            return new PlatformResource
            {
                Id = model.Id,
                Name = model.Name,
                ShortName = model.ShortName,
                IgdbPlatformId = model.IgdbPlatformId,
                Family = model.Family,
                Generation = model.Generation,
                FolderName = model.FolderName,
                FileExtensions = model.FileExtensions,
                NewznabCategories = model.NewznabCategories,
                Enabled = model.Enabled
            };
        }

        public static List<PlatformResource> ToResource(this IEnumerable<Platform> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
