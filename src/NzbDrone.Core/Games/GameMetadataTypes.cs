using System;
using Equ;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Games
{
    public class AgeRating : MemberwiseEquatable<AgeRating>, IEmbeddedDocument
    {
        // Rating board, e.g. ESRB, PEGI, CERO, USK, GRAC, CLASS_IND, ACB
        public string Organization { get; set; }

        // Rating within the board, e.g. "E10+", "M", "12", "18"
        public string Rating { get; set; }
    }

    public class GameWebsite : MemberwiseEquatable<GameWebsite>, IEmbeddedDocument
    {
        public string Url { get; set; }

        // Website type, e.g. Official, Steam, GOG, Epic, Itch, Wikipedia
        public string Type { get; set; }
    }

    public class PlatformReleaseDate : MemberwiseEquatable<PlatformReleaseDate>, IEmbeddedDocument
    {
        public int IgdbPlatformId { get; set; }
        public DateTime? Date { get; set; }
    }
}
