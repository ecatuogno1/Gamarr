using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles
{
    // Game file extensions: the union of every platform's extensions plus generic disc, archive and installer
    // formats, and the quality a bare extension implies.
    public static class MediaFileExtensions
    {
        // Formats that are valid for any platform's game (archives of ROMs, generic disc images)
        private static readonly HashSet<string> GenericFileExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".zip",
            ".7z",
            ".rar",
            ".iso",
            ".bin",
            ".cue",
            ".img",
            ".ccd",
            ".sub",
            ".mds",
            ".mdf",
            ".chd",
            ".ecm",
            ".m3u"
        };

        // Formats that aren't tied to one platform, but are recognised as game files
        private static readonly HashSet<string> OtherGameFileExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".rvz",
            ".gcz",
            ".wbfs",
            ".wia",
            ".ciso",
            ".cso",
            ".zso",
            ".pbp",
            ".gdi",
            ".cdi",
            ".nrg",
            ".exe",
            ".msi",
            ".pkg",
            ".nsp",
            ".nsz",
            ".xci",
            ".xcz",
            ".3ds",
            ".cia",
            ".wua",
            ".wux",
            ".wud",
            ".rpx",
            ".xex",
            ".god",
            ".vpk",
            ".dmg",
            ".sh"
        };

        private static readonly Dictionary<string, Quality> ExtensionQualities = new(StringComparer.OrdinalIgnoreCase)
        {
            // Lossy or scrubbed disc images
            { ".cso", Quality.DiscCompressedLossy },
            { ".zso", Quality.DiscCompressedLossy },
            { ".ciso", Quality.DiscCompressedLossy },
            { ".pbp", Quality.DiscCompressedLossy },
            { ".wbfs", Quality.DiscCompressedLossy },

            // Full disc images
            { ".iso", Quality.DiscIso },
            { ".cue", Quality.DiscIso },
            { ".bin", Quality.DiscIso },
            { ".img", Quality.DiscIso },
            { ".ccd", Quality.DiscIso },
            { ".mds", Quality.DiscIso },
            { ".mdf", Quality.DiscIso },
            { ".gdi", Quality.DiscIso },
            { ".cdi", Quality.DiscIso },
            { ".nrg", Quality.DiscIso },
            { ".gcm", Quality.DiscIso },
            { ".xiso", Quality.DiscIso },
            { ".ecm", Quality.DiscIso },
            { ".m3u", Quality.DiscIso },

            // Lossless compressed disc images
            { ".chd", Quality.DiscChdRvz },
            { ".rvz", Quality.DiscChdRvz },
            { ".gcz", Quality.DiscChdRvz },
            { ".wia", Quality.DiscChdRvz },

            // Console digital
            { ".nsp", Quality.NspXci },
            { ".nsz", Quality.NspXci },
            { ".xci", Quality.NspXci },
            { ".xcz", Quality.NspXci },
            { ".pkg", Quality.Pkg },
            { ".vpk", Quality.Pkg },
            { ".wua", Quality.WuaWux },
            { ".wux", Quality.WuaWux },
            { ".wud", Quality.WuaWux },
            { ".rpx", Quality.WuaWux },
            { ".cia", Quality.Cia3ds },
            { ".3ds", Quality.Cia3ds },
            { ".cci", Quality.Cia3ds },
            { ".cxi", Quality.Cia3ds },
            { ".3dsx", Quality.Cia3ds },
            { ".god", Quality.XexGod },
        };

        // Primary file selection for multi-file releases, lower is preferred
        private static readonly Dictionary<string, int> PrimaryPriorities = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".m3u", 0 },
            { ".cue", 1 },
            { ".gdi", 2 },
            { ".ccd", 3 },
            { ".mds", 3 },
            { ".cdi", 3 }
        };

        private static readonly HashSet<string> CompanionOnlyExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".bin",
            ".img",
            ".sub",
            ".mdf",
            ".ecm"
        };

        private static readonly HashSet<string> KeepNameExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe",
            ".msi",
            ".sh",
            ".dmg",
            ".bat",
            ".ccd",
            ".mds"
        };

        private static readonly Lazy<HashSet<string>> AllExtensions = new(() =>
        {
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            extensions.UnionWith(GenericFileExtensions);
            extensions.UnionWith(OtherGameFileExtensions);
            extensions.UnionWith(ExtensionQualities.Keys);

            foreach (var platform in PlatformDefinitions.All)
            {
                extensions.UnionWith(platform.FileExtensions.Where(e => e.StartsWith('.') && e.Count(c => c == '.') == 1));
            }

            return extensions;
        });

        public static HashSet<string> Extensions => new(AllExtensions.Value, StringComparer.OrdinalIgnoreCase);

        public static HashSet<string> GenericExtensions => new(GenericFileExtensions, StringComparer.OrdinalIgnoreCase);

        public static HashSet<string> ArchiveExtensions => new(new[] { ".zip", ".7z", ".rar" }, StringComparer.OrdinalIgnoreCase);

        public static HashSet<string> InstallerExtensions => new(new[] { ".exe", ".msi", ".sh", ".dmg", ".pkg" }, StringComparer.OrdinalIgnoreCase);

        public static bool IsGameFile(string extension)
        {
            return extension != null && AllExtensions.Value.Contains(extension);
        }

        // Whether a file with this extension can be a game file of a game on the given platform:
        // the platform's own extensions plus generic archives and disc images.
        public static bool IsAllowedForPlatform(string extension, Platform platform)
        {
            if (extension == null)
            {
                return false;
            }

            if (GenericFileExtensions.Contains(extension))
            {
                return true;
            }

            if (platform == null || platform.FileExtensions == null || !platform.FileExtensions.Any())
            {
                return IsGameFile(extension);
            }

            return platform.FileExtensions.Any(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }

        // Files that are only ever part of another file's set (bin tracks of a cue sheet etc.)
        public static bool IsCompanionOnly(string extension)
        {
            return extension != null && CompanionOnlyExtensions.Contains(extension);
        }

        // Installers look for their data files by their own name (GOG's setup_x.exe and setup_x-1.bin), CloneCD and
        // Alcohol images need the same name for all their files. These primary files keep their original name.
        public static bool KeepsOriginalName(string extension)
        {
            return extension != null && KeepNameExtensions.Contains(extension);
        }

        public static int GetPrimaryPriority(string extension)
        {
            return extension != null && PrimaryPriorities.TryGetValue(extension, out var priority) ? priority : 10;
        }

        public static Quality GetQualityForExtension(string extension)
        {
            if (extension == null)
            {
                return Quality.Unknown;
            }

            return ExtensionQualities.TryGetValue(extension, out var quality) ? quality : Quality.Unknown;
        }
    }
}
