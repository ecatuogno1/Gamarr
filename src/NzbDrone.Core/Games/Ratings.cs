using Equ;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Games
{
    public class Ratings : MemberwiseEquatable<Ratings>, IEmbeddedDocument
    {
        // IGDB user rating (0-100)
        public RatingChild Igdb { get; set; }

        // IGDB aggregated critic rating (0-100)
        public RatingChild Critic { get; set; }
    }

    public class RatingChild : MemberwiseEquatable<RatingChild>
    {
        public int Votes { get; set; }
        public decimal Value { get; set; }
        public RatingType Type { get; set; }
    }

    public enum RatingType
    {
        User,
        Critic
    }
}
