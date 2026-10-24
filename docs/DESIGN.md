# Gamarr design

Gamarr is a fork of Radarr, mechanically renamed (Radarr→Gamarr, Movie→Game, Tmdb→Igdb), and then
adapted to games. It keeps Radarr's architecture: a .NET backend (`NzbDrone.*` projects + `Gamarr.Api.V3`),
SQLite/Postgres via FluentMigrator + Dapper, SignalR, a React/TypeScript frontend, and a provider framework
for indexers, download clients, import lists, notifications and metadata.

## Domain model

- **Game** works like Radarr's Movie: one monitored library item with a root folder, path, quality
  profile, tags and at most one GameFile.
- **Platform** is new. It's a seeded reference table:
  `Id, Name, ShortName, IgdbPlatformId, Family (Nintendo|Sony|Microsoft|Sega|PC|Arcade|Atari|NEC|SNK|Other),
  Generation, FolderName (ES-DE/RetroArch style: "snes", "psx", "pc"), FileExtensions (JSON array),
  NewznabCategories (JSON array), Enabled`. Only `Enabled` is user editable (`/api/v3/platform`).
  Every Game has a required `PlatformId`. A title released on several platforms is one Game record per
  platform. The unique key is **(IgdbId, PlatformId)**, not IgdbId on its own, so every lookup or uniqueness
  check that used to be by TmdbId has to take the platform into account. In the database this is a unique
  index on Games (GameMetadataId, PlatformId).
- **GameMetadata** (IGDB-backed, shared by Games with the same IgdbId):
  IgdbId, Title, CleanTitle, SortTitle, OriginalTitle, AlternativeTitles (IGDB alternative_names),
  Year, ReleaseDate (first_release_date), PlatformReleaseDates (json: igdbPlatformId→date),
  Overview (summary), Storyline, Genres, Themes, GameModes (single player, co-op…),
  Developers, Publishers (from involved_companies), Franchise / CollectionIgdbId (IGDB collection),
  Ratings (IGDB rating + aggregated critic rating, with vote counts), AgeRatings (ESRB/PEGI),
  Images (cover→Poster, artworks→Fanart, screenshots→Screenshot), Websites (Steam, GOG…),
  YouTubeTrailerId (IGDB videos), Status (released/upcoming/cancelled → maps onto the existing GameStatusType),
  AvailablePlatformIgdbIds, LastInfoSync.
  Removed from Radarr: ImdbId, runtime, certification, studio, cinema/physical/digital dates,
  "minimum availability" based on cinema dates. Availability is just "Released" or "Announced": the
  `MinimumAvailability` field is kept (Announced/Released, InCinemas was removed) and is evaluated against the
  game's release date on its own platform (PlatformReleaseDates), falling back to the first release date.
- **Credits** are repurposed as company credits (developer/publisher/porting/supporting).
  **Collections** become IGDB collections ("series").

## Metadata source: IGDB

- IGDB API v4 (`https://api.igdb.com/v4/`, Apicalypse POST bodies). Auth uses a Twitch client-credentials
  token from `https://id.twitch.tv/oauth2/token`, cached until it expires.
- The user enters a Twitch Client ID and Client Secret under Settings > Metadata. There's a health check
  when these are missing or invalid.
- The provider implements the existing interfaces (`IProvideGameInfo`, `ISearchForNewGame`, etc.) in
  `MetadataSource/Igdb/`. SkyHook is deleted.
