using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Platforms
{
    // The seeded platform list. Rows are matched to the database by IgdbPlatformId, so the order only
    // matters for fresh databases (PC Windows is first and gets Id 1). Folder names follow ES-DE's
    // system folders. Newznab categories: PC platforms use 4050 (PC/Games), consoles use 1000 plus the
    // console subcategory where one exists.
    //
    // IGDB platform ids marked "unverified" below are from memory and couldn't be checked against the
    // live IGDB /platforms endpoint (no credentials were available). Verify them before relying on them.
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
            P("PC (Microsoft Windows)", "PC", 6, PlatformFamily.PC, 0, "pc", new[] { ".exe", ".iso", ".msi", ".zip", ".7z", ".rar" }, PcGames),
            P("Linux", "Linux", 3, PlatformFamily.PC, 0, "linux", new[] { ".sh", ".deb", ".rpm", ".appimage", ".tar.gz", ".zip" }, PcGames),
            P("Mac", "Mac", 14, PlatformFamily.PC, 0, "macos", new[] { ".dmg", ".pkg", ".app", ".zip" }, PcGames),
            P("DOS", "DOS", 13, PlatformFamily.PC, 0, "dos", new[] { ".exe", ".com", ".bat", ".zip", ".dosz", ".iso" }, PcGames),
            P("Commodore C64/128/MAX", "C64", 15, PlatformFamily.Other, 0, "c64", new[] { ".d64", ".d71", ".d81", ".t64", ".prg", ".crt", ".tap", ".g64" }, PcGames),
            P("Commodore VIC-20", "VIC-20", 71, PlatformFamily.Other, 0, "vic20", new[] { ".prg", ".crt", ".d64", ".tap", ".20", ".a0" }, PcGames),
            P("Amiga", "Amiga", 16, PlatformFamily.Other, 0, "amiga", new[] { ".adf", ".ipf", ".lha", ".hdf", ".dms", ".adz" }, PcGames),
            P("Amiga CD32", "CD32", 114, PlatformFamily.Other, 5, "amigacd32", CdImage, Console),

            // unverified IGDB id
            P("Commodore CDTV", "CDTV", 158, PlatformFamily.Other, 0, "cdtv", CdImage, Console),
            P("Amstrad CPC", "CPC", 25, PlatformFamily.Other, 0, "amstradcpc", new[] { ".dsk", ".cdt", ".sna", ".tap", ".voc" }, PcGames),
            P("ZX Spectrum", "ZX Spectrum", 26, PlatformFamily.Other, 0, "zxspectrum", new[] { ".tzx", ".tap", ".z80", ".sna", ".dsk", ".trd", ".scl" }, PcGames),
            P("MSX", "MSX", 27, PlatformFamily.Other, 0, "msx", new[] { ".rom", ".mx1", ".mx2", ".dsk", ".cas" }, PcGames),
            P("MSX2", "MSX2", 53, PlatformFamily.Other, 0, "msx2", new[] { ".rom", ".mx2", ".dsk", ".cas" }, PcGames),
            P("Atari 8-bit", "Atari 800", 65, PlatformFamily.Atari, 0, "atari800", new[] { ".atr", ".xex", ".xfd", ".car", ".cas", ".bin" }, PcGames),
            P("Atari ST/STE", "Atari ST", 63, PlatformFamily.Atari, 0, "atarist", new[] { ".st", ".stx", ".msa", ".ipf", ".dim" }, PcGames),
            P("Apple II", "Apple II", 75, PlatformFamily.Other, 0, "apple2", new[] { ".dsk", ".do", ".po", ".nib", ".woz", ".2mg" }, PcGames),
            P("BBC Microcomputer System", "BBC Micro", 69, PlatformFamily.Other, 0, "bbcmicro", new[] { ".ssd", ".dsd", ".uef", ".adf" }, PcGames),
            P("Sharp X68000", "X68000", 121, PlatformFamily.Other, 0, "x68000", new[] { ".dim", ".hdf", ".xdf", ".hdm", ".d88", ".2hd" }, PcGames),
            P("Sharp X1", "X1", 77, PlatformFamily.Other, 0, "x1", new[] { ".2d", ".d88", ".dx1", ".tap" }, PcGames),
            P("FM Towns", "FM Towns", 118, PlatformFamily.Other, 0, "fmtowns", new[] { ".cue", ".iso", ".chd", ".bin", ".d88" }, PcGames),
            P("PC-8800 Series", "PC-88", 125, PlatformFamily.NEC, 0, "pc88", new[] { ".d88", ".88d", ".cmt", ".t88" }, PcGames),
            P("PC-9800 Series", "PC-98", 149, PlatformFamily.NEC, 0, "pc98", new[] { ".hdi", ".fdi", ".d98", ".fdd", ".hdm", ".nhd" }, PcGames),

            // Nintendo
            P("Nintendo Entertainment System", "NES", 18, PlatformFamily.Nintendo, 3, "nes", new[] { ".nes", ".unf", ".unif" }, Console),
            P("Family Computer", "Famicom", 99, PlatformFamily.Nintendo, 3, "famicom", new[] { ".nes", ".unf", ".unif" }, Console),
            P("Family Computer Disk System", "FDS", 51, PlatformFamily.Nintendo, 3, "fds", new[] { ".fds", ".nes" }, Console),
            P("Super Nintendo Entertainment System", "SNES", 19, PlatformFamily.Nintendo, 4, "snes", new[] { ".sfc", ".smc", ".fig", ".swc", ".bs" }, Console),
            P("Super Famicom", "SFC", 58, PlatformFamily.Nintendo, 4, "sfc", new[] { ".sfc", ".smc", ".fig", ".swc", ".bs" }, Console),

            // unverified IGDB id
            P("Satellaview", "Satellaview", 306, PlatformFamily.Nintendo, 4, "satellaview", new[] { ".bs", ".sfc", ".smc" }, Console),
            P("Nintendo 64", "N64", 4, PlatformFamily.Nintendo, 5, "n64", new[] { ".z64", ".n64", ".v64" }, Console),

            // unverified IGDB id
            P("Nintendo 64DD", "64DD", 416, PlatformFamily.Nintendo, 5, "n64dd", new[] { ".ndd", ".z64", ".n64" }, Console),
            P("Nintendo GameCube", "GameCube", 21, PlatformFamily.Nintendo, 6, "gc", new[] { ".iso", ".gcm", ".gcz", ".rvz", ".ciso", ".dol", ".elf", ".wia" }, Console),
            P("Wii", "Wii", 5, PlatformFamily.Nintendo, 7, "wii", new[] { ".iso", ".wbfs", ".rvz", ".wia", ".gcz", ".ciso", ".wad" }, Console, ConsoleWii, ConsoleWiiWare),
            P("Wii U", "Wii U", 41, PlatformFamily.Nintendo, 8, "wiiu", new[] { ".wua", ".wux", ".wud", ".rpx" }, Console, ConsoleWiiU),
            P("Nintendo Switch", "Switch", 130, PlatformFamily.Nintendo, 8, "switch", new[] { ".nsp", ".xci", ".nsz", ".xcz" }, Console),
            P("Nintendo Switch 2", "Switch 2", 508, PlatformFamily.Nintendo, 9, "switch2", new[] { ".nsp", ".xci", ".nsz", ".xcz" }, Console),

            // unverified IGDB id
            P("Game & Watch", "G&W", 307, PlatformFamily.Nintendo, 2, "gameandwatch", new[] { ".mgw", ".zip" }, Console),
            P("Game Boy", "GB", 33, PlatformFamily.Nintendo, 4, "gb", new[] { ".gb", ".sgb" }, Console),
            P("Game Boy Color", "GBC", 22, PlatformFamily.Nintendo, 5, "gbc", new[] { ".gbc", ".gb" }, Console),
            P("Game Boy Advance", "GBA", 24, PlatformFamily.Nintendo, 6, "gba", new[] { ".gba", ".agb" }, Console),
            P("Nintendo DS", "NDS", 20, PlatformFamily.Nintendo, 7, "nds", new[] { ".nds", ".dsi", ".ids" }, Console, ConsoleNds),
            P("Nintendo DSi", "DSi", 159, PlatformFamily.Nintendo, 7, "nds", new[] { ".nds", ".dsi" }, Console, ConsoleNds),
            P("Nintendo 3DS", "3DS", 37, PlatformFamily.Nintendo, 8, "n3ds", new[] { ".3ds", ".cia", ".cxi", ".cci", ".3dsx", ".app" }, Console, Console3ds),
            P("New Nintendo 3DS", "New 3DS", 137, PlatformFamily.Nintendo, 8, "n3ds", new[] { ".3ds", ".cia", ".cxi", ".cci", ".3dsx" }, Console, Console3ds),
            P("Virtual Boy", "VB", 87, PlatformFamily.Nintendo, 5, "virtualboy", new[] { ".vb", ".vboy" }, Console),
            P("Pokémon mini", "Poké mini", 166, PlatformFamily.Nintendo, 6, "pokemini", new[] { ".min" }, Console),

            // Sony
            P("PlayStation", "PS1", 7, PlatformFamily.Sony, 5, "psx", new[] { ".cue", ".bin", ".chd", ".pbp", ".iso", ".img", ".m3u", ".ecm", ".ccd" }, Console),
            P("PlayStation 2", "PS2", 8, PlatformFamily.Sony, 6, "ps2", new[] { ".iso", ".chd", ".cso", ".zso", ".bin", ".cue", ".gz" }, Console),
            P("PlayStation 3", "PS3", 9, PlatformFamily.Sony, 7, "ps3", new[] { ".iso", ".pkg", ".ps3" }, Console, ConsolePs3),
            P("PlayStation 4", "PS4", 48, PlatformFamily.Sony, 8, "ps4", new[] { ".pkg" }, Console, ConsolePs4),
            P("PlayStation 5", "PS5", 167, PlatformFamily.Sony, 9, "ps5", new[] { ".pkg" }, Console),
            P("PlayStation Portable", "PSP", 38, PlatformFamily.Sony, 7, "psp", new[] { ".iso", ".cso", ".pbp", ".chd", ".zso" }, Console, ConsolePsp),
            P("PlayStation Vita", "PS Vita", 46, PlatformFamily.Sony, 8, "psvita", new[] { ".vpk", ".pkg" }, Console, ConsolePsVita),

            // Microsoft
            P("Xbox", "Xbox", 11, PlatformFamily.Microsoft, 6, "xbox", new[] { ".iso", ".xiso" }, Console, ConsoleXbox),
            P("Xbox 360", "X360", 12, PlatformFamily.Microsoft, 7, "xbox360", new[] { ".iso", ".xex", ".god", ".zar" }, Console, ConsoleXbox360, ConsoleXbox360Dlc),
            P("Xbox One", "XONE", 49, PlatformFamily.Microsoft, 8, "xboxone", Array.Empty<string>(), Console, ConsoleXboxOne),
            P("Xbox Series X|S", "XSX", 169, PlatformFamily.Microsoft, 9, "xboxseries", Array.Empty<string>(), Console),

            // Sega
            P("SG-1000", "SG-1000", 84, PlatformFamily.Sega, 3, "sg-1000", new[] { ".sg", ".sc" }, Console),
            P("Sega Master System/Mark III", "SMS", 64, PlatformFamily.Sega, 3, "mastersystem", new[] { ".sms" }, Console),
            P("Sega Mega Drive/Genesis", "Genesis", 29, PlatformFamily.Sega, 4, "megadrive", new[] { ".md", ".gen", ".smd", ".bin" }, Console),
            P("Sega CD", "Sega CD", 78, PlatformFamily.Sega, 4, "segacd", CdImage, Console),
            P("Sega 32X", "32X", 30, PlatformFamily.Sega, 4, "sega32x", new[] { ".32x", ".bin" }, Console),

            // unverified IGDB id
            P("Sega Pico", "Pico", 339, PlatformFamily.Sega, 4, "pico", new[] { ".md", ".bin" }, Console),
            P("Sega Saturn", "Saturn", 32, PlatformFamily.Sega, 5, "saturn", CdImage, Console),
            P("Dreamcast", "Dreamcast", 23, PlatformFamily.Sega, 6, "dreamcast", new[] { ".gdi", ".cdi", ".chd", ".cue", ".bin" }, Console),
            P("Sega Game Gear", "Game Gear", 35, PlatformFamily.Sega, 4, "gamegear", new[] { ".gg" }, Console),

            // Arcade and SNK
            P("Arcade", "Arcade", 52, PlatformFamily.Arcade, 0, "arcade", new[] { ".zip", ".7z", ".chd" }, Console),
            P("Neo Geo AES", "Neo Geo", 80, PlatformFamily.SNK, 4, "neogeo", new[] { ".zip", ".7z", ".neo" }, Console),
            P("Neo Geo MVS", "MVS", 79, PlatformFamily.SNK, 4, "neogeo", new[] { ".zip", ".7z", ".neo" }, Console),
            P("Neo Geo CD", "Neo Geo CD", 136, PlatformFamily.SNK, 4, "neogeocd", CdImage, Console),
            P("Neo Geo Pocket", "NGP", 119, PlatformFamily.SNK, 5, "ngp", new[] { ".ngp" }, Console),
            P("Neo Geo Pocket Color", "NGPC", 120, PlatformFamily.SNK, 5, "ngpc", new[] { ".ngc", ".ngpc", ".npc" }, Console),

            // Atari
            P("Atari 2600", "2600", 59, PlatformFamily.Atari, 2, "atari2600", new[] { ".a26", ".bin" }, Console),
            P("Atari 5200", "5200", 66, PlatformFamily.Atari, 2, "atari5200", new[] { ".a52", ".bin", ".car" }, Console),
            P("Atari 7800", "7800", 60, PlatformFamily.Atari, 3, "atari7800", new[] { ".a78", ".bin" }, Console),
            P("Atari Lynx", "Lynx", 61, PlatformFamily.Atari, 4, "atarilynx", new[] { ".lnx", ".o" }, Console),
            P("Atari Jaguar", "Jaguar", 62, PlatformFamily.Atari, 5, "atarijaguar", new[] { ".j64", ".jag", ".rom", ".abs", ".cof" }, Console),

            // unverified IGDB id
            P("Atari Jaguar CD", "Jaguar CD", 410, PlatformFamily.Atari, 5, "atarijaguarcd", CdImage, Console),

            // NEC
            P("TurboGrafx-16/PC Engine", "PCE", 86, PlatformFamily.NEC, 4, "pcengine", new[] { ".pce" }, Console),
            P("Turbografx-16/PC Engine CD", "PCE-CD", 150, PlatformFamily.NEC, 4, "pcenginecd", CdImage, Console),
            P("PC Engine SuperGrafx", "SuperGrafx", 128, PlatformFamily.NEC, 4, "supergrafx", new[] { ".sgx", ".pce" }, Console),

            // unverified IGDB id
            P("PC-FX", "PC-FX", 274, PlatformFamily.NEC, 5, "pcfx", CdImage, Console),

            // Other consoles and handhelds
            P("3DO Interactive Multiplayer", "3DO", 50, PlatformFamily.Other, 5, "3do", CdImage, Console),
            P("Philips CD-i", "CD-i", 117, PlatformFamily.Other, 4, "cdimono1", CdImage, Console),
            P("ColecoVision", "Coleco", 68, PlatformFamily.Other, 2, "colecovision", new[] { ".col", ".rom", ".bin" }, Console),
            P("Intellivision", "Intellivision", 67, PlatformFamily.Other, 2, "intellivision", new[] { ".int", ".rom", ".bin" }, Console),
            P("Vectrex", "Vectrex", 70, PlatformFamily.Other, 2, "vectrex", new[] { ".vec", ".gam", ".bin" }, Console),
            P("Odyssey 2 / Videopac G7000", "Odyssey 2", 133, PlatformFamily.Other, 2, "odyssey2", new[] { ".bin" }, Console),
            P("Fairchild Channel F", "Channel F", 127, PlatformFamily.Other, 2, "channelf", new[] { ".chf", ".bin" }, Console),
            P("WonderSwan", "WS", 57, PlatformFamily.Other, 5, "wonderswan", new[] { ".ws" }, Console),
            P("WonderSwan Color", "WSC", 123, PlatformFamily.Other, 5, "wonderswancolor", new[] { ".wsc" }, Console),
            P("N-Gage", "N-Gage", 42, PlatformFamily.Other, 6, "ngage", new[] { ".n-gage", ".sis", ".zip" }, Console),
            P("Ouya", "Ouya", 72, PlatformFamily.Other, 8, "ouya", new[] { ".apk" }, Console),
            P("Android", "Android", 34, PlatformFamily.Other, 0, "android", new[] { ".apk", ".xapk", ".obb" }, Console),
        };

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
                Family = platform.Family,
                Generation = platform.Generation,
                FolderName = platform.FolderName,
                FileExtensions = platform.FileExtensions.ToList(),
                NewznabCategories = platform.NewznabCategories.ToList(),
                Enabled = platform.Enabled
            };
        }

        private static Platform P(string name, string shortName, int igdbPlatformId, PlatformFamily family, int generation, string folderName, IEnumerable<string> fileExtensions, params int[] newznabCategories)
        {
            return new Platform
            {
                Name = name,
                ShortName = shortName,
                IgdbPlatformId = igdbPlatformId,
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
