using System.Collections.Generic;

namespace NzbDrone.Core.Games.Credits
{
    // A company credit from IGDB involved_companies (one row per company and role)
    public class Credit : Entity<Credit>
    {
        public Credit()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public string Name { get; set; }

        // "{involvedCompanyId}-{role}", unique per game
        public string CreditIgdbId { get; set; }
        public int CompanyIgdbId { get; set; }
        public int GameMetadataId { get; set; }

        // Company logo
        public List<MediaCover.MediaCover> Images { get; set; }
        public int Order { get; set; }
        public CreditType Type { get; set; }
    }

    public enum CreditType
    {
        Developer = 0,
        Publisher = 1,
        Porting = 2,
        Supporting = 3
    }
}
