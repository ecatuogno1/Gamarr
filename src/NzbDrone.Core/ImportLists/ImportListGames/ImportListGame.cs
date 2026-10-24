using NzbDrone.Core.Datastore;
using NzbDrone.Core.Games;

namespace NzbDrone.Core.ImportLists.ImportListGames
{
    public class ImportListGame : ModelBase
    {
        public ImportListGame()
        {
            GameMetadata = new GameMetadata();
        }

        public int ListId { get; set; }
        public int GameMetadataId { get; set; }
        public LazyLoaded<GameMetadata> GameMetadata { get; set; }

        // 0 when the list doesn't know the platform, the first enabled IGDB platform is used when adding
        public int PlatformId { get; set; }

        public string Title
        {
            get { return GameMetadata.Value.Title; }
            set { GameMetadata.Value.Title = value; }
        }

        public int IgdbId
        {
            get { return GameMetadata.Value.IgdbId; }
            set { GameMetadata.Value.IgdbId = value; }
        }

        public int Year
        {
            get { return GameMetadata.Value.Year; }
            set { GameMetadata.Value.Year = value; }
        }
    }
}