- Search terms: `igdb:1234` → lookup by id, otherwise a name search. A platform filter is optional.
- Requests are rate-limited to 4/s (IGDB's limit).

## Stage 2 implementation notes

- 93 platforms are seeded (migration 243, `PlatformDefinitions`), PC (Microsoft Windows) gets Id 1 and existing
  games were assigned to it. IGDB ids that couldn't be verified are marked in `PlatformDefinitions`. New entries in
  `PlatformDefinitions` are added on startup.
- Adding a game through `POST /api/v3/game` requires `platformId`. Internal adds (import lists, collections, the
  library import) may leave it at 0, then the first enabled platform IGDB lists for the game is used.
- Import list exclusions and list items have a PlatformId, 0 meaning "any platform". Collections have a PlatformId
  used for their missing games (0 = per game, as above).
- `titleSlug` is `{igdbId}-{platformId}` because IgdbId isn't unique in the library anymore.
- Lookup results (`/api/v3/game/lookup`) have `availablePlatforms` (enabled Gamarr platform ids mapped from the IGDB
  platforms) and default `platformId`/`platform` to the first of them. `/game/lookup` accepts an optional
  `platformId` filter.
- Credits are company credits with `type` developer/publisher/porting/supporting, one row per company and role.
  Ratings are `igdb` (users) and `critic` (aggregated), both 0-100.
- Images: cover -> Poster (`t_original`), first artwork (or first screenshot) -> Fanart (`t_1080p`), screenshots ->
  Screenshot (`t_1080p`), company logos (`t_logo_med`).
- The Twitch Client ID/Secret are sent in the token request body, not the URL, so they don't end up in logs.
- Game translations are kept but IGDB doesn't fill them; OriginalLanguage defaults to English.

## Releases: qualities

A "quality" describes the format and dump fidelity of a release, not a resolution. The seeded qualities are:

| Group | Qualities |
|---|---|
| Unknown | Unknown |
| ROM (cartridge) | ROM-Unverified (hacks, bad dumps, overdumps [b]/[o]), ROM-Verified (No-Intro / GoodTools [!]) |
| Disc image | Disc-Compressed-Lossy (CSO, ZSO, PBP), Disc-ISO (ISO/BIN+CUE/IMG/GDI/CDI), Disc-CHD/RVZ (lossless compressed), Disc-Redump (verified dump) |
| Console digital | NSP/XCI/NSZ (Switch), PKG (PS3/PS4/PSVita/PSP), WUA/WUX/RPX (Wii U), CIA/3DS (3DS), XEX/GOD (Xbox 360) |
| PC | PC-Repack (FitGirl, DODI, ElAmigos…), PC-Scene (cracked ISO/folder: CODEX, RUNE, TENOKE, SKIDROW, RAZOR1911…), PC-Portable, PC-GOG (DRM-free installer), PC-Steam-Rip |

Quality profiles keep Radarr's shape (allowed qualities, cutoff, upgrade allowed, custom formats, min score).
The defaults are "Any", "Verified ROMs / Redump", "PC – GOG preferred" and "PC – Scene/Repack".
Custom formats keep working. New specification types are **Region** (USA/Europe/Japan/World/…) and
**ReleaseType** (Base game, Update, DLC, Bundle/Complete, Hack, Translation, Homebrew, Demo, Beta/Proto).

## Releases: parsing

`Parser` handles game release names in these styles:
- Scene PC: `Game.Name.Update.v1.2.3-CODEX`, `Game.Name.MULTi10-ElAmigos`, `Game_Name_PS4-DUPLEX`,
  `Game.Name.NSW-VENOM`, `Game.Name.Deluxe.Edition-RUNE`
- No-Intro / Redump / TOSEC: `Game Name (USA) (En,Fr,De) (Rev 1).zip`, `Game Name (Europe) (Disc 1).chd`,
  `Game Name (1995)(Publisher)(US)[!].zip`
- Repacks: `Game Name [FitGirl Repack]`, `Game Name - [DODI Repack]`, `Game Name v1.0.5 [GOG]`
- Switch: `Game Name [0100ABCD12345000][v0][US].nsp`, titles with `[UPDATE]`/`[DLC]` tags

From these it extracts: game title (and year when present), platform (from tokens like PS4, NSW, PS2,
"SNES", file extension or Newznab category), region(s), languages (`En,Fr` lists, MULTi), version/revision
(`v1.2`, `Rev 1`, `Build 1234567`), disc number, release type (base/update/DLC/hack…), edition (GOTY,
Deluxe, Definitive, Complete), release group, and quality (as above, using the extension and the group/tag).
Repack/Proper and revisions map onto the existing Revision model.

## Import and media files

- Game files are identified by the platform's FileExtensions plus generic ones (.iso .bin .cue .chd .rvz
  .7z .zip .rar .exe .pkg .nsp .xci). There's no MediaInfo/ffprobe step and no runtime-based sample
  detection, so both are removed or no-op'd.
