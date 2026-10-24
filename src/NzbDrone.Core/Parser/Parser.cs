using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser
{
    // Game release name parser. Handles scene names ("Game.Name.v1.2-CODEX", "Game_Name_PS4-DUPLEX"),
    // No-Intro/Redump/TOSEC/GoodTools file names ("Game (USA) (En,Fr) (Rev 1) [!]"), repacks
    // ("Game - Deluxe Edition (v1.2 + DLCs) [FitGirl Repack]"), GOG installers ("setup_game_1.2_(1234).exe")
    // and Switch dumps ("Game [0100ABCD12345000][v0][US].nsp").
    //
    // The title is everything before the first "marker" (version, platform, edition, release type,
    // language, region... tokens or a bracket group), everything else is parsed for release information.
    public static class Parser
    {
        private const string EditionWords = @"Digital|Super|Ultimate|Deluxe|Definitive|Complete|Gold|Special|Collectors?|Collector'?s|Legendary|Premium|Enhanced|Royal|Standard|Platinum|Champions?|Limited|Launch|Elite|Classic|Extended|Final|Directors?|Director'?s|Anniversary|\d+(?:th|st|nd|rd)|Galaxy|Imperial|Steelbook|Exclusive|Signature|Grand|Maximum|Supreme|Uncut|Uncensored|Year|Game[._ ]of[._ ]the|Gotham|Day[._ ]One|Hero|Ultra|GOTY|Bonus|Season|Remastered|Anniversary|Cross[._ -]Gen|Next[._ -]Gen|Mega|Master|Expanded|Ultimate[._ ]Deluxe";

        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(Parser));

        private static readonly Regex[] RejectHashedReleasesRegex = new Regex[]
            {
                // Generic match for md5 and mixed-case hashes.
                new Regex(@"^[0-9a-zA-Z]{32}", RegexOptions.Compiled),

                // Generic match for shorter lower-case hashes.
                new Regex(@"^[a-z0-9]{24}$", RegexOptions.Compiled),

                // Format seen on some NZBGeek releases
                new Regex(@"^[A-Z]{11}\d{3}$", RegexOptions.Compiled),
                new Regex(@"^[a-z]{12}\d{3}$", RegexOptions.Compiled),

                // Backup filename (Unknown origins)
                new Regex(@"^Backup_\d{5,}S\d{2}-\d{2}$", RegexOptions.Compiled),

                // 123 - Started appearing December 2014
                new Regex(@"^123$", RegexOptions.Compiled),

                // abc - Started appearing January 2015
                new Regex(@"^abc$", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // abc - Started appearing 2020
                new Regex(@"^abc[-_. ]xyz", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // b00bs - Started appearing January 2015
                new Regex(@"^b00bs$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

        // Obfuscated names: a long token mixing upper and lower case letters and digits
        private static readonly Regex RandomTokenRegex = new(@"(?:^|[ ._-])(?=[A-Za-z0-9]*\d[A-Za-z0-9]*\d)(?=[A-Za-z0-9]*[a-z])(?=[A-Za-z0-9]*[A-Z])[A-Za-z0-9]{16,}(?:$|[ ._-])", RegexOptions.Compiled);

        // Regex to split game titles that contain `AKA`.
        private static readonly Regex AlternativeTitleRegex = new Regex(@"[ ]+(?:AKA|\/)[ ]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex NormalizeRegex = new Regex(@"((?:\b|_)(?<!^|[^a-zA-Z0-9_']\w[^a-zA-Z0-9_'])([aà](?!$|[^a-zA-Z0-9_']\w[^a-zA-Z0-9_'])|an|the|and|or|of)(?!$)(?:\b|_))|\W|_",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ReportIgdbId = new Regex(@"igdb(?:id)?-(?<igdbid>\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SimpleReleaseTitleRegex = new Regex(@"\s*(?:[<>?*|])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SpecialCharRegex = new Regex(@"(\&|\:|\\|\/)+", RegexOptions.Compiled);
        private static readonly Regex PunctuationRegex = new Regex(@"[^\w\s]", RegexOptions.Compiled);
        private static readonly Regex ArticleWordRegex = new Regex(@"^(a|an|the)\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SpecialEpisodeWordRegex = new Regex(@"\b(part|special|edition|christmas)\b\s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex DuplicateSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);

        private static readonly Regex MultiRegex = new(@"(?:^|[_. \[(])(?<multi>multi(?:\d+|lang(?:uage)?)?)(?=$|[_. \])-])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex BracketGroupRegex = new(@"\[(?<content>[^\[\]]*)\]|\((?<content>[^()]*)\)|\{(?<content>[^{}]*)\}", RegexOptions.Compiled);

        // "-GROUP" at the end of a scene name (the part before has to look like a scene name, so "Spider-Man" isn't a group)
        private static readonly Regex SceneGroupRegex = new(@"^(?<body>.*?[._ ].*?[^\s\-])-(?<group>[A-Za-z0-9][A-Za-z0-9_]{0,30}(?:-SCENE)?)$", RegexOptions.Compiled);

        private static readonly Regex TitleCaseWordRegex = new(@"^[A-Z][a-z]+$", RegexOptions.Compiled);

        private static readonly Regex TrailingGroupRegex = new(@"^\s*-(?<group>[A-Za-z0-9][A-Za-z0-9_]{0,30})\s*$", RegexOptions.Compiled);

        private static readonly Regex InvalidReleaseGroupRegex = new(@"^(?:\d+|v\d.*|[se]\d+|[0-9a-f]{8}|USA|EUR|JPN|JAP|PAL|NTSC|ISO|UPDATE|DLC|MULTi\d*|Disc\d*|CD\d*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // "[FitGirl Repack]", "[DODI Repack]", "[Repack by xatab]", "[GOG]", "[ElAmigos]", "[KaOs Repack]"
        private static readonly Regex BracketReleaseGroupRegex = new(@"^(?:(?<group>[\w.]+(?:\s[\w.]+)?)[\s._-]+Repack(?:ed)?|Repack(?:ed)?\s+by\s+(?<group>[\w. ]+)|(?<group>GOG|FitGirl|DODI|ElAmigos|KaOs|xatab|Masquerade|R\.\s?G\.\s?Mechanics|CorePack|Chovka|Decepticon|qoob|Darck|ZAZIX|BlackBox|Wanterlude|Gnarly|ARMGDDN|TENOKE|CODEX|RUNE|SKIDROW|PLAZA|EMPRESS|DOGE|FLT|TiNYiSO))$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex VersionTokenRegex = new(@"^v\d+(?:[._]\d+)*[a-z]?\d*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Versions in the full release name
        private static readonly Regex VersionRegex = new(@"(?:^|[-._ \[(+])(?:v(?<version>\d+(?:[._]\d+)*[a-z]?\d*)|(?:Build|b)[._ ]?(?<build>\d{3,})|Update[._ ](?<version>\d+(?:\.\d+)+)|(?<version>\d+\.\d+\.\d+(?:\.\d+)*))(?=$|[-._ \])+,])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ThreePartVersionRegex = new(@"(?<=[._ ])(?:\d+\.\d+\.\d+(?:\.\d+)*|Build[._ ]?\d{3,})(?=$|[._ \-])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DiscRegex = new(@"(?:^|[-._ \[(])(?:Disc|Disk|CD)[._ ]?(?<disc>\d{1,2})(?:[._ ]?of[._ ]?(?<count>\d{1,2}))?(?=$|[-._ \])])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DiscCountRegex = new(@"(?:^|[-._ \[(])(?<count>\d{1,2})[._ ]?(?:Discs|Disks|CDs)(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex YearRegex = new(@"^(?<year>(?:19[7-9]\d|20[0-3]\d))(?:-\d{2}(?:-\d{2})?)?$", RegexOptions.Compiled);

        private static readonly Regex TitleIdRegex = new(@"\[(?<id>0[0-9a-f]{15})\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SwitchVersionRegex = new(@"\[v(?<version>\d+)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Things a base game release can include ("+ 5 DLCs", "incl. Update 3", "All DLC"). They don't make the
        // release an update or a DLC.
        private static readonly Regex IncludedContentRegex = new(@"(?:\+|\b(?:incl(?:uding|\.)?|with|w\/)[._ ]?)[._ ]*(?:\d+[._ ]|all[._ ])?(?:(?<dlc>DLCs?|Expansions?|Season[._ ]Pass|Bonus(?:[._ ]Content)?|Soundtrack|OST)|(?<update>Updates?|Patch(?:es)?|Hotfix(?:es)?)(?:[._ ]?v?\d+(?:\.\d+)*)?)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex AllDlcRegex = new(@"\b(?:All[._ ]DLCs?|Complete[._ ]Bundle|Bundle)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex UpdateRegex = new(@"(?:^|[-._ \[(])(?:Update|Patch|Hotfix|Crackfix|Upd)(?=$|[-._ \])\d])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex DlcRegex = new(@"(?:^|[-._ \[(])(?:DLCs?|Unlocker|Season[._ ]Pass|Add[._ -]?On)(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex DemoRegex = new(@"(?:^|[-._ \[(])(?:Demo|Trial|Kiosk|Sample)(?=$|[-._ \])\d])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex PrototypeRegex = new(@"(?:^|[-._ \[(])(?:Proto(?:type)?|Beta|Alpha|Pre[-._ ]?Release|Preview|Debug)(?=$|[-._ \])\d])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex HackRegex = new(@"\[h\d*[^\]]*\]|\((?:Hack|.*\bHack)\)|(?:^|[-._ \[])Hack(?:ed)?(?=$|[-._ \]])", RegexOptions.Compiled);
        private static readonly Regex TranslationRegex = new(@"\[T[+-][A-Za-z]{2,3}[^\]]*\]|(?:^|[-._ \[(])(?:Translat(?:ed|ion)|Fan[-._ ]?Translation)(?=$|[-._ \])])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex HomebrewRegex = new(@"\((?:Homebrew|PD|Public[ ]Domain|Aftermarket)\)|(?:^|[-._ \[])Homebrew(?=$|[-._ \]])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex EditionRegex = new(@"(?<=^|[._ \-(\[,:])(?<edition>(?:The[._ ])?(?:(?:" + EditionWords + @")[._ ]){1,3}(?:Edition|Cut|Version)|GOTY|Game[._ ]of[._ ]the[._ ]Year)(?=$|[._ \-)\],])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Tokens that end the title of a scene or indexer name
        private static readonly HashSet<string> MarkerTokens = new(StringComparer.OrdinalIgnoreCase)
        {
            "update", "patch", "hotfix", "crackfix", "upd", "unlocker", "dlc", "dlcs", "demo", "trial", "beta", "alpha", "prototype", "proto",
            "incl", "including", "bundle", "trainer", "crack", "cracked", "crackonly", "repack", "repacked", "proper", "rerip", "readnfo",
            "nfofix", "dirfix", "internal", "multi", "multilang", "multilanguage", "x64", "x86", "64bit", "32bit", "iso", "clonedvd", "rip",
            "gog", "drmfree", "steamrip", "portable", "preinstalled", "redump", "nointro", "nsp", "xci", "nsz", "pkg", "chd", "rvz",
            "cso", "wbfs", "hack", "translated", "translation", "homebrew", "eshop", "psn", "xbla", "english", "german", "french", "spanish",
            "italian", "polish", "russian", "japanese", "korean", "chinese", "eng", "ger", "fra", "fre", "ita", "spa", "rus", "pol", "chs",
            "cht", "goty", "disc", "cd1", "cd2", "cd3", "cd4", "disc1", "disc2", "disc3", "disc4", "dvd5", "dvd9"
        };

        // Region tokens that end a scene title, case sensitive ("USA" yes, "Usa" no)
        private static readonly HashSet<string> RegionMarkerTokens = new(StringComparer.Ordinal)
        {
            "USA", "EUR", "JPN", "JAP", "PAL", "NTSC", "NTSC-U", "NTSC-J", "ASIA", "KOR"
        };

        // Platform tokens that are also ordinary words. They only end the title if they're the last token or are
        // followed by another marker ("Nintendo.Switch.Sports.NSW-GROUP" keeps "Switch" in the title).
        private static readonly HashSet<string> WeakPlatformTokens = new(StringComparer.OrdinalIgnoreCase)
        {
            "switch", "arcade", "mac", "windows", "linux", "android", "saturn", "genesis", "jaguar", "lynx", "amiga", "dreamcast",
            "gamecube", "xbox", "famicom", "megadrive", "mastersystem", "gameboy", "vita", "dos", "pc", "win", "md", "gb", "gc", "dc",
            "mame", "neogeo", "nes", "snes", "wii", "3do", "cdi", "ngage", "superfamicom", "turbografx", "pcengine"
        };

        private static readonly Regex TokenSplitRegex = new(@"[._ ]+", RegexOptions.Compiled);

        private static readonly Regex TokenTrimRegex = new(@"^[^\w+]+|[^\w+]+$", RegexOptions.Compiled);

        // "Legend of Zelda, The - A Link to the Past" -> "The Legend of Zelda - A Link to the Past"
        private static readonly Regex TrailingArticleRegex = new(@"^(?<name>[^,]+?), (?<article>The|A|An|Le|La|Les|L'|Der|Die|Das|El|Il|Lo|Gli|I)(?<rest>(?:\s+-\s+.*)?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // GOG offline installers: setup_the_witcher_3_wild_hunt_goty_1.32_(a)_(10706).exe, gog_game_name_2.0.0.2.sh
        private static readonly Regex GogInstallerRegex = new(@"^(?:setup|gog)_(?<name>[a-z0-9_.()'+-]+?)(?:-\d+)?\.(?<ext>exe|bin|sh|pkg|dmg)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex GogVersionTokenRegex = new(@"^v?\d+(?:\.\d+)+[a-z0-9.]*$|^\(.*\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Generic installer/disc file names that say nothing about the game, the folder name is used instead
        private static readonly Regex GenericFileNameRegex = new(@"^(?:setup|install(?:er)?|autorun|launcher|game|start|play|default|main|data\d*|disc\s?\d+|disk\s?\d+|cd\s?\d+|track\s?\d+|\d+|sample)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex AnyExtensionRegex = new(@"\.[a-z0-9]{2,4}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SceneFileNameRegex = new(@"^[a-z0-9_]+-[a-z0-9._-]+$", RegexOptions.Compiled);

        private static readonly Regex FileExtensionRegex = new(@"\.[a-z0-9]{1,5}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex WebsitePrefixRegex = new(@"^(?:\[\s*)?(?:www\.)?[-a-z0-9]{1,256}\.(?:com|net|org|info|ru|to|cc|me|xyz|io)\b(?:\s*\])?[ -]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly HashSet<string> UsenetExtensions = new(StringComparer.OrdinalIgnoreCase) { ".par2", ".nzb", ".torrent" };

        public static ParsedGameInfo ParseGamePath(string path)
        {
            var fileInfo = new FileInfo(path);
            var fileName = fileInfo.Name;
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            ParsedGameInfo result = null;

            if (!GenericFileNameRegex.IsMatch(nameWithoutExtension))
            {
                result = ParseGameTitle(fileName, true);
            }

            if (fileInfo.Directory != null)
            {
                // Scene file names are abbreviated and start with the group ("codex-eldenring.iso"), the folder has
                // the real release name
                var folderResult = result == null || SceneFileNameRegex.IsMatch(nameWithoutExtension)
                    ? ParseGameTitle(fileInfo.Directory.Name + fileInfo.Extension, true)
                    : null;

                if (result == null ||
                    (folderResult?.ReleaseGroup != null && nameWithoutExtension.StartsWith(folderResult.ReleaseGroup + "-", StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.Debug("Using game info from the directory name. {0}", fileInfo.Directory.Name);
                    result = folderResult ?? result;
                }
            }

            return result;
        }

        public static ParsedGameInfo ParseGameTitle(string title, bool isDir = false)
        {
            var originalTitle = title;

            try
            {
                if (title.IsNullOrWhiteSpace() || !ValidateBeforeParsing(title))
                {
                    return null;
                }

                Logger.Debug("Parsing string '{0}'", title);

                title = title.Trim();

                var extension = GetGameFileExtension(title);
                var releaseTitle = extension == null ? title : title.Substring(0, title.Length - extension.Length);

                releaseTitle = releaseTitle.Replace("【", "[").Replace("】", "]").Replace('–', '-').Replace('—', '-');
                releaseTitle = WebsitePrefixRegex.Replace(releaseTitle, string.Empty);
                releaseTitle = ParserCommon.WebsitePrefixRegex.Replace(releaseTitle);
                releaseTitle = ParserCommon.WebsitePostfixRegex.Replace(releaseTitle);
                releaseTitle = ParserCommon.CleanTorrentSuffixRegex.Replace(releaseTitle);
                releaseTitle = releaseTitle.Trim(' ', '-', '_', '.');

                if (releaseTitle.IsNullOrWhiteSpace())
                {
                    return null;
                }

                var forcedPlatform = 0;
                var parseTitle = releaseTitle;

                var gogMatch = GogInstallerRegex.Match(title);

                if (gogMatch.Success)
                {
                    parseTitle = BuildGogTitle(gogMatch.Groups["name"].Value);
                    forcedPlatform = gogMatch.Groups["ext"].Value.ToLowerInvariant() switch
                    {
                        "sh" => PlatformParser.Linux,
                        "pkg" or "dmg" => PlatformParser.Mac,
                        _ => PlatformParser.PcWindows
                    };
                }

                var result = ParseRelease(parseTitle, extension, forcedPlatform);

                if (result == null)
                {
                    Logger.Debug("Unable to parse {0}", title);
                    return null;
                }

                result.OriginalTitle = originalTitle;
                result.ReleaseTitle = releaseTitle;

                Logger.Debug("Game Parsed. {0}", result);

                return result;
            }
            catch (Exception e)
            {
                if (!title.ToLower().Contains("password") && !title.ToLower().Contains("yenc"))
                {
                    Logger.Error(e, "An error has occurred while trying to parse {0}", title);
                }
            }

            Logger.Debug("Unable to parse {0}", title);
            return null;
        }

        private static ParsedGameInfo ParseRelease(string releaseTitle, string extension, int forcedPlatform)
        {
            var groups = BracketGroupRegex.Matches(releaseTitle).ToList();

            // Leading bracket groups ("[FitGirl] Game Name") are tags, the title starts after them
            var titleStart = 0;

            foreach (var group in groups)
            {
                if (group.Index > titleStart && releaseTitle.Substring(titleStart, group.Index - titleStart).Trim(' ', '-', '_', '.').Length > 0)
                {
                    break;
                }

                titleStart = group.Index + group.Length;
            }

            var firstGroup = groups.FirstOrDefault(g => g.Index >= titleStart);
            var prefixEnd = firstGroup?.Index ?? releaseTitle.Length;
            var prefix = releaseTitle.Substring(titleStart, prefixEnd - titleStart);
            var trailing = firstGroup == null ? string.Empty : BracketGroupRegex.Replace(releaseTitle.Substring(prefixEnd), " ");

            // Scene release group
            string releaseGroup = null;
            var body = prefix.Trim();

            if (trailing.IsNotNullOrWhiteSpace())
            {
                var trailingGroup = TrailingGroupRegex.Match(trailing);

                if (trailingGroup.Success && IsValidReleaseGroup(trailingGroup.Groups["group"].Value))
                {
                    releaseGroup = trailingGroup.Groups["group"].Value;
                }
            }
            else
            {
                var sceneGroup = SceneGroupRegex.Match(body);

                if (sceneGroup.Success && IsValidReleaseGroup(sceneGroup.Groups["group"].Value))
                {
                    releaseGroup = sceneGroup.Groups["group"].Value;
                    body = sceneGroup.Groups["body"].Value;
                }
            }

            var bodyOffset = titleStart + (prefix.Length - prefix.TrimStart().Length);
            var dotted = !body.Contains(' ') && (body.Contains('.') || body.Contains('_'));

            // No-Intro/Redump/TOSEC names have the canonical title before the tags, editions are part of it
            var hasDumpTags = groups.Any(g => RegionParser.ParseBracketGroup(g.Groups["content"].Value).Any());
            var markerIndex = FindFirstMarker(body, !hasDumpTags);
            var titlePart = markerIndex > 0 ? body.Substring(0, markerIndex) : body;
            var remainder = markerIndex > 0 ? body.Substring(markerIndex) : string.Empty;

            var gameTitle = CleanTitle(titlePart, dotted);

            if (gameTitle.IsNullOrWhiteSpace())
            {
                return null;
            }

            var result = new ParsedGameInfo();
            var bracketContents = groups.Select(g => g.Groups["content"].Value.Trim()).Where(c => c.IsNotNullOrWhiteSpace()).ToList();

            // Text that may hold release information, without the title so its words aren't taken for tags
            var infoText = releaseTitle.Remove(bodyOffset, titlePart.Length).Insert(bodyOffset, " ");

            result.GameTitles = BuildTitles(gameTitle, out var titleYear);
            result.Year = ParseYear(bracketContents, titleYear);

            // Release group from bracket tags if there's no scene group
            if (releaseGroup == null)
            {
                foreach (var content in bracketContents)
                {
                    var bracketGroup = BracketReleaseGroupRegex.Match(content);

                    if (bracketGroup.Success)
                    {
                        releaseGroup = bracketGroup.Groups["group"].Value.Trim();
                        break;
                    }
                }
            }

            result.ReleaseGroup = releaseGroup;
            result.Edition = ParseEditionInternal(infoText);
            result.PlatformIgdbId = forcedPlatform > 0 ? forcedPlatform : ParsePlatform(remainder, trailing, bracketContents, extension, releaseGroup);
            result.Regions = ParseRegions(remainder + " " + trailing, bracketContents);
            result.Languages = ParseLanguages(infoText);
            result.Version = ParseVersion(infoText);
            result.TitleId = ParseTitleId(releaseTitle);
            result.ReleaseType = ParseReleaseType(infoText, result.TitleId);

            ParseDisc(infoText, result);

            var qualityName = releaseTitle + (extension ?? string.Empty);

            if (forcedPlatform > 0)
            {
                // GOG installers
                result.Quality = QualityParser.ParseQuality(qualityName, forcedPlatform);
                result.Quality.Quality = Qualities.Quality.PcGog;
                result.Quality.SourceDetectionSource = Qualities.QualityDetectionSource.Name;
                result.ReleaseGroup ??= "GOG";
            }
            else
            {
                result.Quality = QualityParser.ParseQuality(qualityName, result.PlatformIgdbId);
            }

            result.IgdbId = ParseIgdbId(releaseTitle);

            var simpleReleaseTitle = SimpleReleaseTitleRegex.Replace(releaseTitle, string.Empty);

            if (titlePart.IsNotNullOrWhiteSpace() && simpleReleaseTitle.Length >= bodyOffset + titlePart.Length && simpleReleaseTitle.Substring(bodyOffset, titlePart.Length) == titlePart)
            {
                simpleReleaseTitle = simpleReleaseTitle.Remove(bodyOffset, titlePart.Length).Insert(bodyOffset, dotted ? "A.Game." : "A Game ");
            }

            result.SimpleReleaseTitle = simpleReleaseTitle;

            return result;
        }

        private static int FindFirstMarker(string body, bool editionIsMarker)
        {
            var tokens = Tokenize(body);
            var markerIndex = -1;

            for (var i = 1; i < tokens.Count; i++)
            {
                if (IsMarkerAt(tokens, i))
                {
                    markerIndex = tokens[i].Index;
                    break;
                }
            }

            // Versions without a "v" ("1.2.3") and builds are split into several tokens in dotted names
            var versionMatch = ThreePartVersionRegex.Match(body);

            if (versionMatch.Success && versionMatch.Index > 0 && (markerIndex < 0 || versionMatch.Index < markerIndex))
            {
                markerIndex = versionMatch.Index;
            }

            var editionMatch = EditionRegex.Match(body);

            if (editionIsMarker && editionMatch.Success && editionMatch.Index > 0 && (markerIndex < 0 || editionMatch.Index < markerIndex))
            {
                markerIndex = editionMatch.Index;
            }

            return markerIndex;
        }

        private static bool IsMarkerAt(List<Token> tokens, int i)
        {
            var token = tokens[i].Text;

            if (token.StartsWith('+'))
            {
                return true;
            }

            if (VersionTokenRegex.IsMatch(token))
            {
                return true;
            }

            if (MarkerTokens.Contains(token) || RegionMarkerTokens.Contains(token) || IsMultiToken(token))
            {
                return true;
            }

            if (token.Equals("Build", StringComparison.OrdinalIgnoreCase) && i + 1 < tokens.Count && tokens[i + 1].Text.All(char.IsDigit))
            {
                return true;
            }

            if (IsPlatformAt(tokens, i, out var weak))
            {
                return !weak || i == tokens.Count - 1 || IsStrongMarker(tokens, i + 1);
            }

            return false;
        }

        private static bool IsStrongMarker(List<Token> tokens, int i)
        {
            var token = tokens[i].Text;

            if (VersionTokenRegex.IsMatch(token) || MarkerTokens.Contains(token) || RegionMarkerTokens.Contains(token) || IsMultiToken(token))
            {
                return true;
            }

            return IsPlatformAt(tokens, i, out var weak) && !weak;
        }

        private static bool IsMultiToken(string token)
        {
            return token.StartsWith("multi", StringComparison.OrdinalIgnoreCase) && (token.Length == 5 || token.Substring(5).All(char.IsDigit));
        }

        private static bool IsPlatformAt(List<Token> tokens, int i, out bool weak)
        {
            weak = false;
            var token = tokens[i].Text;

            // Two token platforms: "Wii.U", "Xbox.360", "Xbox.One", "PS.Vita", "Game.Boy"
            if (i + 1 < tokens.Count && PlatformParser.ParseSceneToken(token + tokens[i + 1].Text) > 0 && token.Length > 1)
            {
                weak = token.Equals("game", StringComparison.OrdinalIgnoreCase) || token.Equals("nintendo", StringComparison.OrdinalIgnoreCase);
                return true;
            }

            if (PlatformParser.ParseSceneToken(token) > 0)
            {
                weak = WeakPlatformTokens.Contains(token);
                return true;
            }

            return false;
        }

        private static List<Token> Tokenize(string body)
        {
            var tokens = new List<Token>();
            var index = 0;

            foreach (var part in TokenSplitRegex.Split(body))
            {
                var start = body.IndexOf(part, index, StringComparison.Ordinal);

                if (start < 0)
                {
                    continue;
                }

                index = start + part.Length;

                var text = TokenTrimRegex.Replace(part, string.Empty);

                if (text.IsNullOrWhiteSpace())
                {
                    continue;
                }

                // Keep the position of the token itself, not of leading punctuation
                tokens.Add(new Token(text, start + part.IndexOf(text, StringComparison.Ordinal)));
            }

            return tokens;
        }

        private static string CleanTitle(string titlePart, bool dotted)
        {
            var title = titlePart.Trim();

            if (dotted)
            {
                title = title.Replace('_', ' ');
                title = JoinDottedTitle(title);
            }
            else
            {
                title = title.Replace('_', ' ');
            }

            title = DuplicateSpacesRegex.Replace(title, " ").Trim(' ', '-', ':', ',', '~', '+', '.', ';', '/', '&', '|');

            var articleMatch = TrailingArticleRegex.Match(title);

            if (articleMatch.Success)
            {
                title = $"{articleMatch.Groups["article"].Value} {articleMatch.Groups["name"].Value}{articleMatch.Groups["rest"].Value}";
            }

            return title.Trim();
        }

        // Scene names use dots for spaces, single letters joined with dots are acronyms ("S.T.A.L.K.E.R")
        private static string JoinDottedTitle(string title)
        {
            var parts = title.Split('.');
            var gameName = string.Empty;
            var previousAcronym = false;

            for (var n = 0; n < parts.Length; n++)
            {
                var part = parts[n];
                var nextPart = n + 1 < parts.Length ? parts[n + 1] : string.Empty;

                if (part.Length == 1 && part.ToLower() != "a" && !int.TryParse(part, out _) &&
                    (previousAcronym || n < parts.Length - 1) &&
                    (previousAcronym || nextPart.Length != 1 || !int.TryParse(nextPart, out _)))
                {
                    gameName += part + ".";
                    previousAcronym = true;
                }
                else if (part.ToLower() == "a" && (previousAcronym || nextPart.Length == 1))
                {
                    gameName += part + ".";
                    previousAcronym = true;
                }
                else
                {
                    if (previousAcronym)
                    {
                        gameName += " ";
                        previousAcronym = false;
                    }

                    gameName += part + " ";
                }
            }

            return gameName.Trim();
        }

        private static List<string> BuildTitles(string gameTitle, out int titleYear)
        {
            titleYear = 0;

            var titles = new List<string> { gameTitle };

            titles.AddRange(AlternativeTitleRegex.Split(gameTitle)
                .Select(t => t.Trim())
                .Where(t => t.IsNotNullOrWhiteSpace() && t != gameTitle));

            // Scene names sometimes add the year to tell remakes apart ("Hitman.2016"), keep the title as is
            // and add one without the year.
            var words = gameTitle.Split(' ');

            if (words.Length > 1 && YearRegex.IsMatch(words.Last()))
            {
                titleYear = int.Parse(words.Last());
                titles.Add(string.Join(" ", words.Take(words.Length - 1)));
            }

            return titles.Distinct().ToList();
        }

        private static int ParseYear(List<string> bracketContents, int titleYear)
        {
            foreach (var content in bracketContents)
            {
                var match = YearRegex.Match(content);

                if (match.Success)
                {
                    return int.Parse(match.Groups["year"].Value);
                }
            }

            return titleYear;
        }

        private static int ParsePlatform(string remainder, string trailing, List<string> bracketContents, string extension, string releaseGroup)
        {
            var tokens = Tokenize(remainder + " " + trailing);

            for (var i = 0; i < tokens.Count; i++)
            {
                if (i + 1 < tokens.Count)
                {
                    var joined = PlatformParser.ParseSceneToken(tokens[i].Text + tokens[i + 1].Text);

                    if (joined > 0 && tokens[i].Text.Length > 1)
                    {
                        return joined;
                    }
                }

                var platform = PlatformParser.ParseSceneToken(tokens[i].Text);

                if (platform > 0)
                {
                    return platform;
                }
            }

            foreach (var content in bracketContents)
            {
                var platform = PlatformParser.ParseAlias(content);

                if (platform > 0)
                {
                    return platform;
                }
            }

            var extensionPlatform = PlatformParser.ParseExtension(extension);

            if (extensionPlatform > 0)
            {
                return extensionPlatform;
            }

            // PC scene and repack groups only release PC games
            if (QualityParser.IsPcSceneGroup(releaseGroup) || QualityParser.IsRepackGroup(releaseGroup) || "GOG".Equals(releaseGroup, StringComparison.OrdinalIgnoreCase))
            {
                return PlatformParser.PcWindows;
            }

            return 0;
        }

        private static List<Region> ParseRegions(string sceneText, List<string> bracketContents)
        {
            var regions = new List<Region>();

            foreach (var content in bracketContents)
            {
                regions.AddRange(RegionParser.ParseBracketGroup(content));
            }

            regions.AddRange(RegionParser.ParseSceneTokens(sceneText));

            return regions.Distinct().ToList();
        }

        private static List<Language> ParseLanguages(string infoText)
        {
            var languages = LanguageParser.ParseLanguages(infoText);

            if (languages.Count > 1)
            {
                languages.Remove(Language.Unknown);
            }

            return languages;
        }

        private static string ParseVersion(string infoText)
        {
            var switchVersion = SwitchVersionRegex.Match(infoText);

            if (switchVersion.Success)
            {
                return switchVersion.Groups["version"].Value == "0" ? null : switchVersion.Groups["version"].Value;
            }

            var match = VersionRegex.Match(infoText);

            if (!match.Success)
            {
                return null;
            }

            if (match.Groups["build"].Success)
            {
                return "Build " + match.Groups["build"].Value;
            }

            return match.Groups["version"].Value.Replace('_', '.');
        }

        private static string ParseTitleId(string releaseTitle)
        {
            var match = TitleIdRegex.Match(releaseTitle);

            return match.Success ? match.Groups["id"].Value.ToUpperInvariant() : null;
        }

        private static ReleaseType ParseReleaseType(string infoText, string titleId)
        {
            if (HackRegex.IsMatch(infoText))
            {
                return ReleaseType.Hack;
            }

            if (TranslationRegex.IsMatch(infoText))
            {
                return ReleaseType.Translation;
            }

            if (HomebrewRegex.IsMatch(infoText))
            {
                return ReleaseType.Homebrew;
            }

            if (DemoRegex.IsMatch(infoText))
            {
                return ReleaseType.Demo;
            }

            if (PrototypeRegex.IsMatch(infoText))
            {
                return ReleaseType.Prototype;
            }

            // Content included with a base game ("+ 5 DLCs", "incl. Update 3") doesn't count
            var includesDlc = false;
            var text = IncludedContentRegex.Replace(infoText, m =>
            {
                includesDlc |= m.Groups["dlc"].Success;
                return " ";
            });

            if (UpdateRegex.IsMatch(text))
            {
                return ReleaseType.Update;
            }

            if (DlcRegex.IsMatch(text))
            {
                return ReleaseType.Dlc;
            }

            if (titleId != null)
            {
                // Switch title ids: base games end in 000, updates in 800, DLC have their own ids
                if (titleId.EndsWith("800"))
                {
                    return ReleaseType.Update;
                }

                if (!titleId.EndsWith("000"))
                {
                    return ReleaseType.Dlc;
                }
            }

            if (includesDlc || AllDlcRegex.IsMatch(text))
            {
                return ReleaseType.Bundle;
            }

            return ReleaseType.Base;
        }

        private static void ParseDisc(string infoText, ParsedGameInfo result)
        {
            var match = DiscRegex.Match(infoText);

            if (match.Success)
            {
                result.DiscNumber = int.Parse(match.Groups["disc"].Value);

                if (match.Groups["count"].Success)
                {
                    result.DiscCount = int.Parse(match.Groups["count"].Value);
                }
            }

            var countMatch = DiscCountRegex.Match(infoText);

            if (countMatch.Success && result.DiscCount == 0)
            {
                result.DiscCount = int.Parse(countMatch.Groups["count"].Value);
            }
        }

        private static bool IsValidReleaseGroup(string group)
        {
            if (group.IsNullOrWhiteSpace() || InvalidReleaseGroupRegex.IsMatch(group))
            {
                return false;
            }

            if (PlatformParser.ParseSceneToken(group) > 0)
            {
                return false;
            }

            // "Spider-Man" isn't a release from the group "Man"
            if (TitleCaseWordRegex.IsMatch(group) && !QualityParser.IsPcSceneGroup(group) && !QualityParser.IsRepackGroup(group))
            {
                return false;
            }

            return true;
        }

        private static string BuildGogTitle(string name)
        {
            var tokens = name.Split('_', StringSplitOptions.RemoveEmptyEntries);
            var titleTokens = new List<string>();
            string version = null;

            foreach (var token in tokens)
            {
                if (GogVersionTokenRegex.IsMatch(token))
                {
                    if (version == null && !token.StartsWith('('))
                    {
                        version = token.TrimStart('v', 'V');
                    }

                    break;
                }

                titleTokens.Add(token);
            }

            var title = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(string.Join(" ", titleTokens));

            return version == null ? $"{title} [GOG]" : $"{title} v{version} [GOG]";
        }

        private static string GetGameFileExtension(string title)
        {
            var match = FileExtensionRegex.Match(title);

            if (!match.Success)
            {
                return null;
            }

            var extension = match.Value.ToLowerInvariant();

            if (MediaFileExtensions.IsGameFile(extension) || PlatformParser.ParseExtension(extension) > 0 || UsenetExtensions.Contains(extension))
            {
                return match.Value;
            }

            return null;
        }

        public static int ParseIgdbId(string title)
        {
            var match = ReportIgdbId.Match(title);

            if (match.Success && match.Groups["igdbid"].Value != null)
            {
                return int.TryParse(match.Groups["igdbid"].Value, out var igdbId) ? igdbId : 0;
            }

            return 0;
        }

        public static string ParseEdition(string title)
        {
            return title.IsNullOrWhiteSpace() ? string.Empty : ParseEditionInternal(title) ?? string.Empty;
        }

        private static string ParseEditionInternal(string text)
        {
            var match = EditionRegex.Match(text);

            if (!match.Success)
            {
                return null;
            }

            var edition = match.Groups["edition"].Value.Replace('.', ' ').Replace('_', ' ');

            return edition.Equals("Game of the Year", StringComparison.OrdinalIgnoreCase) ? "Game of the Year Edition" : edition;
        }

        public static string ReplaceGermanUmlauts(string s)
        {
            var t = s;
            t = t.Replace("ä", "ae");
            t = t.Replace("ö", "oe");
            t = t.Replace("ü", "ue");
            t = t.Replace("Ä", "Ae");
            t = t.Replace("Ö", "Oe");
            t = t.Replace("Ü", "Ue");
            t = t.Replace("ß", "ss");
            return t;
        }

        public static string ToUrlSlug(string value, bool invalidDashReplacement = false, string trimEndChars = "-_", string deduplicateChars = "-_")
        {
            // First to lower case
            value = value.ToLowerInvariant();

            // Remove all accents
            value = value.RemoveAccent();

            // Replace spaces
            value = Regex.Replace(value, @"\s", "-", RegexOptions.Compiled);

            // Should invalid characters be replaced with dash or empty string?
            var replaceCharacter = invalidDashReplacement ? "-" : string.Empty;

            // Remove invalid chars
            value = Regex.Replace(value, @"[^a-z0-9\s-_]", replaceCharacter, RegexOptions.Compiled);

            // Trim dashes or underscores from end, or user defined character set
            if (!string.IsNullOrEmpty(trimEndChars))
            {
                value = value.Trim(trimEndChars.ToCharArray());
            }

            // Replace double occurrences of - or _, or user defined character set
            if (!string.IsNullOrEmpty(deduplicateChars))
            {
                value = Regex.Replace(value, @"([" + deduplicateChars + "]){2,}", "$1", RegexOptions.Compiled);
            }

            return value;
        }

        public static string CleanGameTitle(this string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return title;
            }

            // If Title only contains numbers return it as is.
            if (long.TryParse(title, out _))
            {
                return title;
            }

            return ReplaceGermanUmlauts(NormalizeRegex.Replace(title, string.Empty).ToLowerInvariant()).RemoveAccent();
        }

        public static string NormalizeGameTitle(string title)
        {
            title = SpecialEpisodeWordRegex.Replace(title, string.Empty);
            title = PunctuationRegex.Replace(title, " ");
            title = DuplicateSpacesRegex.Replace(title, " ");

            return title.Trim().ToLower();
        }

        public static string NormalizeTitle(string title)
        {
            title = PunctuationRegex.Replace(title, string.Empty);
            title = ArticleWordRegex.Replace(title, string.Empty);
            title = DuplicateSpacesRegex.Replace(title, " ");
            title = SpecialCharRegex.Replace(title, string.Empty);

            return title.Trim().ToLower();
        }

        public static string SimplifyReleaseTitle(this string title)
        {
            return SimpleReleaseTitleRegex.Replace(title, string.Empty);
        }

        public static bool HasMultipleLanguages(string title)
        {
            return MultiRegex.IsMatch(title);
        }

        public static string ParseReleaseGroup(string title)
        {
            return ParseGameTitle(title)?.ReleaseGroup;
        }

        private static bool ValidateBeforeParsing(string title)
        {
            if (title.ToLower().Contains("password") && title.ToLower().Contains("yenc"))
            {
                Logger.Debug("");
                return false;
            }

            if (!title.Any(char.IsLetterOrDigit))
            {
                return false;
            }

            var titleWithoutExtension = AnyExtensionRegex.Replace(title, string.Empty);

            if (RejectHashedReleasesRegex.Any(v => v.IsMatch(titleWithoutExtension)) || RandomTokenRegex.IsMatch(titleWithoutExtension))
            {
                Logger.Debug("Rejected Hashed Release Title: " + title);
                return false;
            }

            return true;
        }

        private sealed class Token
        {
            public Token(string text, int index)
            {
                Text = text;
                Index = index;
            }

            public string Text { get; }
            public int Index { get; }
        }
    }
}
