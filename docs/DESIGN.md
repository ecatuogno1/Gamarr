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
  Generation, FolderName (ES-DE/RetroArch style: "snes", "psx", "pc"), FileExtensions (csv),
  NewznabCategories (csv), Enabled`.
  Every Game has a required `PlatformId`. A title released on several platforms is one Game record per
  platform. The unique key is **(IgdbId, PlatformId)**, not IgdbId on its own, so every lookup or uniqueness
  check that used to be by TmdbId has to take the platform into account.
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
  "minimum availability" based on cinema dates. Availability is just "Released" or "Announced".
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

## Indexers and download clients

- Newznab/Torznab categories: Console 1000 (and subcategories 1010 NDS, 1020 PSP, 1030 Wii, 1040 Xbox,
  1050 Xbox 360, 1060 WiiWare, 1070 Xbox 360 DLC, 1080 PS3, 1110 3DS, 1120 PS Vita, 1130 WiiU,
  1140 Xbox One, 1180 PS4) and PC 4000 / 4050 Games.
  Each Platform knows its categories, and searches go out with `t=search&q=<title>` plus the platform's
  categories. There are no imdb/tmdb id parameters.
- The default download-client category is `gamarr`.

## Integrations

- Metadata consumers write frontend files: an ES-DE / EmulationStation `gamelist.xml` (per platform folder)
  and a LaunchBox-compatible export (optional, later).
- Import lists: Gamarr→Gamarr sync, IGDB popular/upcoming by platform, and Steam/GOG wishlist (later).
- Notifications: the generic set (Discord, webhook, email…) is kept.
