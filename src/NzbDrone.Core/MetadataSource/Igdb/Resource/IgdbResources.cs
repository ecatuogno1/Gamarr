using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NzbDrone.Core.MetadataSource.Igdb.Resource
{
    // IGDB returns either a bare id or an expanded object for reference fields, depending on the query.
    // IgdbReference accepts both, and has the fields of the small lookup tables we expand
    // (genres, themes, game_modes, game_statuses, game_types, website_types, age rating organizations/categories).
    [JsonConverter(typeof(IgdbReferenceConverter))]
    public class IgdbReference
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        // game_statuses.status
        [JsonProperty("status")]
        public string Status { get; set; }

        // game_types.type, website_types.type
        [JsonProperty("type")]
        public string Type { get; set; }

        // age_rating_categories.rating
        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("image_id")]
        public string ImageId { get; set; }
    }

    public class IgdbReferenceConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IgdbReference);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            switch (token.Type)
            {
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;
                case JTokenType.Integer:
                    return new IgdbReference { Id = token.Value<int>() };
                case JTokenType.Object:
                    var reference = new IgdbReference();
                    serializer.Populate(token.CreateReader(), reference);
                    return reference;
                default:
                    return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }

    public class IgdbImage
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("image_id")]
        public string ImageId { get; set; }
    }

    public class IgdbCompany
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("logo")]
        public IgdbImage Logo { get; set; }
    }

    public class IgdbInvolvedCompany
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("company")]
        public IgdbCompany Company { get; set; }

        [JsonProperty("developer")]
        public bool Developer { get; set; }

        [JsonProperty("publisher")]
        public bool Publisher { get; set; }

        [JsonProperty("porting")]
        public bool Porting { get; set; }

        [JsonProperty("supporting")]
        public bool Supporting { get; set; }
    }

    public class IgdbReleaseDate
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        // Unix timestamp (seconds)
        [JsonProperty("date")]
        public long? Date { get; set; }

        [JsonProperty("platform")]
        public IgdbReference Platform { get; set; }
    }

    public class IgdbAlternativeName
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }

    public class IgdbAgeRating
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        // Deprecated enum fields (pre-2025)
        [JsonProperty("category")]
        public int? Category { get; set; }

        [JsonProperty("rating")]
        public int? Rating { get; set; }

        // Replacement reference fields (age_rating_organizations / age_rating_categories)
        [JsonProperty("organization")]
        public IgdbReference Organization { get; set; }

        [JsonProperty("rating_category")]
        public IgdbReference RatingCategory { get; set; }
    }

    public class IgdbWebsite
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        // Deprecated enum (pre-2025)
        [JsonProperty("category")]
        public int? Category { get; set; }

        // Replacement reference to website_types
        [JsonProperty("type")]
        public IgdbReference Type { get; set; }
    }

    public class IgdbVideo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("video_id")]
        public string VideoId { get; set; }
    }

    public class IgdbGameResource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("storyline")]
        public string Storyline { get; set; }

        [JsonProperty("first_release_date")]
        public long? FirstReleaseDate { get; set; }

        [JsonProperty("updated_at")]
        public long? UpdatedAt { get; set; }

        [JsonProperty("cover")]
        public IgdbImage Cover { get; set; }

        [JsonProperty("artworks")]
        public List<IgdbImage> Artworks { get; set; }

        [JsonProperty("screenshots")]
        public List<IgdbImage> Screenshots { get; set; }

        [JsonProperty("genres")]
        public List<IgdbReference> Genres { get; set; }

        [JsonProperty("themes")]
        public List<IgdbReference> Themes { get; set; }

        [JsonProperty("game_modes")]
        public List<IgdbReference> GameModes { get; set; }

        [JsonProperty("keywords")]
        public List<IgdbReference> Keywords { get; set; }

        [JsonProperty("platforms")]
        public List<IgdbReference> Platforms { get; set; }

        [JsonProperty("release_dates")]
        public List<IgdbReleaseDate> ReleaseDates { get; set; }

        [JsonProperty("involved_companies")]
        public List<IgdbInvolvedCompany> InvolvedCompanies { get; set; }

        // Deprecated single collection, replaced by "collections"
        [JsonProperty("collection")]
        public IgdbReference Collection { get; set; }

        [JsonProperty("collections")]
        public List<IgdbReference> Collections { get; set; }

        [JsonProperty("franchises")]
        public List<IgdbReference> Franchises { get; set; }

        [JsonProperty("alternative_names")]
        public List<IgdbAlternativeName> AlternativeNames { get; set; }

        [JsonProperty("age_ratings")]
        public List<IgdbAgeRating> AgeRatings { get; set; }

        [JsonProperty("websites")]
        public List<IgdbWebsite> Websites { get; set; }

        [JsonProperty("videos")]
        public List<IgdbVideo> Videos { get; set; }

        [JsonProperty("similar_games")]
        public List<IgdbReference> SimilarGames { get; set; }

        [JsonProperty("rating")]
        public double? Rating { get; set; }

        [JsonProperty("rating_count")]
        public int? RatingCount { get; set; }

        [JsonProperty("aggregated_rating")]
        public double? AggregatedRating { get; set; }

        [JsonProperty("aggregated_rating_count")]
        public int? AggregatedRatingCount { get; set; }

        [JsonProperty("total_rating_count")]
        public int? TotalRatingCount { get; set; }

        [JsonProperty("hypes")]
        public int? Hypes { get; set; }

        // Deprecated enum, replaced by game_status
        [JsonProperty("status")]
        public int? Status { get; set; }

        [JsonProperty("game_status")]
        public IgdbReference GameStatus { get; set; }

        // Deprecated enum, replaced by game_type
        [JsonProperty("category")]
        public int? Category { get; set; }

        [JsonProperty("game_type")]
        public IgdbReference GameType { get; set; }

        [JsonProperty("version_parent")]
        public IgdbReference VersionParent { get; set; }

        [JsonProperty("parent_game")]
        public IgdbReference ParentGame { get; set; }
    }

    public class IgdbCollectionResource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("games")]
        public List<IgdbReference> Games { get; set; }
    }

    public class IgdbSearchResource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("game")]
        public IgdbReference Game { get; set; }
    }

    public class TwitchTokenResource
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}
