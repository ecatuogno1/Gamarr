using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Core.Platforms;

namespace NzbDrone.Core.Parser
{
    // Detects the platform of a release. Platforms are identified by their IGDB platform id because that's
    // stable across databases, PlatformService maps it to the Gamarr platform id.
    public static class PlatformParser
    {
        public const int PcWindows = 6;
        public const int Linux = 3;
        public const int Mac = 14;
        public const int Dos = 13;
        public const int Nes = 18;
        public const int Famicom = 99;
        public const int Fds = 51;
        public const int Snes = 19;
        public const int SuperFamicom = 58;
        public const int N64 = 4;
        public const int GameCube = 21;
        public const int Wii = 5;
        public const int WiiU = 41;
        public const int Switch = 130;
        public const int Switch2 = 508;
        public const int GameBoy = 33;
        public const int GameBoyColor = 22;
        public const int GameBoyAdvance = 24;
        public const int Nds = 20;
        public const int Dsi = 159;
        public const int N3ds = 37;
        public const int New3ds = 137;
        public const int VirtualBoy = 87;
        public const int PokemonMini = 166;
        public const int Ps1 = 7;
        public const int Ps2 = 8;
        public const int Ps3 = 9;
        public const int Ps4 = 48;
        public const int Ps5 = 167;
        public const int Psp = 38;
        public const int PsVita = 46;
        public const int Xbox = 11;
        public const int Xbox360 = 12;
        public const int XboxOne = 49;
        public const int XboxSeries = 169;
        public const int Sg1000 = 84;
        public const int MasterSystem = 64;
        public const int Genesis = 29;
        public const int SegaCd = 78;
        public const int Sega32X = 30;
        public const int Saturn = 32;
        public const int Dreamcast = 23;
        public const int GameGear = 35;
        public const int Arcade = 52;
        public const int NeoGeoAes = 80;
        public const int NeoGeoMvs = 79;
        public const int NeoGeoCd = 136;
        public const int NeoGeoPocket = 119;
        public const int NeoGeoPocketColor = 120;
        public const int Atari2600 = 59;
        public const int Atari5200 = 66;
        public const int Atari7800 = 60;
        public const int Lynx = 61;
        public const int Jaguar = 62;
        public const int PcEngine = 86;
        public const int PcEngineCd = 150;
        public const int SuperGrafx = 128;
        public const int ThreeDo = 50;
        public const int CdI = 117;
        public const int ColecoVision = 68;
        public const int Intellivision = 67;
        public const int Vectrex = 70;
        public const int WonderSwan = 57;
        public const int WonderSwanColor = 123;
        public const int C64 = 15;
        public const int Amiga = 16;
        public const int ZxSpectrum = 26;
        public const int NGage = 42;
        public const int Android = 34;

        // Has to be initialized before the alias table that uses it
        private static readonly Regex NonAlphanumericRegex = new(@"[^a-z0-9]", RegexOptions.Compiled);

