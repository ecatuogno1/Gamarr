using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.Games
{
    public class Game : ModelBase
    {
        public Game()
        {
            Tags = new HashSet<int>();
            GameMetadata = new GameMetadata();
        }

        public int GameMetadataId { get; set; }

        public bool Monitored { get; set; }
        public GameStatusType MinimumAvailability { get; set; }
        public int QualityProfileId { get; set; }
        public int PlatformId { get; set; }

        public string Path { get; set; }

        public LazyLoaded<GameMetadata> GameMetadata { get; set; }

        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public QualityProfile QualityProfile { get; set; }
        public Platform Platform { get; set; }
        public HashSet<int> Tags { get; set; }
        public AddGameOptions AddOptions { get; set; }
        public DateTime? LastSearchTime { get; set; }
        public GameFile GameFile { get; set; }
        public int GameFileId { get; set; }

        public bool HasFile => GameFileId > 0;

        // compatibility properties
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

        public string FolderName()
        {
            if (Path.IsNullOrWhiteSpace())
            {
                return "";
            }

            // Well what about Path = Null?
            // return new DirectoryInfo(Path).Name;
            return Path;
        }

        public bool IsAvailable(int delay = 0)
        {
            // Games are either announced or released, so "minimum availability" only looks at the release date
            if (MinimumAvailability is GameStatusType.TBA or GameStatusType.Announced)
            {
                return true;
            }

            var releaseDate = GetReleaseDate();

            if (releaseDate.HasValue)
            {
                return DateTime.UtcNow >= releaseDate.Value.AddDays(delay);
            }

            return GameMetadata.Value.Status == GameStatusType.Released;
        }

        public DateTime? GetReleaseDate()
        {
            var metadata = GameMetadata.Value;
            var igdbPlatformId = Platform?.IgdbPlatformId ?? 0;

            if (igdbPlatformId > 0 && metadata.PlatformReleaseDates != null)
            {
                var platformRelease = metadata.PlatformReleaseDates
                    .Where(d => d.IgdbPlatformId == igdbPlatformId && d.Date.HasValue)
                    .Select(d => d.Date)
                    .Min();

                if (platformRelease.HasValue)
                {
                    return platformRelease;
                }
            }

            return metadata.ReleaseDate;
        }

        public override string ToString()
        {
            return string.Format("[{0} ({1})][{2}, {3}]", GameMetadata.Value.Title.NullSafe(), GameMetadata.Value.Year.NullSafe(), GameMetadata.Value.IgdbId, Platform?.ShortName ?? PlatformId.ToString());
        }

        public void ApplyChanges(Game otherGame)
        {
            Path = otherGame.Path;
            QualityProfileId = otherGame.QualityProfileId;

            // Updates that don't send a platform keep the current one
            if (otherGame.PlatformId > 0)
            {
                PlatformId = otherGame.PlatformId;
                Platform = otherGame.Platform;
            }

            Monitored = otherGame.Monitored;
            MinimumAvailability = otherGame.MinimumAvailability;

            RootFolderPath = otherGame.RootFolderPath;
            Tags = otherGame.Tags;
            AddOptions = otherGame.AddOptions;
        }
    }
}
