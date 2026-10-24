# Getting started with Gamarr

Gamarr manages a video game library (ROMs, disc images and PC games) the way Radarr manages movies: it watches
indexers for releases of the games you want, sends them to your download client and imports, renames and organizes
the finished downloads. Game information comes from [IGDB](https://www.igdb.com).

## Run from source

Requirements: the .NET SDK from `global.json` (10.0), Node.js 20+ and Yarn 1.

```bash
# Backend
dotnet build src/Gamarr.sln -c Debug

# Frontend (writes _output/UI)
yarn install
yarn build

# Run
dotnet _output/net10.0/Gamarr.dll -nobrowser -data=/path/to/gamarr-data
```

Gamarr listens on <http://localhost:7171> (HTTPS on 7172 when enabled). `-data` is where the database, logs, backups
and cached covers go; leave it out to use the default AppData folder. During frontend work `yarn start` rebuilds the UI
on changes.

On first start Gamarr asks you to choose an authentication method, then work through the settings in this order.

## 1. IGDB credentials (required)

IGDB is accessed through a Twitch application. Without credentials you can't search for, add or refresh games, and
System > Status shows a health warning.

1. Sign in at the [Twitch developer console](https://dev.twitch.tv/console) (a Twitch account with two-factor
   authentication enabled is required).
2. Applications > **Register Your Application**: any name (e.g. "Gamarr at home"), OAuth Redirect URL
   `http://localhost`, category **Application Integration**, client type **Confidential**.
3. Open the application with **Manage**, copy the **Client ID** and create a **New Secret**.
4. In Gamarr, go to **Settings > Metadata** and enter the Twitch Client ID and Client Secret, then save.

Gamarr requests an app access token with these credentials and caches it until it expires. IGDB allows 4 requests a
second, Gamarr stays below that.

## 2. Platforms and folders

**Settings > Platforms** lists the 93 supported platforms with the folder name, file extensions and Newznab/Torznab
categories of each. Disable the ones you don't collect for, so they aren't offered when adding games.

A game is always on one platform. The same title on two platforms is two games in Gamarr (e.g. Hollow Knight on PC
and on Switch), each with its own folder, file and quality profile.

Recommended layout, one root folder with a folder per platform (the default game folder format is
`{Platform Folder}/{Game Title} ({Release Year})`):

```
/games
├── pc
│   └── The Witcher 3 - Wild Hunt (2015)
│       ├── setup_the_witcher_3_wild_hunt_goty_4.04a.exe
│       └── setup_the_witcher_3_wild_hunt_goty_4.04a-1.bin
├── psx
│   └── Spyro 2 - Ripto's Rage! (1999)
│       ├── Spyro 2 - Ripto's Rage! (1999) (USA).cue
│       ├── Spyro 2 - Ripto's Rage! (USA) (Track 1).bin
│       └── Spyro 2 - Ripto's Rage! (USA) (Track 2).bin
└── snes
    └── Super Mario World (1990)
        └── Super Mario World (1990) (USA).sfc
```

The platform folder names (`snes`, `psx`, `pc`…) follow ES-DE/RetroArch conventions, so the root folder can be used by
an emulation frontend as its ROM directory. Add `/games` under **Settings > Media Management > Root Folders**. When
you import an existing library (**Games > Library Import**), Gamarr recognizes platform folders by their folder name
and looks the games inside them up on that platform.

A release can consist of several files (bin/cue tracks, multi-disc sets with an .m3u, installer parts). Gamarr imports
the primary file (the .cue, .m3u, setup.exe…) as the game file and keeps the other files as companion files next to
it; they're moved, renamed and deleted together. Turn on **Rename Games** in Media Management to rename the primary
file with the naming tokens (`{Region}`, `{Version}`, `{Disc}`…).

## 3. Quality profiles and custom formats

Qualities describe the format and dump of a release (ROM-Verified, Disc-Redump, NSP-XCI, PC-GOG, PC-Scene…), see
**Settings > Quality**. Four profiles are created: Any, Verified ROMs / Redump, PC – GOG preferred and
PC – Scene/Repack. Custom formats can match Region, Release Type and Platform besides the usual conditions; updates,
DLC, demos, hacks, translations and prototypes are only grabbed when a custom format with a matching Release Type
condition scores positive in the profile.

## 4. Indexers through Prowlarr

Prowlarr has no Gamarr app sync yet, so add each indexer manually, pointing at Prowlarr's per-indexer feeds:

1. In Prowlarr, open the indexer and copy its **Torznab Feed** (or **Newznab Feed** for usenet indexers) URL, e.g.
   `http://prowlarr:9696/3/api`, and Prowlarr's API key (Settings > General).
2. In Gamarr, **Settings > Indexers > +** and choose **Torznab** (or **Newznab**). Enter the feed URL without the
   trailing `/api` as the URL (`http://prowlarr:9696/3/`), keep the API path `/api` and paste Prowlarr's API key.
3. Keep the default categories (Console 1000 and its sub categories, PC/Games 4050) unless the indexer uses others.

Gamarr searches with the game title (`t=search`) inside the categories of the game's platform, a second pass adds the
platform's short name to the title. Releases that don't name their platform are matched through their indexer
categories (a release in PS3 1080 is rejected for an SNES game); from RSS such releases are only accepted when the
categories name the game's platform.

## 5. Download client

Add your download client under **Settings > Download Clients**. The category defaults to `gamarr`; create that category
(or label) in the client so Gamarr's downloads are kept apart and it only imports its own. If the client runs in a
container or on another machine, set up remote path mappings like in the other \*arr apps.

## 6. EmulationStation (ES-DE) metadata

**Settings > Metadata > EmulationStation (ES-DE)** keeps a `gamelist.xml` per platform (ES-DE "system") when games are
imported or rescanned:

- one `<game>` entry per Gamarr game with path, name, description, release date, developer, publisher, genre,
  players (only when IGDB says single player) and rating (0–1). `<path>` is relative to the platform folder, e.g.
  `./Super Mario World (1990)/Super Mario World (1990) (USA).sfc`;
- existing entries of other games are kept. Gamarr finds its own entries by `id`/`source="IGDB"` or by a path inside the
  game folder, updates the fields it manages and keeps the rest (favorite, playcount, lastplayed, hidden…). An
  invalid gamelist.xml isn't touched. Deleting a game with its files removes its entry, moving a game (new root folder or
  platform) moves its entry to the gamelist of the new location.