        // Tokens used by scene and indexer release names ("Game.Name.NSW-VENOM", "Game_Name_PS4-DUPLEX").
        // Keys are lower case without separators, multi-word tokens ("Wii.U", "Xbox.360") are matched joined.
        private static readonly Dictionary<string, int> SceneTokens = new(StringComparer.OrdinalIgnoreCase)
        {
            { "pc", PcWindows },
            { "win", PcWindows },
            { "win32", PcWindows },
            { "win64", PcWindows },
            { "windows", PcWindows },
            { "linux", Linux },
            { "mac", Mac },
            { "macos", Mac },
            { "osx", Mac },
            { "dos", Dos },
            { "msdos", Dos },
            { "nes", Nes },
            { "famicom", Nes },
            { "fds", Fds },
            { "snes", Snes },
            { "sfc", Snes },
            { "superfamicom", Snes },
            { "n64", N64 },
            { "ngc", GameCube },
            { "gc", GameCube },
            { "gamecube", GameCube },
            { "wii", Wii },
            { "wiiu", WiiU },
            { "nsw", Switch },
            { "switch", Switch },
            { "nintendoswitch", Switch },
            { "ns2", Switch2 },
            { "switch2", Switch2 },
            { "gb", GameBoy },
            { "gameboy", GameBoy },
            { "gbc", GameBoyColor },
            { "gba", GameBoyAdvance },
            { "nds", Nds },
            { "dsi", Dsi },
            { "3ds", N3ds },
            { "n3ds", N3ds },
            { "psx", Ps1 },
            { "ps1", Ps1 },
            { "psone", Ps1 },
            { "ps2", Ps2 },
            { "ps3", Ps3 },
            { "ps4", Ps4 },
            { "ps5", Ps5 },
            { "psp", Psp },
            { "psv", PsVita },
            { "psvita", PsVita },
            { "vita", PsVita },
            { "xbox", Xbox },
            { "xbox360", Xbox360 },
            { "x360", Xbox360 },
            { "xboxone", XboxOne },
            { "xone", XboxOne },
            { "xb1", XboxOne },
            { "xsx", XboxSeries },
            { "xboxseries", XboxSeries },
            { "xboxseriesx", XboxSeries },
            { "sms", MasterSystem },
            { "mastersystem", MasterSystem },
            { "md", Genesis },
            { "megadrive", Genesis },
            { "genesis", Genesis },
            { "segacd", SegaCd },
            { "megacd", SegaCd },
            { "32x", Sega32X },
            { "saturn", Saturn },
            { "dc", Dreamcast },
            { "dreamcast", Dreamcast },
            { "gamegear", GameGear },
            { "mame", Arcade },
            { "arcade", Arcade },
            { "neogeo", NeoGeoAes },
            { "ngcd", NeoGeoCd },
            { "ngp", NeoGeoPocket },
            { "ngpc", NeoGeoPocketColor },
            { "atari2600", Atari2600 },
            { "a2600", Atari2600 },
            { "atari7800", Atari7800 },
            { "lynx", Lynx },
            { "jaguar", Jaguar },
            { "pce", PcEngine },
            { "pcengine", PcEngine },
            { "tg16", PcEngine },
            { "turbografx", PcEngine },
            { "turbografx16", PcEngine },
            { "pcecd", PcEngineCd },
            { "3do", ThreeDo },
            { "cdi", CdI },
            { "wsc", WonderSwanColor },
            { "c64", C64 },
            { "amiga", Amiga },
            { "ngage", NGage },
            { "android", Android }
        };

