import Game from 'Game/Game';
import { kinds } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import getProgressBarKind from 'Utilities/Game/getProgressBarKind';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';

// A title is every Game (platform version) with the same IGDB id.

export interface GameTitleItem {
  // The IGDB id, used as the selection key in the library index
  id: number;
  igdbId: number;
  sortTitle: string;
  // Every platform version of the title, sorted by platform name
  gameIds: number[];
  // The versions that match the selected filter
  matchingGameIds: number[];
}

export interface GameTitleVersion {
  game: Game;
  kind: Kind;
  isDownloading: boolean;
  statusText: string;
  tooltip: string;
}

export interface GameTitleSummary {
  primary: Game;
  versions: GameTitleVersion[];
  totalCount: number;
  downloadedCount: number;
  monitoredCount: number;
  isDownloading: boolean;
  monitored: boolean;
  sizeOnDisk: number;
  kind: Kind;
  progress: number;
}

export function getPlatformName(game: Pick<Game, 'platform'>) {
  return game.platform?.name ?? '';
}

export function sortVersions<T extends Pick<Game, 'platform' | 'id'>>(
  games: T[]
): T[] {
  return [...games].sort((a, b) => {
    const result = getPlatformName(a).localeCompare(getPlatformName(b));

    return result === 0 ? a.id - b.id : result;
  });
}

export function groupGamesByTitle<T extends Pick<Game, 'igdbId'>>(
  games: T[]
): Map<number, T[]> {
  return games.reduce((acc, game) => {
    const versions = acc.get(game.igdbId);

    if (versions) {
      versions.push(game);
    } else {
      acc.set(game.igdbId, [game]);
    }

    return acc;
  }, new Map<number, T[]>());
}

// "Super Mario World (SNES)"
export function getTitleWithPlatform(
  title: string,
  platform?: { shortName?: string; name?: string } | null
) {
  const platformName = platform?.shortName || platform?.name;

  return platformName ? `${title} (${platformName})` : title;
}

// The title page: /game/{igdbId}, or /game/{igdbId}-{platformId} with a platform selected
export function getTitleLink(igdbId: number, platformId?: number) {
  return platformId ? `/game/${igdbId}-${platformId}` : `/game/${igdbId}`;
}

function getVersionStatusText(game: Game, isDownloading: boolean) {
  if (isDownloading) {
    return translate('Downloading');
  }

  if (game.hasFile) {
    return game.gameFile?.quality?.quality.name ?? translate('Downloaded');
  }

  if (game.status === 'deleted') {
    return translate('Deleted');
  }

  if (!game.monitored) {
    return translate('Unmonitored');
  }

  return game.isAvailable ? translate('Missing') : translate('NotAvailable');
}

export function getGameTitleVersion(
  game: Game,
  isDownloading: boolean
): GameTitleVersion {
  const statusText = getVersionStatusText(game, isDownloading);
  const sizeOnDisk = game.statistics?.sizeOnDisk ?? 0;
  const details = [statusText];

  if (game.hasFile && sizeOnDisk) {
    details.push(formatBytes(sizeOnDisk));
  }

  if (game.hasFile && !game.monitored) {
    details.push(translate('Unmonitored'));
  }

  return {
    game,
    kind: getProgressBarKind(
      game.status,
      game.monitored,
      game.hasFile,
      game.isAvailable,
      isDownloading
    ),
    isDownloading,
    statusText,
    tooltip: `${getPlatformName(game)}: ${details.join(', ')}`,
  };
}

export function getGameTitleSummary(
  games: Game[],
  downloadingGameIds: Set<number>
): GameTitleSummary | undefined {
  if (!games.length) {
    return undefined;
  }

  const versions = sortVersions(games).map((game) =>
    getGameTitleVersion(game, downloadingGameIds.has(game.id))
  );

  const totalCount = versions.length;
  const downloadedCount = versions.filter((v) => v.game.hasFile).length;
  const monitoredVersions = versions.filter((v) => v.game.monitored);
  const isDownloading = versions.some((v) => v.isDownloading);
  const sizeOnDisk = versions.reduce(
    (acc, v) => acc + (v.game.statistics?.sizeOnDisk ?? 0),
    0
  );

  let kind: Kind = kinds.PRIMARY;

  if (isDownloading) {
    kind = kinds.PURPLE;
  } else if (
    monitoredVersions.some(
      (v) =>
        !v.game.hasFile && v.game.isAvailable && v.game.status !== 'deleted'
    )
  ) {
    kind = kinds.DANGER;
  } else if (
    monitoredVersions.length &&
    monitoredVersions.every((v) => v.game.hasFile)
  ) {
    kind = kinds.SUCCESS;
  } else if (!monitoredVersions.length) {
    kind = downloadedCount ? kinds.DEFAULT : kinds.WARNING;
  }

  // The version with a file (or the first one) stands in for the title
  const primary =
    versions.find((v) => v.game.hasFile && v.game.monitored)?.game ??
    versions[0].game;

  return {
    primary,
    versions,
    totalCount,
    downloadedCount,
    monitoredCount: monitoredVersions.length,
    isDownloading,
    monitored: monitoredVersions.length > 0,
    sizeOnDisk,
    kind,
    progress: totalCount ? (downloadedCount / totalCount) * 100 : 0,
  };
}

export function getMonitoredText(summary: GameTitleSummary) {
  if (summary.monitoredCount === summary.totalCount) {
    return translate('Monitored');
  }

  if (!summary.monitoredCount) {
    return translate('Unmonitored');
  }

  return translate('PlatformsMonitoredOfTotal', {
    monitored: summary.monitoredCount,
    total: summary.totalCount,
  });
}