Pick the **gamelist.xml Location** that matches your frontend:

| Location | gamelist.xml | Images (with **Game Images** on) | Frontends |
|---|---|---|---|
| ROM folder (default) | `<root>/<platform folder>/gamelist.xml` | `<game folder>/media/cover.jpg`, `screenshot.jpg`, `fanart.jpg`, referenced from `<image>`, `<thumbnail>`, `<fanart>` | RetroPie, Batocera, EmulationStation, ES-DE with `LegacyGamelistFileLocation` |
| ES-DE gamelists directory | `<ES-DE Gamelists Directory>/<platform folder>/gamelist.xml` (default `~/ES-DE/gamelists`) | `<ES-DE Media Directory>/<platform folder>/covers\|screenshots\|fanart/<game folder>/<game file name>.jpg` (default `~/ES-DE/downloaded_media`) | ES-DE 2.0 and later |

How the frontends read them:

- **ES-DE 2.0+** only reads `~/ES-DE/gamelists/<system>/gamelist.xml` and ignores `gamelist.xml` files in the ROM
  folders unless `LegacyGamelistFileLocation` is set to `true` in `es_settings.xml`. It ignores image tags and finds media
  by the ROM's path and file name below `downloaded_media/<system>/<covers|screenshots|fanart…>/`, mirroring sub folders,
  which is the layout the ES-DE location writes. Use the ES-DE location, with the directories pointing at the ES-DE folder
  of the machine running ES-DE (`~` is the home folder of the user Gamarr runs as; in Docker map the ES-DE folder into the
  container and use the mapped path). ES-DE system names are the platform folder names (`snes`, `psx`, `gc`…), so the
  root folder works as ES-DE's ROM directory.
- **RetroPie** reads `<ROM folder>/gamelist.xml` first, then `~/.emulationstation/gamelists/<system>/`, and **Batocera**
  reads `/userdata/roms/<system>/gamelist.xml`; both use the `./media/...` image tags. Use the ROM folder location.

When ES-DE itself saves an entry (e.g. after you mark a favorite) it rewrites the whole entry; Gamarr's fields are written
again on the next rescan.

## Troubleshooting

- Searching for a game says IGDB credentials are missing: see step 1.
- A release is rejected as "wrong platform": the release names another platform, or its indexer categories do.
  Check the platform of the game and the categories of the indexer.
- Logs are under the data folder in `logs/`, or in **System > Log Files**.