        // Extensions that point to one platform (or one family of compatible platforms)
        private static readonly Dictionary<string, int> ExtensionPlatforms = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".nes", Nes },
            { ".unf", Nes },
            { ".unif", Nes },
            { ".fds", Fds },
            { ".sfc", Snes },
            { ".smc", Snes },
            { ".fig", Snes },
            { ".swc", Snes },
            { ".n64", N64 },
            { ".z64", N64 },
            { ".v64", N64 },
            { ".gcm", GameCube },
            { ".gcz", GameCube },
            { ".wbfs", Wii },
            { ".wad", Wii },
            { ".wua", WiiU },
            { ".wux", WiiU },
            { ".wud", WiiU },
            { ".rpx", WiiU },
            { ".nsp", Switch },
            { ".nsz", Switch },
            { ".xci", Switch },
            { ".xcz", Switch },
            { ".gb", GameBoy },
            { ".sgb", GameBoy },
            { ".gbc", GameBoyColor },
            { ".gba", GameBoyAdvance },
            { ".agb", GameBoyAdvance },
            { ".nds", Nds },
            { ".dsi", Dsi },
            { ".3ds", N3ds },
            { ".cia", N3ds },
            { ".cci", N3ds },
            { ".cxi", N3ds },
            { ".3dsx", N3ds },
            { ".vb", VirtualBoy },
            { ".vboy", VirtualBoy },
            { ".min", PokemonMini },
            { ".vpk", PsVita },
            { ".xiso", Xbox },
            { ".god", Xbox360 },
            { ".sms", MasterSystem },
            { ".md", Genesis },
            { ".gen", Genesis },
            { ".smd", Genesis },
            { ".32x", Sega32X },
            { ".gg", GameGear },
            { ".sg", Sg1000 },
            { ".gdi", Dreamcast },
            { ".cdi", Dreamcast },
            { ".ngp", NeoGeoPocket },
            { ".ngc", NeoGeoPocketColor },
            { ".ngpc", NeoGeoPocketColor },
            { ".npc", NeoGeoPocketColor },
            { ".a26", Atari2600 },
            { ".a52", Atari5200 },
            { ".a78", Atari7800 },
            { ".lnx", Lynx },
            { ".j64", Jaguar },
            { ".jag", Jaguar },
            { ".pce", PcEngine },
            { ".sgx", SuperGrafx },
            { ".col", ColecoVision },
            { ".int", Intellivision },
            { ".vec", Vectrex },
            { ".ws", WonderSwan },
            { ".wsc", WonderSwanColor },
            { ".d64", C64 },
            { ".t64", C64 },
            { ".adf", Amiga },
            { ".tzx", ZxSpectrum },
            { ".apk", Android },
            { ".exe", PcWindows },
            { ".msi", PcWindows }
        };

        // Platforms that share their games (and file formats). A release for one is fine for the other.
        private static readonly List<HashSet<int>> CompatibleGroups = new()
        {
            new HashSet<int> { Nes, Famicom },
            new HashSet<int> { Snes, SuperFamicom },
            new HashSet<int> { Nds, Dsi },
            new HashSet<int> { N3ds, New3ds },
            new HashSet<int> { NeoGeoAes, NeoGeoMvs },
            new HashSet<int> { PcEngine, SuperGrafx },
            new HashSet<int> { Switch, Switch2 },
            new HashSet<int> { PcWindows, Dos }
        };

        // Bracket/parenthesis aliases ("[PS4]", "(Switch)", "[Nintendo 3DS]"): the scene tokens plus every
        // platform's name, short name and folder name.
        private static readonly Dictionary<string, int> Aliases = BuildAliases();

        private static readonly HashSet<int> CartridgePlatforms = new()
        {
            Nes, Famicom, Fds, Snes, SuperFamicom, N64, GameBoy, GameBoyColor, GameBoyAdvance, Nds, Dsi, VirtualBoy, PokemonMini,
            Sg1000, MasterSystem, Genesis, Sega32X, GameGear, NeoGeoPocket, NeoGeoPocketColor, Atari2600, Atari5200, Atari7800,
            Lynx, Jaguar, PcEngine, SuperGrafx, ColecoVision, Intellivision, Vectrex, WonderSwan, WonderSwanColor, NeoGeoAes, NeoGeoMvs,
            Arcade, 127, 133, 306, 307, 339
        };

        public static int ParseSceneToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return 0;
            }

            return SceneTokens.TryGetValue(Normalize(token), out var platform) ? platform : 0;
        }

        public static int ParseAlias(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                return 0;
            }

            return Aliases.TryGetValue(Normalize(alias), out var platform) ? platform : 0;
        }

        public static int ParseExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return 0;
            }

            if (!extension.StartsWith('.'))
            {
                extension = "." + extension;
            }

            return ExtensionPlatforms.TryGetValue(extension, out var platform) ? platform : 0;
        }

        public static int ParsePlatform(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return 0;
            }

            return Parser.ParseGameTitle(title)?.PlatformIgdbId ?? ParseExtension(Path.GetExtension(title));
        }

        public static bool AreCompatible(int igdbPlatformId, int otherIgdbPlatformId)
        {
            if (igdbPlatformId == otherIgdbPlatformId)
            {
                return true;
            }

            return CompatibleGroups.Any(g => g.Contains(igdbPlatformId) && g.Contains(otherIgdbPlatformId));
        }

        public static bool IsCartridgePlatform(int igdbPlatformId)
        {
            return CartridgePlatforms.Contains(igdbPlatformId);
        }

        public static bool IsPcPlatform(int igdbPlatformId)
        {
            return PlatformDefinitions.All.FirstOrDefault(p => p.IgdbPlatformId == igdbPlatformId)?.Family == PlatformFamily.PC;
        }

        public static IEnumerable<string> GetSceneTokens()
        {
            return SceneTokens.Keys;
        }

        private static string Normalize(string value)
        {
            return NonAlphanumericRegex.Replace(value.ToLowerInvariant(), string.Empty);
        }

        private static Dictionary<string, int> BuildAliases()
        {
            var aliases = new Dictionary<string, int>(SceneTokens, StringComparer.OrdinalIgnoreCase);

            foreach (var platform in PlatformDefinitions.All)
            {
                foreach (var alias in new[] { platform.Name, platform.ShortName, platform.FolderName })
                {
                    var key = Normalize(alias ?? string.Empty);

                    // Single letters and numbers are too ambiguous ("2600" is fine as a token, "X1" isn't)
                    if (key.Length < 2 || key.All(char.IsDigit))
                    {
                        continue;
                    }

                    aliases.TryAdd(key, platform.IgdbPlatformId);
                }
            }

            aliases.TryAdd("playstation", Ps1);
            aliases.TryAdd("playstationportable", Psp);
            aliases.TryAdd("ps", Ps1);

            return aliases;
        }
    }
}
