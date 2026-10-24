using System;
using System.Collections.Generic;
using Gamarr.Api.V3.Games;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.Translations;
using NzbDrone.Core.MediaCover;

namespace Gamarr.Api.V3.Collections
{
    public class CollectionGameResource
    {
        public int IgdbId { get; set; }
        public string IgdbSlug { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public GameStatusType Status { get; set; }
        public string Overview { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<int> AvailablePlatformIgdbIds { get; set; }
        public List<MediaCover> Images { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }
        public List<string> Genres { get; set; }
        public string Folder { get; set; }
        public bool IsExisting { get; set; }
        public bool IsExcluded { get; set; }
    }

    public static class CollectionGameResourceMapper
    {
        public static CollectionGameResource ToResource(this GameMetadata model, GameTranslation gameTranslation = null)
        {
            if (model == null)
            {
                return null;
            }

            var translatedTitle = gameTranslation?.Title ?? model.Title;
            var translatedOverview = gameTranslation?.Overview ?? model.Overview;

            return new CollectionGameResource
            {
                IgdbId = model.IgdbId,
                Title = translatedTitle,
                Status = model.Status,
                Overview = translatedOverview,
                SortTitle = model.SortTitle,
                Images = model.Images,
                IgdbSlug = model.IgdbSlug,
                Ratings = model.Ratings,
                ReleaseDate = model.ReleaseDate.ToReleaseDay(),
                AvailablePlatformIgdbIds = model.AvailablePlatformIgdbIds,
                CleanTitle = model.CleanTitle,
                Genres = model.Genres,
                Year = model.Year
            };
        }
    }
}
