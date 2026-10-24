using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Platforms
{
    public class Platform : ModelBase
    {
        public Platform()
        {
            FileExtensions = new List<string>();
            NewznabCategories = new List<int>();
        }

        public string Name { get; set; }
        public string ShortName { get; set; }
        public int IgdbPlatformId { get; set; }

        // IGDB's slug of the platform (igdb.com/platforms/<slug>), used to correct IgdbPlatformId
        public string Slug { get; set; }
        public PlatformFamily Family { get; set; }
        public int Generation { get; set; }
        public string FolderName { get; set; }
        public List<string> FileExtensions { get; set; }
        public List<int> NewznabCategories { get; set; }
        public bool Enabled { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
