using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Gamarr.Api.V3.GameFiles;
using Gamarr.Http.REST;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.Translations;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Platforms;
using Swashbuckle.AspNetCore.Annotations;

namespace Gamarr.Api.V3.Games
{
    public class GameResource : RestResource
    {
        public GameResource()
        {
            Monitored = true;
            MinimumAvailability = GameStatusType.Released;
        }

        // Todo: Sorters should be done completely on the client
        // Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        // Todo: We should get the entire Profile instead of ID and Name separately

        // View Only
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public Language OriginalLanguage { get; set; }
        public List<AlternativeTitleResource> AlternateTitles { get; set; }
        public int? SecondaryYear { get; set; }
        public int SecondaryYearSourceId { get; set; }
        public string SortTitle { get; set; }
        public long? SizeOnDisk { get; set; }
        public GameStatusType Status { get; set; }
        public string Overview { get; set; }
        public string Storyline { get; set; }

        // Release date on this game's platform (falls back to IGDB first_release_date)
        public DateTime? ReleaseDate { get; set; }

        // IGDB first_release_date across all platforms
        public DateTime? FirstReleaseDate { get; set; }
        public List<PlatformReleaseDate> PlatformReleaseDates { get; set; }
        public List<int> AvailablePlatformIgdbIds { get; set; }
        public List<MediaCover> Images { get; set; }
        public string Website { get; set; }
        public List<GameWebsite> Websites { get; set; }

        // public bool Downloaded { get; set; }
        public string RemotePoster { get; set; }
        public int Year { get; set; }
        public string YouTubeTrailerId { get; set; }
        public List<string> Developers { get; set; }
        public List<string> Publishers { get; set; }

        // View & Edit
        public string Path { get; set; }
        public int QualityProfileId { get; set; }

        // Required when adding a game, a title can be added once per platform
        public int PlatformId { get; set; }
        public GamePlatformResource Platform { get; set; }

        // Lookup only: the enabled Gamarr platforms (ids) IGDB lists for the game
        public List<int> AvailablePlatforms { get; set; }

        // Compatibility
        public bool? HasFile { get; set; }
        public int GameFileId { get; set; }

        // Editing Only
        public bool Monitored { get; set; }
        public GameStatusType MinimumAvailability { get; set; }
        public bool IsAvailable { get; set; }
        public string FolderName { get; set; }

        public string CleanTitle { get; set; }
        public int IgdbId { get; set; }
        public string IgdbSlug { get; set; }
        public string IgdbUrl { get; set; }
        public string TitleSlug { get; set; }
        public string RootFolderPath { get; set; }
        public string Folder { get; set; }
        public List<string> Genres { get; set; }
        public List<string> Themes { get; set; }
        public List<string> GameModes { get; set; }
        public List<string> Keywords { get; set; }
        public List<AgeRating> AgeRatings { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public AddGameOptions AddOptions { get; set; }
        public Ratings Ratings { get; set; }
        public GameFileResource GameFile { get; set; }
        public GameCollectionResource Collection { get; set; }
        public float Popularity { get; set; }
        public DateTime? LastSearchTime { get; set; }
        public GameStatisticsResource Statistics { get; set; }

        // Hiding this so people don't think its usable (only used to set the initial state)
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [SwaggerIgnore]
        public bool Grabbed { get; set; }

        // Hiding this so people don't think its usable (only used for searches)
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [SwaggerIgnore]
        public bool IsExcluded { get; set; }
    }