- A release can span several files (bin/cue tracks, multi-disc sets + .m3u, PC installer parts or a whole
  installed folder, updates/DLC). The GameFile is the primary file (a .cue, .m3u, setup.exe, .iso…).
  The companion files from the same release are imported next to it and kept together, so they get moved,
  renamed and deleted with it.
- Default folder layout: `{Root}/{Platform Folder}/{Game Title} ({Release Year})/`. The naming tokens
  `{Platform}`, `{Platform Folder}`, `{Region}`, `{Version}`, `{Release Type}` and `{Disc}` are added, and
  movie-only tokens are removed.

## Stage 3 implementation notes

### Qualities and profiles

- Quality ids start at 100 so a persisted Radarr id can never be read as a game quality: Unknown 0,
  ROM-Unverified 100, ROM-Verified 101, Disc-Compressed-Lossy 110, Disc-ISO 111, Disc-CHD-RVZ 112, Disc-Redump 113,
  NSP-XCI 120, PKG 121, WUA-WUX 122, CIA-3DS 123, XEX-GOD 124, PC-Portable 130, PC-Steam-Rip 131, PC-Repack 132,
  PC-Scene 133, PC-GOG 134 (weights follow that order). Names use dashes instead of slashes because they end up in
  file names.
- `QualitySource` is Unknown/Cartridge/Disc/Digital/PC. Resolution and the quality modifier are gone (the custom
  format conditions too); the Source condition now matches those five values.
- Size limits are absolute sizes of the whole release in GB (fractions allowed for ROMs), between 0 and 1000. A max
  of null is unlimited, a null preferred size means "largest wins". The release comparer rounds sizes to 200 MB.
- Fresh installs get "Any" (every quality incl. Unknown, cutoff ROM-Verified), "Verified ROMs / Redump",
  "PC – GOG preferred" and "PC – Scene/Repack". The default profiles don't filter on language (`Any`), game
  releases rarely have language tags.
