using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Core.Games;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookGame
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string FilePath { get; set; }
        public string ReleaseDate { get; set; }
        public string FolderPath { get; set; }
        public int IgdbId { get; set; }
        public string IgdbSlug { get; set; }
        public int PlatformId { get; set; }
        public string Platform { get; set; }
        public string Overview { get; set; }
        public List<string> Genres { get; set; }
        public List<WebhookImage> Images { get; set; }
        public List<string> Tags { get; set; }
        public Language OriginalLanguage { get; set; }

        public WebhookGame()
        {
        }

        public WebhookGame(Game game, List<string> tags)
        {
            Id = game.Id;
            Title = game.Title;
            Year = game.Year;
            ReleaseDate = game.GetReleaseDate()?.ToString("yyyy-MM-dd");
            FolderPath = game.Path;
            IgdbId = game.IgdbId;
            IgdbSlug = game.GameMetadata.Value.IgdbSlug;
            PlatformId = game.PlatformId;
            Platform = game.Platform?.Name;
            Overview = game.GameMetadata.Value.Overview;
            Genres = game.GameMetadata.Value.Genres;
            Images = game.GameMetadata.Value.Images.Select(i => new WebhookImage(i)).ToList();
            Tags = tags;
            OriginalLanguage = game.GameMetadata.Value.OriginalLanguage;
        }

        public WebhookGame(Game game, GameFile gameFile, List<string> tags)
            : this(game, tags)
        {
            FilePath = Path.Combine(game.Path, gameFile.RelativePath);
        }
    }
}
