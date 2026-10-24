using NzbDrone.Core.Games;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookRemoteGame
    {
        public int IgdbId { get; set; }
        public string IgdbSlug { get; set; }
        public int PlatformId { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }

        public WebhookRemoteGame()
        {
        }

        public WebhookRemoteGame(RemoteGame remoteGame)
        {
            IgdbId = remoteGame.Game.GameMetadata.Value.IgdbId;
            IgdbSlug = remoteGame.Game.GameMetadata.Value.IgdbSlug;
            PlatformId = remoteGame.Game.PlatformId;
            Title = remoteGame.Game.GameMetadata.Value.Title;
            Year = remoteGame.Game.GameMetadata.Value.Year;
        }

        public WebhookRemoteGame(Game game)
        {
            IgdbId = game.GameMetadata.Value.IgdbId;
            IgdbSlug = game.GameMetadata.Value.IgdbSlug;
            PlatformId = game.PlatformId;
            Title = game.GameMetadata.Value.Title;
            Year = game.GameMetadata.Value.Year;
        }
    }
}