- Migration 245 deletes the quality definitions (they're re-seeded on startup), resets file/history/blocklist
  qualities to Unknown, drops pending releases, makes existing profiles allow every game quality and adds the three
  other default profiles, removes resolution/modifier/source conditions from custom formats, drops
  `GameFiles.MediaInfo` and the media info/hardcoded subs settings, and replaces the movie default file format.

### Parsing

- `Parser` takes the title from everything before the first "marker": a bracket group, a version (`v1.2`,
  `1.2.3`, `Build 123`), a platform token, an edition (`Deluxe Edition`, `GOTY`, `The Final Cut`), a release type
  word (Update, DLC, Demo, Crackfix…), a language or region token, a scene tag (REPACK, PROPER, MULTi10…) or a
  `+` (included content). Platform words that are also ordinary words (Switch, Arcade, Mac…) only count at the end or
  before another marker, so "Nintendo.Switch.Sports.NSW" keeps its title. In No-Intro/TOSEC names (with region tags)
  editions are part of the title. Years in brackets set `Year`; a trailing year in a scene title is kept and a second
  title without it is added ("Hitman 2016" + "Hitman").
- No-Intro ", The" titles are turned around. GOG installer names (`setup_x_1.2_(123).exe`, `gog_x_1.0.sh`) are
  parsed as "X v1.2 [GOG]" with the platform from the extension.
- Platforms are identified by IGDB platform id (`PlatformParser`): scene tokens (NSW, PS4, XBOX360, Wii.U…),
  bracket aliases (any platform name/short name/folder name, "[PS Vita]"), unique extensions (.sfc, .nsp, .gdi…) and
  PC scene/repack groups. SNES/Super Famicom, NES/Famicom, DS/DSi, 3DS/New 3DS, Neo Geo AES/MVS, PC Engine/SuperGrafx,
  Switch/Switch 2 and PC/DOS are compatible. `ParsingService` resolves it to the Gamarr platform id
  (`ParsedGameInfo.PlatformId`).
- Regions: No-Intro names, GoodTools letters (`(U)`, `(JUE)`), TOSEC/Switch country codes (`(US-EU)`, `[US]`) and
  scene tokens (USA, EUR, JPN, PAL, NTSC-J…). Languages: `(En,Fr,De)`, TOSEC `(en-de)`, `[T+Eng]`, words (GERMAN,
  ENG…). `MULTi10` sets no languages (the indexer's multi-language setting still applies).
- Release types: Hack (`[h]`, `(Hack)`), Translation (`[T+Eng]`), Homebrew (`(Homebrew)`, `(PD)`), Demo (`(Demo)`,
  `(Kiosk)`, `(Sample)`), Prototype (`(Proto)`, `(Beta)`), Update (Update/Patch/Crackfix, Switch title id ending in
  800), DLC (DLC/Unlocker, other Switch title ids), Bundle ("+ 5 DLCs", "incl. DLC", All DLCs). Included updates
  ("+ Update 3") don't make a release an update.
- Revisions: PROPER, scene REPACK/RERIP and No-Intro `(Rev 1)`/`(Rev A)` (version 2). "[FitGirl Repack]" style
  repacks are a quality, not a revision.
- Quality detection order: tags in the name (Redump, repack groups, GOG, Steam Rip, Portable, CHD/RVZ, CSO/WBFS,
  NSP/XCI, WUA, CIA, XBLA/GOD, PKG/PSN, PC scene groups, ISO), then the extension (ROM extensions are
  ROM-Verified when the name has `[!]`, No-Intro or a No-Intro region tag and no bad dump flags), then the
  platform (cartridge → ROM, Switch → NSP-XCI, PS4/PS5/Vita → PKG, Wii U → WUA-WUX, 3DS → CIA-3DS, other consoles →
  Disc-ISO). A platform guess counts like an extension (lower confidence than a tag). When a release doesn't name
  its platform, its quality is parsed again with the matched game's platform.

### Decision engine and custom formats

- Removed: hardcoded subs, raw disk (BR/DVD) specs, IMDb ids in mapping. Added `PlatformMatchSpecification` (the
  release's platform has to be compatible with the game's; a release without a platform is only accepted from
  searches) and `ReleaseTypeAllowedSpecification` (Update, DLC, Demo, Hack, Translation and Prototype releases are
  rejected unless a custom format with a matching Release Type condition has a positive score in the profile;
  Prototype is rejected too, which goes a bit beyond the brief).
- Mapping by title only considers games on the release's platform, and an ambiguous title (same game on several
  platforms, release without platform) isn't matched. The title with the edition appended is tried as well. Years
  match within ±1 (ports), an exact year wins.
- New custom format conditions: Region (with "include World"), Release Type and Platform (value = IGDB platform
  id, so formats can be shared). Release Title, Release Group, Edition, Language, Size, Indexer Flags, Year and
  Source are kept. Game files store Regions, Version, ReleaseType, DiscNumber and DiscCount so custom formats and
  naming work for existing files.

### Import and multi-file releases

- `MediaFileExtensions` is the union of all platforms' extensions plus generic disc/archive/installer formats. A
  game file has to be in a format of the game's platform or a generic one (zip/7z/rar, iso/bin/cue/img/chd/m3u…).
- MediaInfo/ffprobe is gone (no media info on files, no runtime sample detection). Samples are files named
  sample/demo or in a Sample folder (unless the game's title has that word).
- All game files of a download (or of a game folder without a game file) are one release
  (`GameReleaseFileService`). The primary file (the GameFile) is picked as .m3u > .cue > .gdi > .ccd/.mds/.cdi >
  single image/ROM/package/archive (base game before update/DLC, disc 1 first) > installer exe > largest. Loose files
  of several releases (a folder of ROMs, manual import of an unknown folder) are grouped by cue/gdi/m3u references
  and disc names. A release whose primary is an installer (.exe/.msi/.sh/.dmg) takes its whole folder tree.
- The other files are companions: a new extra file type (`CompanionFiles` table) imported with the game file. They
  keep their names and their path relative to the primary file's folder (or the release root), so cue sheets,
  playlists and installers keep working. Only the primary file is renamed, and installers and CloneCD/Alcohol
  images (.ccd/.mds) keep their original name. Companions are deleted (recycled) with their game file and moved with
  the game folder; a file format with sub folders would leave them in place. A rescan registers other game files
  next to the game file, everything in sub folders, and every other file for PC games, as companions.
- ROMs in archives are game files. A RAR set (.r00/.partN.rar) or a RAR for a PC game is rejected with "needs to be
  extracted"; executables aren't rejected anymore.
- Manual import lists one item per release with its `companionFiles`; the import command can pass them back, if not
  they're looked up again from the primary file.
- Unmapped folders: root folder sub folders named like a platform's folder are platform folders, their sub folders
  are games on that platform (`UnmappedFolder.PlatformId`); other folders are games without a platform. The library
  import passes the platform to the lookup and to the added game.

### Naming

- Tokens: `{Region}` (USA, Europe), `{Languages}` (En,Fr,De), `{Version}` (v1.1 / Build 123), `{Release Type}`
  (empty for base games), `{Disc}` (Disc 1), `{Edition}`, `{Platform}`, `{Platform Short}`, `{Platform Folder}`,
  `{Quality Title}`/`{Quality Full}`, `{Release Group}`, `{Original Title}`, `{Custom Formats}`, `{IgdbId}`.
  MediaInfo tokens are gone. The file-level tokens aren't allowed in the folder format.
- Default file format: `{Game Title} ({Release Year}){ (Region)}{ [Version]}`, e.g.
  "Super Mario World (1990) (USA, Europe) [v1.1].sfc".
- The "in cinemas" file date option is gone, the release date option uses the game's release date on its platform.

## Indexers and download clients

- Newznab/Torznab categories: Console 1000 (and subcategories 1010 NDS, 1020 PSP, 1030 Wii, 1040 Xbox,
  1050 Xbox 360, 1060 WiiWare, 1070 Xbox 360 DLC, 1080 PS3, 1110 3DS, 1120 PS Vita, 1130 WiiU,
  1140 Xbox One, 1180 PS4) and PC 4000 / 4050 Games.
  Each Platform knows its categories, and searches go out with `t=search&q=<title>` plus the platform's
  categories. There are no imdb/tmdb id parameters.
- The default download-client category is `gamarr`.

## Stage 4 implementation notes

- Newznab/Torznab default to `1000,1010,1020,1030,1040,1050,1060,1070,1080,1110,1120,1130,1140,1180,4050`
  (`NewznabStandardCategory.DefaultGameCategories`). The category picker hides Movies/Audio/TV/XXX/Books and falls
  back to the standard Console and PC trees when the caps can't be fetched.
- Caps: only the basic `search` element is used. `movie-search`/`tv-search`/any `game-search` and id parameters are
  ignored, and RSS is `t=search` without `q`. Radarr's "Remove year" setting is gone, the year isn't searched.
- Search requests: tier 1 is `t=search&q=<title>` for every search title (title, original title, translations and up
  to 5 latin-script IGDB alternative names). Tier 2, used only if tier 1 found nothing, is `<title> <platform short name>`.
  Categories are the configured categories that cover one of the platform's categories (a configured parent like
  1000/4000 covers its subcategories); if none do, all configured categories are used.
- FileList uses its own categories 9 (Jocuri PC) and 10 (Jocuri Console), mapped to 4050/1000 for the platform filter.
  Nyaa defaults to `&cats=6_2` (Software - Games). IPTorrents and Torrent RSS are RSS-only, the categories are part
  of the feed URL.
- `ReleaseInfo.ImdbId` is removed. `ReleaseInfo.Categories` holds the Newznab/Torznab `category` attributes (FileList
  maps its category name). `IPlatformCategoryMapper` maps them to platforms: console subcategories to the platforms that
  list them, 4000/4050 to PC (Windows), the generic 1000 to nothing. `IsCompatible(platform, categories)` is meant for
  a decision engine check.

## Integrations

- Metadata consumers write frontend files: an ES-DE / EmulationStation `gamelist.xml` (per platform folder)
  and a LaunchBox-compatible export (optional, later).
- Import lists: Gamarr→Gamarr sync, IGDB popular/upcoming by platform, and Steam/GOG wishlist (later).
- Notifications: the generic set (Discord, webhook, email…) is kept.
