using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ImportLists.ImportExclusions
{
    public class ImportListExclusion : ModelBase
    {
        public int IgdbId { get; set; }

        // 0 excludes the game on every platform
        public int PlatformId { get; set; }
        public string GameTitle { get; set; }
        public int GameYear { get; set; }

        public bool Matches(int igdbId, int platformId)
        {
            return IgdbId == igdbId && (PlatformId == 0 || platformId == 0 || PlatformId == platformId);
        }

        public new string ToString()
        {
            return string.Format("Excluded Game: [{0}][{1}][{2} {3}]", IgdbId, PlatformId, GameTitle, GameYear);
        }
    }
}
