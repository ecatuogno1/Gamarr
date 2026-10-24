using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Platforms
{
    // The seeded platform list. Rows are matched to the database by IGDB slug (or IgdbPlatformId), so the order only
    // matters for fresh databases (PC Windows is first and gets Id 1). Folder names follow ES-DE's system folders.
    // Newznab categories: PC platforms use 4050 (PC/Games), consoles use 1000 plus the console subcategory where one
    // exists.
    //
    // IGDB ids and slugs were checked against IGDB's platform pages, RomM's IGDB platform list
    // (backend/adapters/services/igdb.py), Gaseous Server's PlatformMap.json and Gameyfin's IGDB PlatformMapper.
    // IGDB renames slugs now and then (Sega CD "segacd" -> "sega-cd", Philips CD-i "philips-cd-i" -> "philips-cdi"),
    // PlatformIgdbSyncService corrects the stored ids by slug (and slugs by id) when IGDB credentials are set.
    public static class PlatformDefinitions
    {
        private const int Console = 1000;
        private const int ConsoleNds = 1010;
        private const int ConsolePsp = 1020;
        private const int ConsoleWii = 1030;
        private const int ConsoleXbox = 1040;
        private const int ConsoleXbox360 = 1050;
        private const int ConsoleWiiWare = 1060;
        private const int ConsoleXbox360Dlc = 1070;
        private const int ConsolePs3 = 1080;
        private const int Console3ds = 1110;
        private const int ConsolePsVita = 1120;
        private const int ConsoleWiiU = 1130;
        private const int ConsoleXboxOne = 1140;
        private const int ConsolePs4 = 1180;
        private const int PcGames = 4050;

        private static readonly string[] CdImage = { ".cue", ".bin", ".chd", ".iso", ".img", ".ccd", ".m3u" };

        public static IReadOnlyList<Platform> All { get; } = new List<Platform>
        {
            // PC and computers
            P("PC (Microsoft Windows)", "PC", 6, "win", PlatformFamily.PC, 0, "pc", new[] { ".exe", ".iso", ".msi", ".zip", ".7z", ".rar" }, PcGames),
            P("Linux", "Linux", 3, "linux", PlatformFamily.PC, 0, "linux", new[] { ".sh", ".deb", ".rpm", ".appimage", ".tar.gz", ".zip" }, PcGames),
            P("Mac", "Mac", 14, "mac", PlatformFamily.PC, 0, "macos", new[] { ".dmg", ".pkg", ".app", ".zip" }, PcGames),
            P("DOS", "DOS", 13, "dos", PlatformFamily.PC, 0, "dos", new[] { ".exe", ".com", ".bat", ".zip", ".dosz", ".iso" }, PcGames),
            P("Commodore C64/128/MAX", "C64", 15, "c64", PlatformFamily.Other, 0, "c64", new[] { ".d64", ".d71", ".d81", ".t64", ".prg", ".crt", ".tap", ".g64" }, PcGames),
            P("Commodore VIC-20", "VIC-20", 71, "vic-20", PlatformFamily.Other, 0, "vic20", new[] { ".prg", ".crt", ".d64", ".tap", ".20", ".a0" }, PcGames),
            P("Amiga", "Amiga", 16, "amiga", PlatformFamily.Other, 0, "amiga", new[] { ".adf", ".ipf", ".lha", ".hdf", ".dms", ".adz" }, PcGames),
            P("Amiga CD32", "CD32", 114, "amiga-cd32", PlatformFamily.Other, 5, "amigacd32", CdImage, Console),
            P("Commodore CDTV", "CDTV", 158, "commodore-cdtv", PlatformFamily.Other, 0, "cdtv", CdImage, Console),
            P("Amstrad CPC", "CPC", 25, "acpc", PlatformFamily.Other, 0, "amstradcpc", new[] { ".dsk", ".cdt", ".sna", ".tap", ".voc" }, PcGames),
            P("ZX Spectrum", "ZX Spectrum", 26, "zxs", PlatformFamily.Other, 0, "zxspectrum", new[] { ".tzx", ".tap", ".z80", ".sna", ".dsk", ".trd", ".scl" }, PcGames),
            P("MSX", "MSX", 27, "msx", PlatformFamily.Other, 0, "msx", new[] { ".rom", ".mx1", ".mx2", ".dsk", ".cas" }, PcGames),
            P("MSX2", "MSX2", 53, "msx2", PlatformFamily.Other, 0, "msx2", new[] { ".rom", ".mx2", ".dsk", ".cas" }, PcGames),
            P("Atari 8-bit", "Atari 800", 65, "atari8bit", PlatformFamily.Atari, 0, "atari800", new[] { ".atr", ".xex", ".xfd", ".car", ".cas", ".bin" }, PcGames),
            P("Atari ST/STE", "Atari ST", 63, "atari-st", PlatformFamily.Atari, 0, "atarist", new[] { ".st", ".stx", ".msa", ".ipf", ".dim" }, PcGames),
            P("Apple II", "Apple II", 75, "appleii", PlatformFamily.Other, 0, "apple2", new[] { ".dsk", ".do", ".po", ".nib", ".woz", ".2mg" }, PcGames),
            P("BBC Microcomputer System", "BBC Micro", 69, "bbcmicro", PlatformFamily.Other, 0, "bbcmicro", new[] { ".ssd", ".dsd", ".uef", ".adf" }, PcGames),
            P("Sharp X68000", "X68000", 121, "sharp-x68000", PlatformFamily.Other, 0, "x68000", new[] { ".dim", ".hdf", ".xdf", ".hdm", ".d88", ".2hd" }, PcGames),
            P("Sharp X1", "X1", 77, "x1", PlatformFamily.Other, 0, "x1", new[] { ".2d", ".d88", ".dx1", ".tap" }, PcGames),
            P("FM Towns", "FM Towns", 118, "fm-towns", PlatformFamily.Other, 0, "fmtowns", new[] { ".cue", ".iso", ".chd", ".bin", ".d88" }, PcGames),
            P("PC-8800 Series", "PC-88", 125, "pc-8800-series", PlatformFamily.NEC, 0, "pc88", new[] { ".d88", ".88d", ".cmt", ".t88" }, PcGames),
            P("PC-9800 Series", "PC-98", 149, "pc-9800-series", PlatformFamily.NEC, 0, "pc98", new[] { ".hdi", ".fdi", ".d98", ".fdd", ".hdm", ".nhd" }, PcGames),

            // Nintendo
            P("Nintendo Entertainment System", "NES", 18, "nes", PlatformFamily.Nintendo, 3, "nes", new[] { ".nes", ".unf", ".unif" }, Console),
            P("Family Computer", "Famicom", 99, "famicom", PlatformFamily.Nintendo, 3, "famicom", new[] { ".nes", ".unf", ".unif" }, Console),
            P("Family Computer Disk System", "FDS", 51, "fds", PlatformFamily.Nintendo, 3, "fds", new[] { ".fds", ".nes" }, Console),
            P("Super Nintendo Entertainment System", "SNES", 19, "snes", PlatformFamily.Nintendo, 4, "snes", new[] { ".sfc", ".smc", ".fig", ".swc", ".bs" }, Console),
            P("Super Famicom", "SFC", 58, "sfam", PlatformFamily.Nintendo, 4, "sfc", new[] { ".sfc", ".smc", ".fig", ".swc", ".bs" }, Console),
            P("Satellaview", "Satellaview", 306, "satellaview", PlatformFamily.Nintendo, 4, "satellaview", new[] { ".bs", ".sfc", ".smc" }, Console),
            P("Nintendo 64", "N64", 4, "n64", PlatformFamily.Nintendo, 5, "n64", new[] { ".z64", ".n64", ".v64" }, Console),
            P("Nintendo 64DD", "64DD", 416, "64dd", PlatformFamily.Nintendo, 5, "n64dd", new[] { ".ndd", ".z64", ".n64" }, Console),
            P("Nintendo GameCube", "GameCube", 21, "ngc", PlatformFamily.Nintendo, 6, "gc", new[] { ".iso", ".gcm", ".gcz", ".rvz", ".ciso", ".dol", ".elf", ".wia" }, Console),
            P("Wii", "Wii", 5, "wii", PlatformFamily.Nintendo, 7, "wii", new[] { ".iso", ".wbfs", ".rvz", ".wia", ".gcz", ".ciso", ".wad" }, Console, ConsoleWii, ConsoleWiiWare),
            P("Wii U", "Wii U", 41, "wiiu", PlatformFamily.Nintendo, 8, "wiiu", new[] { ".wua", ".wux", ".wud", ".rpx" }, Console, ConsoleWiiU),
            P("Nintendo Switch", "Switch", 130, "switch", PlatformFamily.Nintendo, 8, "switch", new[] { ".nsp", ".xci", ".nsz", ".xcz" }, Console),
            P("Nintendo Switch 2", "Switch 2", 508, "switch-2", PlatformFamily.Nintendo, 9, "switch2", new[] { ".nsp", ".xci", ".nsz", ".xcz" }, Console),
            P("Game & Watch", "G&W", 307, "g-and-w", PlatformFamily.Nintendo, 2, "gameandwatch", new[] { ".mgw", ".zip" }, Console),
            P("Game Boy", "GB", 33, "gb", PlatformFamily.Nintendo, 4, "gb", new[] { ".gb", ".sgb" }, Console),
            P("Game Boy Color", "GBC", 22, "gbc", PlatformFamily.Nintendo, 5, "gbc", new[] { ".gbc", ".gb" }, Console),
            P("Game Boy Advance", "GBA", 24, "gba", PlatformFamily.Nintendo, 6, "gba", new[] { ".gba", ".agb" }, Console),
            P("Nintendo DS", "NDS", 20, "nds", PlatformFamily.Nintendo, 7, "nds", new[] { ".nds", ".dsi", ".ids" }, Console, ConsoleNds),
            P("Nintendo DSi", "DSi", 159, "nintendo-dsi", PlatformFamily.Nintendo, 7, "nds", new[] { ".nds", ".dsi" }, Console, ConsoleNds),
            P("Nintendo 3DS", "3DS", 37, "3ds", PlatformFamily.Nintendo, 8, "n3ds", new[] { ".3ds", ".cia", ".cxi", ".cci", ".3dsx", ".app" }, Console, Console3ds),
            P("New Nintendo 3DS", "New 3DS", 137, "new-nintendo-3ds", PlatformFamily.Nintendo, 8, "n3ds", new[] { ".3ds", ".cia", ".cxi", ".cci", ".3dsx" }, Console, Console3ds),
            P("Virtual Boy", "VB", 87, "virtualboy", PlatformFamily.Nintendo, 5, "virtualboy", new[] { ".vb", ".vboy" }, Console),
            P("Pokémon mini", "Poké mini", 166, "pokemon-mini", PlatformFamily.Nintendo, 6, "pokemini", new[] { ".min" }, Console),

            // Sony
            P("PlayStation", "PS1", 7, "ps", PlatformFamily.Sony, 5, "psx", new[] { ".cue", ".bin", ".chd", ".pbp", ".iso", ".img", ".m3u", ".ecm", ".ccd" }, Console),
            P("PlayStation 2", "PS2", 8, "ps2", PlatformFamily.Sony, 6, "ps2", new[] { ".iso", ".chd", ".cso", ".zso", ".bin", ".cue", ".gz" }, Console),
            P("PlayStation 3", "PS3", 9, "ps3", PlatformFamily.Sony, 7, "ps3", new[] { ".iso", ".pkg", ".ps3" }, Console, ConsolePs3),
            P("PlayStation 4", "PS4", 48, "ps4--1", PlatformFamily.Sony, 8, "ps4", new[] { ".pkg" }, Console, ConsolePs4),
            P("PlayStation 5", "PS5", 167, "ps5", PlatformFamily.Sony, 9, "ps5", new[] { ".pkg" }, Console),
            P("PlayStation Portable", "PSP", 38, "psp", PlatformFamily.Sony, 7, "psp", new[] { ".iso", ".cso", ".pbp", ".chd", ".zso" }, Console, ConsolePsp),
            P("PlayStation Vita", "PS Vita", 46, "psvita", PlatformFamily.Sony, 8, "psvita", new[] { ".vpk", ".pkg" }, Console, ConsolePsVita),

            // Microsoft
            P("Xbox", "Xbox", 11, "xbox", PlatformFamily.Microsoft, 6, "xbox", new[] { ".iso", ".xiso" }, Console, ConsoleXbox),
            P("Xbox 360", "X360", 12, "xbox360", PlatformFamily.Microsoft, 7, "xbox360", new[] { ".iso", ".xex", ".god", ".zar" }, Console, ConsoleXbox360, ConsoleXbox360Dlc),
            P("Xbox One", "XONE", 49, "xboxone", PlatformFamily.Microsoft, 8, "xboxone", Array.Empty<string>(), Console, ConsoleXboxOne),
            P("Xbox Series X|S", "XSX", 169, "series-x-s", PlatformFamily.Microsoft, 9, "xboxseries", Array.Empty<string>(), Console),

            // Sega
            P("SG-1000", "SG-1000", 84, "sg1000", PlatformFamily.Sega, 3, "sg-1000", new[] { ".sg", ".sc" }, Console),
            P("Sega Master System/Mark III", "SMS", 64, "sms", PlatformFamily.Sega, 3, "mastersystem", new[] { ".sms" }, Console),
            P("Sega Mega Drive/Genesis", "Genesis", 29, "genesis-slash-megadrive", PlatformFamily.Sega, 4, "megadrive", new[] { ".md", ".gen", ".smd", ".bin" }, Console),
            P("Sega CD", "Sega CD", 78, "sega-cd", PlatformFamily.Sega, 4, "segacd", CdImage, Console),
            P("Sega 32X", "32X", 30, "sega32", PlatformFamily.Sega, 4, "sega32x", new[] { ".32x", ".bin" }, Console),
            P("Sega Pico", "Pico", 339, "sega-pico", PlatformFamily.Sega, 4, "pico", new[] { ".md", ".bin" }, Console),
            P("Sega Saturn", "Saturn", 32, "saturn", PlatformFamily.Sega, 5, "saturn", CdImage, Console),
            P("Dreamcast", "Dreamcast", 23, "dc", PlatformFamily.Sega, 6, "dreamcast", new[] { ".gdi", ".cdi", ".chd", ".cue", ".bin" }, Console),
            P("Sega Game Gear", "Game Gear", 35, "gamegear", PlatformFamily.Sega, 4, "gamegear", new[] { ".gg" }, Console),

            // Arcade and SNK
            P("Arcade", "Arcade", 52, "arcade", PlatformFamily.Arcade, 0, "arcade", new[] { ".zip", ".7z", ".chd" }, Console),
            P("Neo Geo AES", "Neo Geo", 80, "neogeoaes", PlatformFamily.SNK, 4, "neogeo", new[] { ".zip", ".7z", ".neo" }, Console),
            P("Neo Geo MVS", "MVS", 79, "neogeomvs", PlatformFamily.SNK, 4, "neogeo", new[] { ".zip", ".7z", ".neo" }, Console),
            P("Neo Geo CD", "Neo Geo CD", 136, "neo-geo-cd", PlatformFamily.SNK, 4, "neogeocd", CdImage, Console),
            P("Neo Geo Pocket", "NGP", 119, "neo-geo-pocket", PlatformFamily.SNK, 5, "ngp", new[] { ".ngp" }, Console),
            P("Neo Geo Pocket Color", "NGPC", 120, "neo-geo-pocket-color", PlatformFamily.SNK, 5, "ngpc", new[] { ".ngc", ".ngpc", ".npc" }, Console),

            // Atari
            P("Atari 2600", "2600", 59, "atari2600", PlatformFamily.Atari, 2, "atari2600", new[] { ".a26", ".bin" }, Console),
            P("Atari 5200", "5200", 66, "atari5200", PlatformFamily.Atari, 2, "atari5200", new[] { ".a52", ".bin", ".car" }, Console),
            P("Atari 7800", "7800", 60, "atari7800", PlatformFamily.Atari, 3, "atari7800", new[] { ".a78", ".bin" }, Console),
            P("Atari Lynx", "Lynx", 61, "lynx", PlatformFamily.Atari, 4, "atarilynx", new[] { ".lnx", ".o" }, Console),
            P("Atari Jaguar", "Jaguar", 62, "jaguar", PlatformFamily.Atari, 5, "atarijaguar", new[] { ".j64", ".jag", ".rom", ".abs", ".cof" }, Console),
            P("Atari Jaguar CD", "Jaguar CD", 410, "atari-jaguar-cd", PlatformFamily.Atari, 5, "atarijaguarcd", CdImage, Console),

            // NEC
            P("TurboGrafx-16/PC Engine", "PCE", 86, "turbografx16--1", PlatformFamily.NEC, 4, "pcengine", new[] { ".pce" }, Console),
            P("Turbografx-16/PC Engine CD", "PCE-CD", 150, "turbografx-16-slash-pc-engine-cd", PlatformFamily.NEC, 4, "pcenginecd", CdImage, Console),
            P("PC Engine SuperGrafx", "SuperGrafx", 128, "supergrafx", PlatformFamily.NEC, 4, "supergrafx", new[] { ".sgx", ".pce" }, Console),
            P("PC-FX", "PC-FX", 274, "pc-fx", PlatformFamily.NEC, 5, "pcfx", CdImage, Console),

            // Other consoles and handhelds
            P("3DO Interactive Multiplayer", "3DO", 50, "3do", PlatformFamily.Other, 5, "3do", CdImage, Console),
            P("Philips CD-i", "CD-i", 117, "philips-cdi", PlatformFamily.Other, 4, "cdimono1", CdImage, Console),
            P("ColecoVision", "Coleco", 68, "colecovision", PlatformFamily.Other, 2, "colecovision", new[] { ".col", ".rom", ".bin" }, Console),
            P("Intellivision", "Intellivision", 67, "intellivision", PlatformFamily.Other, 2, "intellivision", new[] { ".int", ".rom", ".bin" }, Console),
            P("Vectrex", "Vectrex", 70, "vectrex", PlatformFamily.Other, 2, "vectrex", new[] { ".vec", ".gam", ".bin" }, Console),
            P("Odyssey 2 / Videopac G7000", "Odyssey 2", 133, "odyssey-2-slash-videopac-g7000", PlatformFamily.Other, 2, "odyssey2", new[] { ".bin" }, Console),
            P("Fairchild Channel F", "Channel F", 127, "fairchild-channel-f", PlatformFamily.Other, 2, "channelf", new[] { ".chf", ".bin" }, Console),
            P("WonderSwan", "WS", 57, "wonderswan", PlatformFamily.Other, 5, "wonderswan", new[] { ".ws" }, Console),
            P("WonderSwan Color", "WSC", 123, "wonderswan-color", PlatformFamily.Other, 5, "wonderswancolor", new[] { ".wsc" }, Console),
            P("N-Gage", "N-Gage", 42, "ngage", PlatformFamily.Other, 6, "ngage", new[] { ".n-gage", ".sis", ".zip" }, Console),
            P("Ouya", "Ouya", 72, "ouya", PlatformFamily.Other, 8, "ouya", new[] { ".apk" }, Console),
            P("Android", "Android", 34, "android", PlatformFamily.Other, 0, "android", new[] { ".apk", ".xapk", ".obb" }, Console),
        };

        public static Platform GetBySlug(string slug)
        {
            var platform = All.SingleOrDefault(p => p.Slug == slug);

            return platform == null ? null : Copy(platform);
        }

        public static Platform Get(int igdbPlatformId)
        {
            var platform = All.SingleOrDefault(p => p.IgdbPlatformId == igdbPlatformId);

            return platform == null ? null : Copy(platform);
        }

        public static Platform Copy(Platform platform)
        {
            return new Platform
            {
                Name = platform.Name,
                ShortName = platform.ShortName,
                IgdbPlatformId = platform.IgdbPlatformId,
                Slug = platform.Slug,
                Family = platform.Family,
                Generation = platform.Generation,
                FolderName = platform.FolderName,
                FileExtensions = platform.FileExtensions.ToList(),
                NewznabCategories = platform.NewznabCategories.ToList(),
                Enabled = platform.Enabled
            };
        }

        private static Platform P(string name, string shortName, int igdbPlatformId, string slug, PlatformFamily family, int generation, string folderName, IEnumerable<string> fileExtensions, params int[] newznabCategories)
        {
            return new Platform
            {
                Name = name,
                ShortName = shortName,
                IgdbPlatformId = igdbPlatformId,
                Slug = slug,
                Family = family,
                Generation = generation,
                FolderName = folderName,
                FileExtensions = fileExtensions.ToList(),
                NewznabCategories = newznabCategories.ToList(),
                Enabled = true
            };
        }
    }
}
