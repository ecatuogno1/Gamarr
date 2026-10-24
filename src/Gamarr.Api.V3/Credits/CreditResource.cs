using System.Collections.Generic;
using System.Linq;
using Gamarr.Http.REST;
using NzbDrone.Core.Games.Credits;
using NzbDrone.Core.MediaCover;

namespace Gamarr.Api.V3.Credits
{
    // A company credit (developer, publisher, porting or supporting studio)
    public class CreditResource : RestResource
    {
        public string CompanyName { get; set; }
        public string CreditIgdbId { get; set; }
        public int CompanyIgdbId { get; set; }
        public int GameMetadataId { get; set; }
        public List<MediaCover> Images { get; set; }
        public int Order { get; set; }
        public CreditType Type { get; set; }
    }

    public static class CreditResourceMapper
    {
        public static CreditResource ToResource(this Credit model)
        {
            if (model == null)
            {
                return null;
            }

            return new CreditResource
            {
                Id = model.Id,
                GameMetadataId = model.GameMetadataId,
                CreditIgdbId = model.CreditIgdbId,
                CompanyIgdbId = model.CompanyIgdbId,
                CompanyName = model.Name,
                Order = model.Order,
                Images = model.Images,
                Type = model.Type
            };
        }

        public static List<CreditResource> ToResource(this IEnumerable<Credit> credits)
        {
            return credits.Select(ToResource).ToList();
        }

        public static Credit ToModel(this CreditResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Credit
            {
                Id = resource.Id,
                GameMetadataId = resource.GameMetadataId,
                Name = resource.CompanyName,
                Order = resource.Order,
                Type = resource.Type,
                Images = resource.Images,
                CreditIgdbId = resource.CreditIgdbId,
                CompanyIgdbId = resource.CompanyIgdbId
            };
        }
    }
}
