using System;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser
{
    // A game "quality" is the format and dump fidelity of a release. It's detected, in order of confidence, from
    // explicit tags in the name (Redump, GOG, FitGirl, CHD...), the file extension and the platform.
    public class QualityParser
    {
        // PC scene groups, a release from one of them is a cracked PC release
        public const string PcSceneGroups = @"CODEX|RUNE|TENOKE|SKIDROW|SKiDROW|RAZOR1911|Razor|PLAZA|CPY|HOODLUM|FLT|FAIRLIGHT|DARKSiDERS|DARKSIDERS|EMPRESS|TiNYiSO|TINYISO|SiMPLEX|SIMPLEX|DOGE|ANOMALY|PROPHET|RELOADED|HI2U|POSTMORTEM|I_KnoW|Unleashed|PiZZA|DINOByTES|ACTiVATED|ALI213|3DM|RAZORDOX|DEViANCE|Vitality|HATRED|Chronos|ENiGMA|GOLDBERG|DRMFREE|DELiGHT|SiMPLEX|BReWErS|OUTLAWS|ZEKE|KaOs-SCENE|Portable";

        private const string RepackGroups = @"FitGirl|DODI|ElAmigos|KaOs|xatab|Masquerade|R\.?\s?G\.?\s?Mechanics|R\.?G\.?\s?Catalyst|CorePack|Chovka|Decepticon|qoob|Darck|ZAZIX|Kapital\s?Sin|BlackBox|Wanterlude|Gnarly|Tiny\s?Repacks|JohnCena141|M4ckD0ge|IgroMir|Spieler|ARMGDDN|Elamigos|InsaneRamZes|Lazy\s?Repacks|R\.?G\.?\s?Freedom|Scooter|Dyto|nemos";

        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(QualityParser));

        private static readonly Regex RepackRegex = new(@"(?:[\[(]\s*(?:" + RepackGroups + @")(?:[\s._-]+Repack)?\s*[\])]|[\[(]\s*(?:[\w.]+\s+)?Repack(?:ed)?(?:\s+by\s+[\w. ]+)?\s*[\])]|\bRepack(?:ed)?\s+by\s+\w+|(?<=[-._ ])(?:" + RepackGroups + @")(?:[-._ ]Repack)?(?=$|[\])._ \-]))",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RepackGroupNameRegex = new(@"^(?:" + RepackGroups + @")$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex GogRegex = new(@"(?:^|[-._ \[(])GOG(?:[-._ ]?Rip)?(?=$|[-._ \])])|^setup_.+\.(?:exe|bin)$|^gog_.+\.sh$|(?:^|[\[(._ ])DRM[-._ ]?Free(?=$|[\])._ ])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SteamRipRegex = new(@"(?:^|[-._ \[(])Steam[-._ ]?Rip(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PortableRegex = new(@"(?:^|[-._ \[(])(?:Portable|Pre[-._ ]?Installed)(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SceneGroupRegex = new(@"-(?:" + PcSceneGroups + @")$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SceneIsoRegex = new(@"(?:^|[._ \[(])(?:ISO|CLONEDVD)(?=$|[._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RedumpRegex = new(@"(?:^|[-._ \[(])(?:Redump(?:\.org)?)(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex NoIntroRegex = new(@"(?:^|[-._ \[(])No[-._ ]?Intro(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex LossyDiscRegex = new(@"(?:^|[-._ \[(])(?:CSO|ZSO|WBFS|PBP|CISO)(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex LosslessDiscRegex = new(@"(?:^|[-._ \[(])(?:CHD|RVZ|GCZ|WIA)(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex NspRegex = new(@"(?:^|[-._ \[(])(?:NSP|XCI|NSZ|XCZ)(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PkgRegex = new(@"(?:^|[-._ \[(])(?:PKG|PSN|FPKG)(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex WuaRegex = new(@"(?:^|[-._ \[(])(?:WUA|WUX|WUD|Loadiine|WUP)(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex CiaRegex = new(@"(?:^|[-._ \[(])(?:CIA|3DS[-._ ]?ROM)(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // "GOD" (Games on Demand) only in upper case, "God of War" isn't an Xbox 360 package
        private static readonly Regex XexRegex = new(@"(?:^|[-._ \[(])(?:(?i:XEX|XBLA|Games[-._ ]on[-._ ]Demand)|GOD)(?=$|[-._ \])])", RegexOptions.Compiled);

        private static readonly Regex IsoTagRegex = new(@"(?:^|[-._ \[(])(?:ISO|XISO|BIN[-._ ]?CUE)(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // GoodTools verified dump "[!]"
        private static readonly Regex VerifiedDumpRegex = new(@"\[!\]", RegexOptions.Compiled);

        // GoodTools/TOSEC bad, overdump, hack, trainer, fixed, pirate, alternate, translation flags
        private static readonly Regex BadDumpRegex = new(@"\[(?:b|o|h|t|f|p|a|T[+-])\d*[^\]]*\]|\((?:Hack|Pirate|Bootleg|Unl)\)|\[(?:cr|tr|m)\s?[^\]]*\]", RegexOptions.Compiled);

        // No-Intro style region tag with full region names: "(USA)", "(Europe)", "(USA, Europe)", "(World)"
        private static readonly Regex NoIntroRegionRegex = new(@"\((?:World|USA|Europe|Japan|Asia|Australia|Brazil|Canada|China|France|Germany|Hong Kong|Italy|Korea|Netherlands|Russia|Spain|Sweden|Taiwan|UK|Scandinavia|Latin America)(?:,\s?(?:World|USA|Europe|Japan|Asia|Australia|Brazil|Canada|China|France|Germany|Hong Kong|Italy|Korea|Netherlands|Russia|Spain|Sweden|Taiwan|UK|Scandinavia|Latin America))*\)",
            RegexOptions.Compiled);

        private static readonly Regex ProperRegex = new(@"(?:^|[-._ \[(])(?<proper>PROPER)(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Scene repack/rerip (a fixed release). "[FitGirl Repack]" style repacks are a quality, not a revision.
        private static readonly Regex SceneRepackRegex = new(@"(?:^|[._ ])(?<repack>REPACK|RERIP)(?<version>\d)?(?=$|[-._ ])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RealRegex = new(@"(?:^|[._ ])(?<real>REAL)(?=$|[-._ ])", RegexOptions.Compiled);

        // No-Intro "(Rev 1)", "(Rev A)", GoodTools "(PRG1)"
        private static readonly Regex DumpRevisionRegex = new(@"\(Rev\s?(?<rev>\d+|[A-Z])\)|\(PRG(?<rev>\d)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex FileExtensionRegex = new(@"\.[a-z0-9]{1,5}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static QualityModel ParseQuality(string name)
        {
            if (name.IsNullOrWhiteSpace())
            {
                return new QualityModel { Quality = Quality.Unknown };
            }

            var parsed = Parser.ParseGameTitle(name);

            if (parsed?.Quality != null)
            {
                return parsed.Quality;
            }

            return ParseQuality(name, PlatformParser.ParseExtension(GetExtension(name)));
        }

        public static QualityModel ParseQuality(string name, int platformIgdbId)
        {
            Logger.Debug("Trying to parse quality for '{0}'", name);

            if (name.IsNullOrWhiteSpace())
            {
                return new QualityModel { Quality = Quality.Unknown };
            }

            name = name.Trim();

            var fileName = Path.GetFileName(name);
            var extension = GetExtension(fileName);
            var result = ParseRevision(name);

            // Tags are looked for in the name, not the extension (".chd" isn't a "CHD" tag)
            var nameWithoutExtension = extension == null ? fileName : fileName.Substring(0, fileName.Length - extension.Length);
            var tagQuality = ParseTagQuality(nameWithoutExtension, platformIgdbId);

            if (tagQuality != Quality.Unknown)
            {
                result.Quality = tagQuality;
                result.SourceDetectionSource = QualityDetectionSource.Name;

                return result;
            }

            // ROMs: the extension only says "ROM", whether it's verified depends on the naming
            if (IsRom(extension, platformIgdbId, fileName))
            {
                result.Quality = IsVerifiedRom(fileName) ? Quality.RomVerified : Quality.RomUnverified;
                result.SourceDetectionSource = VerifiedDumpRegex.IsMatch(fileName) || NoIntroRegex.IsMatch(fileName) || BadDumpRegex.IsMatch(fileName)
                    ? QualityDetectionSource.Name
                    : QualityDetectionSource.Extension;

                return result;
            }

            var extensionQuality = MediaFileExtensions.GetQualityForExtension(extension);

            if (extensionQuality != Quality.Unknown)
            {
                result.Quality = extensionQuality;
                result.SourceDetectionSource = QualityDetectionSource.Extension;

                return result;
            }

            var platformQuality = GetPlatformQuality(nameWithoutExtension, platformIgdbId);

            if (platformQuality != Quality.Unknown)
            {
                // Only a guess from the platform, as reliable as an extension
                result.Quality = platformQuality;
                result.SourceDetectionSource = QualityDetectionSource.Extension;
            }

            return result;
        }

        public static QualityModel ParseQualityName(string name)
        {
            var normalizedName = name.Replace('_', ' ').Trim();

            return ParseQuality(normalizedName, 0);
        }

        private static Quality ParseTagQuality(string name, int platformIgdbId)
        {
            if (RedumpRegex.IsMatch(name))
            {
                return Quality.DiscRedump;
            }

            if (RepackRegex.IsMatch(name))
            {
                return Quality.PcRepack;
            }

            if (GogRegex.IsMatch(name))
            {
                return Quality.PcGog;
            }

            if (SteamRipRegex.IsMatch(name))
            {
                return Quality.PcSteamRip;
            }

            if (PortableRegex.IsMatch(name))
            {
                return Quality.PcPortable;
            }

            if (LosslessDiscRegex.IsMatch(name))
            {
                return Quality.DiscChdRvz;
            }

            if (LossyDiscRegex.IsMatch(name))
            {
                return Quality.DiscCompressedLossy;
            }

            if (NspRegex.IsMatch(name))
            {
                return Quality.NspXci;
            }

            if (WuaRegex.IsMatch(name))
            {
                return Quality.WuaWux;
            }

            if (CiaRegex.IsMatch(name))
            {
                return Quality.Cia3ds;
            }

            if (XexRegex.IsMatch(name) && (platformIgdbId == 0 || platformIgdbId == PlatformParser.Xbox360))
            {
                return Quality.XexGod;
            }

            if (PkgRegex.IsMatch(name) && (platformIgdbId == 0 || !PlatformParser.IsPcPlatform(platformIgdbId)))
            {
                return Quality.Pkg;
            }

            if ((platformIgdbId == 0 || PlatformParser.IsPcPlatform(platformIgdbId)) && SceneGroupRegex.IsMatch(FileExtensionRegex.Replace(name, string.Empty)))
            {
                return Quality.PcScene;
            }

            if (PlatformParser.IsPcPlatform(platformIgdbId) && SceneIsoRegex.IsMatch(name))
            {
                return Quality.PcScene;
            }

            if (IsoTagRegex.IsMatch(name) && !PlatformParser.IsPcPlatform(platformIgdbId))
            {
                return Quality.DiscIso;
            }

            return Quality.Unknown;
        }

        // The quality a release for a platform has when nothing else is known
        private static Quality GetPlatformQuality(string name, int platformIgdbId)
        {
            if (platformIgdbId == 0)
            {
                return Quality.Unknown;
            }

            if (PlatformParser.IsCartridgePlatform(platformIgdbId))
            {
                return IsVerifiedRom(name) ? Quality.RomVerified : Quality.RomUnverified;
            }

            switch (platformIgdbId)
            {
                case PlatformParser.Switch:
                case PlatformParser.Switch2:
                    return Quality.NspXci;
                case PlatformParser.Ps4:
                case PlatformParser.Ps5:
                case PlatformParser.PsVita:
                    return Quality.Pkg;
                case PlatformParser.WiiU:
                    return Quality.WuaWux;
                case PlatformParser.N3ds:
                case PlatformParser.New3ds:
                    return Quality.Cia3ds;
                case PlatformParser.XboxOne:
                case PlatformParser.XboxSeries:
                    return Quality.Unknown;
            }

            if (PlatformParser.IsPcPlatform(platformIgdbId))
            {
                return Quality.Unknown;
            }

            // Everything else (PS1-3, PSP, GameCube, Wii, Xbox, 360, Saturn, Dreamcast...) is disc based
            return Quality.DiscIso;
        }

        private static bool IsRom(string extension, int platformIgdbId, string name)
        {
            if (extension.IsNullOrWhiteSpace())
            {
                return false;
            }

            var extensionPlatform = PlatformParser.ParseExtension(extension);

            if (extensionPlatform > 0 && PlatformParser.IsCartridgePlatform(extensionPlatform))
            {
                return true;
            }

            // GoodTools sets (Mega Drive, Atari) use .bin with "[!]"/"[b1]" flags, disc images don't have those
            if (platformIgdbId == 0 && extension.Equals(".bin", StringComparison.OrdinalIgnoreCase) && (VerifiedDumpRegex.IsMatch(name) || BadDumpRegex.IsMatch(name)))
            {
                return true;
            }

            // ROMs in archives or with ambiguous extensions (.bin for Mega Drive/Atari)
            if (platformIgdbId > 0 && PlatformParser.IsCartridgePlatform(platformIgdbId))
            {
                return MediaFileExtensions.ArchiveExtensions.Contains(extension) ||
                       MediaFileExtensions.GetQualityForExtension(extension) == Quality.Unknown ||
                       extension.Equals(".bin", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool IsVerifiedRom(string name)
        {
            if (BadDumpRegex.IsMatch(name))
            {
                return false;
            }

            return VerifiedDumpRegex.IsMatch(name) || NoIntroRegex.IsMatch(name) || NoIntroRegionRegex.IsMatch(name);
        }

        private static QualityModel ParseRevision(string name)
        {
            var result = new QualityModel { Quality = Quality.Unknown };

            var revisionMatch = DumpRevisionRegex.Match(name);

            if (revisionMatch.Success)
            {
                var rev = revisionMatch.Groups["rev"].Value;

                // "Rev 1" is the second release of a dump, "Rev A" as well
                result.Revision.Version = int.TryParse(rev, out var revNumber) ? revNumber + 1 : char.ToUpperInvariant(rev[0]) - 'A' + 2;
                result.RevisionDetectionSource = QualityDetectionSource.Name;
            }

            var repackMatch = SceneRepackRegex.Match(name);

            if (repackMatch.Success && !RepackRegex.IsMatch(name))
            {
                result.Revision.Version = repackMatch.Groups["version"].Success ? Convert.ToInt32(repackMatch.Groups["version"].Value) + 1 : 2;
                result.Revision.IsRepack = true;
                result.RevisionDetectionSource = QualityDetectionSource.Name;
            }

            if (ProperRegex.IsMatch(name))
            {
                result.Revision.Version = Math.Max(result.Revision.Version + (repackMatch.Success ? 1 : 0), 2);
                result.RevisionDetectionSource = QualityDetectionSource.Name;
            }

            var realMatches = RealRegex.Matches(name);

            if (realMatches.Count > 0)
            {
                result.Revision.Real = realMatches.Count;
                result.RevisionDetectionSource = QualityDetectionSource.Name;
            }

            return result;
        }

        public static bool IsPcSceneGroup(string releaseGroup)
        {
            return releaseGroup.IsNotNullOrWhiteSpace() && SceneGroupRegex.IsMatch("-" + releaseGroup);
        }

        public static bool IsRepackGroup(string releaseGroup)
        {
            return releaseGroup.IsNotNullOrWhiteSpace() && RepackGroupNameRegex.IsMatch(releaseGroup);
        }

        private static string GetExtension(string name)
        {
            var match = FileExtensionRegex.Match(name);

            if (!match.Success)
            {
                return null;
            }

            var extension = match.Value.ToLowerInvariant();

            return MediaFileExtensions.IsGameFile(extension) || PlatformParser.ParseExtension(extension) > 0 ? extension : null;
        }
    }
}
