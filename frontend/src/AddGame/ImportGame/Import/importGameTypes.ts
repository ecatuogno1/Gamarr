import AppState from 'App/State/AppState';
import Game, { GameAvailability, GameMonitor } from 'Game/Game';

// Shape of `state.importGame` (Store/Actions/importGameActions.js), which
// isn't part of the typed AppState yet.

export interface ImportGameItem {
  id: string;
  term?: string;
  path: string;
  relativePath: string;
  monitor?: GameMonitor;
  qualityProfileId?: number;
  minimumAvailability?: GameAvailability;
  selectedGame?: Game;
  items?: Game[];
  isFetching?: boolean;
  isPopulated?: boolean;
  isQueued?: boolean;
  error?: {
    responseJSON?: {
      message?: string;
    };
  } | null;
}

export interface ImportGameState {
  isLookingUpGame: boolean;
  isImporting: boolean;
  importError?: {
    responseJSON?: unknown;
  } | null;
  items: ImportGameItem[];
}

export interface UnmappedFolder {
  name: string;
  path: string;
  relativePath: string;
  // Set when the folder is inside a platform folder of the root folder
  platformId?: number;
}

export type ImportGameEligibility = 'ready' | 'unmatched' | 'existing';

export interface AddGameDefaults {
  monitor: GameMonitor;
  qualityProfileId: number;
  minimumAvailability: GameAvailability;
}

export type ImportGameAppState = AppState & {
  importGame: ImportGameState;
  addGame: {
    defaults: AddGameDefaults;
  };
};

export function getImportGameEligibility(
  item: ImportGameItem | undefined,
  existingIgdbIds: Set<number>
): ImportGameEligibility {
  const igdbId = item?.selectedGame?.igdbId;

  if (igdbId == null) {
    return 'unmatched';
  }

  return existingIgdbIds.has(igdbId) ? 'existing' : 'ready';
}
