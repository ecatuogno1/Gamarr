# Gamarr

Gamarr is a video game library manager for Usenet and BitTorrent users, in the spirit of the other *arr
applications. It is meant to manage ROMs, disc images and PC games: it monitors indexers for releases of the
games you want, sends them to your download client, then imports, sorts and renames the results into your
library.

Gamarr is under active early development, and much of the code still comes from the movie-oriented application
it is based on.

## Credits and license

Gamarr is a fork of [Radarr](https://github.com/Radarr/Radarr) and is distributed under the
[GNU General Public License v3.0](LICENSE), the same license as Radarr. A big thank you to the Radarr team and
everyone who has contributed to Radarr, Sonarr and the other *arr projects; Gamarr would not exist without their work.

Gamarr is not affiliated with or supported by the Radarr or Servarr teams. Please do not report Gamarr issues to them.

## Building

Requirements:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 24 with Yarn (`corepack enable`)

Backend:

```shell
dotnet build src/Gamarr.sln -c Debug
```

Frontend:

```shell
yarn install
yarn build
```

The build output goes to `_output/`. To start Gamarr from there:

```shell
dotnet _output/net10.0/Gamarr.dll
```

Then open <http://localhost:7171> (the default port is **7171**).

See [CONTRIBUTING.md](CONTRIBUTING.md) for more development details.

## Support

Bug reports and feature requests: [GitHub Issues](https://github.com/ecatuogno1/Gamarr/issues)
