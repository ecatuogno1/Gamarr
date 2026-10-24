using System;
using System.Collections.Generic;
using Gamarr.Http.REST;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.Collections;
using NzbDrone.Core.ImportLists.ImportListGames;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;

namespace Gamarr.Api.V3.ImportLists
{
    public class ImportListGamesResource : RestResource
    {
        public ImportListGamesResource()
        {
            Lists = new HashSet<int>();
        }

        public string Title { get; set; }
        public string SortTitle { get; set; }
        public Language OriginalLanguage { get; set; }
        public GameStatusType Status { get; set; }
        public string Overview { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<int> AvailablePlatformIgdbIds { get; set; }
        public List<MediaCover> Images { get; set; }
        public string Website { get; set; }
        public string RemotePoster { get; set; }
        public int Year { get; set; }
        public string YouTubeTrailerId { get; set; }
        public List<string> Developers { get; set; }
        public List<string> Publishers { get; set; }

        public int IgdbId { get; set; }
        public string IgdbSlug { get; set; }

        // 0 when the list doesn't know the platform
        public int PlatformId { get; set; }
        public string Folder { get; set; }
        public float Popularity { get; set; }
        public List<string> Genres { get; set; }
        public Ratings Ratings { get; set; }
        public GameCollection Collection { get; set; }
        public bool IsExcluded { get; set; }
        public bool IsExisting { get; set; }
        public bool IsTrending { get; set; }
        public bool IsPopular { get; set; }

        public bool IsRecommendation { get; set; }
        public HashSet<int> Lists { get; set; }
    }

    public static class DiscoverGamesResourceMapper
    {
        public static ImportListGamesResource ToResource(this Game model)
        {
            if (model == null)
            {
                return null;
            }

            return new ImportListGamesResource
            {
                IgdbId = model.IgdbId,
                Title = model.Title,
                SortTitle = model.GameMetadata.Value.SortTitle,
                OriginalLanguage = model.GameMetadata.Value.OriginalLanguage,
                ReleaseDate = model.GameMetadata.Value.ReleaseDate,
                AvailablePlatformIgdbIds = model.GameMetadata.Value.AvailablePlatformIgdbIds,

                Status = model.GameMetadata.Value.Status,
                Overview = model.GameMetadata.Value.Overview,

                Images = model.GameMetadata.Value.Images,

                Year = model.Year,

                IgdbSlug = model.GameMetadata.Value.IgdbSlug,
                PlatformId = model.PlatformId,
                Website = model.GameMetadata.Value.Website,
                Genres = model.GameMetadata.Value.Genres,
                Ratings = model.GameMetadata.Value.Ratings,
                Popularity = model.GameMetadata.Value.Popularity,
                YouTubeTrailerId = model.GameMetadata.Value.YouTubeTrailerId,
                Collection = new GameCollection { Title = model.GameMetadata.Value.CollectionTitle, IgdbId = model.GameMetadata.Value.CollectionIgdbId },
                Developers = model.GameMetadata.Value.Developers,
                Publishers = model.GameMetadata.Value.Publishers
            };
        }

        public static ImportListGamesResource ToResource(this ImportListGame model)
        {
            if (model == null)
            {
                return null;
            }

            return new ImportListGamesResource
            {
                IgdbId = model.IgdbId,
                Title = model.Title,
                SortTitle = model.GameMetadata.Value.SortTitle,
                OriginalLanguage = model.GameMetadata.Value.OriginalLanguage,
                ReleaseDate = model.GameMetadata.Value.ReleaseDate,
                AvailablePlatformIgdbIds = model.GameMetadata.Value.AvailablePlatformIgdbIds,

                Status = model.GameMetadata.Value.Status,
                Overview = model.GameMetadata.Value.Overview,

                Images = model.GameMetadata.Value.Images,

                Year = model.Year,

                IgdbSlug = model.GameMetadata.Value.IgdbSlug,
                PlatformId = model.PlatformId,
                Website = model.GameMetadata.Value.Website,
                Genres = model.GameMetadata.Value.Genres,
                Ratings = model.GameMetadata.Value.Ratings,
                YouTubeTrailerId = model.GameMetadata.Value.YouTubeTrailerId,
                Popularity = model.GameMetadata.Value.Popularity,
                Developers = model.GameMetadata.Value.Developers,
                Publishers = model.GameMetadata.Value.Publishers,
                Collection = new GameCollection { Title = model.GameMetadata.Value.CollectionTitle, IgdbId = model.GameMetadata.Value.CollectionIgdbId },
                Lists = new HashSet<int> { model.ListId }
            };
        }
    }
}