    public static class GameResourceMapper
    {
        public static GameResource ToResource(this Game model, int availDelay, GameTranslation gameTranslation = null, IUpgradableSpecification upgradableSpecification = null, ICustomFormatCalculationService formatCalculationService = null)
        {
            if (model == null)
            {
                return null;
            }

            var gameFile = model.GameFile?.ToResource(model, upgradableSpecification, formatCalculationService);

            var translatedTitle = gameTranslation?.Title ?? model.Title;
            var translatedOverview = gameTranslation?.Overview ?? model.GameMetadata.Value.Overview;

            var collection = model.GameMetadata.Value.CollectionIgdbId > 0 ? new GameCollectionResource { Title = model.GameMetadata.Value.CollectionTitle, IgdbId = model.GameMetadata.Value.CollectionIgdbId } : null;

            return new GameResource
            {
                Id = model.Id,
                IgdbId = model.IgdbId,
                Title = translatedTitle,
                OriginalTitle = model.GameMetadata.Value.OriginalTitle,
                OriginalLanguage = model.GameMetadata.Value.OriginalLanguage,
                SortTitle = GameTitleNormalizer.Normalize(translatedTitle, model.IgdbId),
                ReleaseDate = model.GetReleaseDate().ToReleaseDay(),
                FirstReleaseDate = model.GameMetadata.Value.ReleaseDate.ToReleaseDay(),
                PlatformReleaseDates = model.GameMetadata.Value.PlatformReleaseDates.ToReleaseDays(),
                AvailablePlatformIgdbIds = model.GameMetadata.Value.AvailablePlatformIgdbIds,

                Status = model.GameMetadata.Value.Status,
                Overview = translatedOverview,
                Storyline = model.GameMetadata.Value.Storyline,

                Images = model.GameMetadata.Value.Images.JsonClone(),

                Year = model.Year,
                SecondaryYear = model.GameMetadata.Value.SecondaryYear,

                GameFileId = model.GameFileId,

                Path = model.Path,
                QualityProfileId = model.QualityProfileId,
                PlatformId = model.PlatformId,
                Platform = model.Platform.ToGamePlatformResource(),

                Monitored = model.Monitored,
                MinimumAvailability = model.MinimumAvailability,

                IsAvailable = model.IsAvailable(availDelay),
                FolderName = model.FolderName(),

                CleanTitle = model.GameMetadata.Value.CleanTitle,
                IgdbSlug = model.GameMetadata.Value.IgdbSlug,
                IgdbUrl = model.GameMetadata.Value.IgdbUrl,

                // IgdbId isn't unique in the library anymore (one game per platform), so the slug includes the platform
                TitleSlug = model.PlatformId > 0 ? $"{model.GameMetadata.Value.IgdbId}-{model.PlatformId}" : model.GameMetadata.Value.IgdbId.ToString(),
                RootFolderPath = model.RootFolderPath,
                Website = model.GameMetadata.Value.Website,
                Websites = model.GameMetadata.Value.Websites,
                Genres = model.GameMetadata.Value.Genres,
                Themes = model.GameMetadata.Value.Themes,
                GameModes = model.GameMetadata.Value.GameModes,
                Keywords = model.GameMetadata.Value.Keywords,
                AgeRatings = model.GameMetadata.Value.AgeRatings,
                Developers = model.GameMetadata.Value.Developers,
                Publishers = model.GameMetadata.Value.Publishers,
                Tags = model.Tags,
                Added = model.Added,
                AddOptions = model.AddOptions,
                AlternateTitles = model.GameMetadata.Value.AlternativeTitles.ToResource(),
                Ratings = model.GameMetadata.Value.Ratings,
                GameFile = gameFile,
                YouTubeTrailerId = model.GameMetadata.Value.YouTubeTrailerId,
                Collection = collection,
                Popularity = model.GameMetadata.Value.Popularity,
                LastSearchTime = model.LastSearchTime,
            };
        }

        public static Game ToModel(this GameResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Game
            {
                Id = resource.Id,

                GameMetadata = new GameMetadata
                {
                    IgdbId = resource.IgdbId,
                    Title = resource.Title,
                    Genres = resource.Genres,
                    Images = resource.Images,
                    OriginalTitle = resource.OriginalTitle,
                    SortTitle = resource.SortTitle,
                    ReleaseDate = resource.FirstReleaseDate,
                    Year = resource.Year,
                    SecondaryYear = resource.SecondaryYear,
                    Overview = resource.Overview,
                    Website = resource.Website,
                    Ratings = resource.Ratings,
                    YouTubeTrailerId = resource.YouTubeTrailerId,
                    CleanTitle = resource.CleanTitle,
                    IgdbSlug = resource.IgdbSlug,
                },

                Path = resource.Path,
                QualityProfileId = resource.QualityProfileId,
                PlatformId = resource.PlatformId,

                Monitored = resource.Monitored,
                MinimumAvailability = resource.MinimumAvailability,

                RootFolderPath = resource.RootFolderPath,

                Tags = resource.Tags ?? new HashSet<int>(),
                Added = resource.Added,
                AddOptions = resource.AddOptions
            };
        }

        public static Game ToModel(this GameResource resource, Game game)
        {
            var updatedGame = resource.ToModel();

            game.ApplyChanges(updatedGame);

            return game;
        }

        public static List<GameResource> ToResource(this IEnumerable<Game> games, int availDelay, IUpgradableSpecification upgradableSpecification = null, ICustomFormatCalculationService formatCalculationService = null)
        {
            return games.Select(x => ToResource(x, availDelay, null, upgradableSpecification, formatCalculationService)).ToList();
        }

        public static GamePlatformResource ToGamePlatformResource(this Platform platform)
        {
            if (platform == null)
            {
                return null;
            }

            return new GamePlatformResource
            {
                Id = platform.Id,
                Name = platform.Name,
                ShortName = platform.ShortName,
                FolderName = platform.FolderName,
                IgdbPlatformId = platform.IgdbPlatformId,
                Slug = platform.Slug
            };
        }

        public static List<Game> ToModel(this IEnumerable<GameResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
