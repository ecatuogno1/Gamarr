using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public class Quality : IEmbeddedDocument, IEquatable<Quality>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public QualitySource Source { get; set; }

        public Quality()
        {
        }

        private Quality(int id, string name, QualitySource source)
        {
            Id = id;
            Name = name;
            Source = source;
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(Quality other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as Quality);
        }

        public static bool operator ==(Quality left, Quality right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Quality left, Quality right)
        {
            return !Equals(left, right);
        }

        // Quality ids are persisted (profiles, files, history). Radarr's video qualities used 0-31, the game
        // qualities start at 100 so an old id can never be mistaken for a game quality.

        // Unable to determine
        public static Quality Unknown => new Quality(0, "Unknown", QualitySource.UNKNOWN);

        // Cartridge ROMs
        public static Quality RomUnverified => new Quality(100, "ROM-Unverified", QualitySource.CARTRIDGE);
        public static Quality RomVerified => new Quality(101, "ROM-Verified", QualitySource.CARTRIDGE);

        // Disc images
        public static Quality DiscCompressedLossy => new Quality(110, "Disc-Compressed-Lossy", QualitySource.DISC);
        public static Quality DiscIso => new Quality(111, "Disc-ISO", QualitySource.DISC);
        public static Quality DiscChdRvz => new Quality(112, "Disc-CHD-RVZ", QualitySource.DISC);
        public static Quality DiscRedump => new Quality(113, "Disc-Redump", QualitySource.DISC);

        // Console digital packages
        public static Quality NspXci => new Quality(120, "NSP-XCI", QualitySource.DIGITAL);
        public static Quality Pkg => new Quality(121, "PKG", QualitySource.DIGITAL);
        public static Quality WuaWux => new Quality(122, "WUA-WUX", QualitySource.DIGITAL);
        public static Quality Cia3ds => new Quality(123, "CIA-3DS", QualitySource.DIGITAL);
        public static Quality XexGod => new Quality(124, "XEX-GOD", QualitySource.DIGITAL);

        // PC releases
        public static Quality PcPortable => new Quality(130, "PC-Portable", QualitySource.PC);
        public static Quality PcSteamRip => new Quality(131, "PC-Steam-Rip", QualitySource.PC);
        public static Quality PcRepack => new Quality(132, "PC-Repack", QualitySource.PC);
        public static Quality PcScene => new Quality(133, "PC-Scene", QualitySource.PC);
        public static Quality PcGog => new Quality(134, "PC-GOG", QualitySource.PC);

        static Quality()
        {
            All = new List<Quality>
            {
                Unknown,
                RomUnverified,
                RomVerified,
                DiscCompressedLossy,
                DiscIso,
                DiscChdRvz,
                DiscRedump,
                NspXci,
                Pkg,
                WuaWux,
                Cia3ds,
                XexGod,
                PcPortable,
                PcSteamRip,
                PcRepack,
                PcScene,
                PcGog
            };

            AllLookup = new Quality[All.Select(v => v.Id).Max() + 1];
            foreach (var quality in All)
            {
                AllLookup[quality.Id] = quality;
            }

            // Sizes are absolute, in GB. A null max means unlimited, a null preferred size means "largest wins".
            DefaultQualityDefinitions = new HashSet<QualityDefinition>
            {
                new QualityDefinition(Quality.Unknown)             { Weight = 1,  MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.RomUnverified)       { Weight = 2,  MinSize = 0, MaxSize = 2, PreferredSize = null },
                new QualityDefinition(Quality.RomVerified)         { Weight = 3,  MinSize = 0, MaxSize = 2, PreferredSize = null },
                new QualityDefinition(Quality.DiscCompressedLossy) { Weight = 4,  MinSize = 0, MaxSize = 25, PreferredSize = null },
                new QualityDefinition(Quality.DiscIso)             { Weight = 5,  MinSize = 0, MaxSize = 60, PreferredSize = null },
                new QualityDefinition(Quality.DiscChdRvz)          { Weight = 6,  MinSize = 0, MaxSize = 60, PreferredSize = null },
                new QualityDefinition(Quality.DiscRedump)          { Weight = 7,  MinSize = 0, MaxSize = 60, PreferredSize = null },
                new QualityDefinition(Quality.NspXci)              { Weight = 8,  MinSize = 0, MaxSize = 100, PreferredSize = null },
                new QualityDefinition(Quality.Pkg)                 { Weight = 9,  MinSize = 0, MaxSize = 250, PreferredSize = null },
                new QualityDefinition(Quality.WuaWux)              { Weight = 10, MinSize = 0, MaxSize = 50, PreferredSize = null },
                new QualityDefinition(Quality.Cia3ds)              { Weight = 11, MinSize = 0, MaxSize = 10, PreferredSize = null },
                new QualityDefinition(Quality.XexGod)              { Weight = 12, MinSize = 0, MaxSize = 30, PreferredSize = null },
                new QualityDefinition(Quality.PcPortable)          { Weight = 13, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.PcSteamRip)          { Weight = 14, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.PcRepack)            { Weight = 15, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.PcScene)             { Weight = 16, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.PcGog)               { Weight = 17, MinSize = 0, MaxSize = null, PreferredSize = null }
            };
        }

        public static readonly List<Quality> All;

        public static readonly Quality[] AllLookup;

        public static readonly HashSet<QualityDefinition> DefaultQualityDefinitions;
        public static Quality FindById(int id)
        {
            if (id == 0)
            {
                return Unknown;
            }

            var quality = id > 0 && id < AllLookup.Length ? AllLookup[id] : null;

            if (quality == null)
            {
                throw new ArgumentException("ID does not match a known quality", "id");
            }

            return quality;
        }

        public static explicit operator Quality(int id)
        {
            return FindById(id);
        }

        public static explicit operator int(Quality quality)
        {
            return quality.Id;
        }
    }
}
