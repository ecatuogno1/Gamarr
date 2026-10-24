import { orderBy } from 'lodash';
import {
  createSelector,
  createSelectorCreator,
  defaultMemoize,
} from 'reselect';
import AppState, { CustomFilter, Filter } from 'App/State/AppState';
import GamesAppState, { GameIndexAppState } from 'App/State/GamesAppState';
import Game from 'Game/Game';
import { filterTypes, sortDirections } from 'Helpers/Props';
import {
  createCustomFiltersSelector,
  itemMatchesFilters,
  sort,
} from 'Store/Selectors/createClientSideCollectionSelector';
import findSelectedFilters from 'Utilities/Filter/findSelectedFilters';
import { GameTitleItem, groupGamesByTitle, sortVersions } from './gameTitle';

export interface GameTitleCollectionState
  extends Omit<GamesAppState, 'items'>,
    GameIndexAppState {
  customFilters: CustomFilter[];
  items: GameTitleItem[];
  // Number of titles in the library
  totalItems: number;
  // Number of platform versions in the library
  totalVersions: number;
}

interface SelectedFilter {
  key: string;
  value: unknown;
  type?: string;
}

type FilterPredicates = Record<
  string,
  (item: Game, value: unknown, type: string) => boolean
>;

// Negative conditions have to hold for every version of a title ("not on SNES" hides a title that's on SNES),
// the other conditions are met when one version meets all of them ("on SNES", "missing").
const negativeFilterTypes: string[] = [
  filterTypes.NOT_EQUAL,
  filterTypes.NOT_CONTAINS,
  filterTypes.NOT_STARTS_WITH,
  filterTypes.NOT_ENDS_WITH,
];

export function getMatchingVersions(
  versions: Game[],
  selectedFilters: SelectedFilter[],
  filterPredicates?: FilterPredicates
) {
  if (!selectedFilters.length) {
    return versions;
  }

  const negative = selectedFilters.filter((f) =>
    negativeFilterTypes.includes(f.type ?? filterTypes.EQUAL)
  );
  const positive = selectedFilters.filter((f) => !negative.includes(f));

  if (
    negative.length &&
    !versions.every((v) => itemMatchesFilters(v, negative, filterPredicates))
  ) {
    return [];
  }

  return positive.length
    ? versions.filter((v) => itemMatchesFilters(v, positive, filterPredicates))
    : versions;
}

type TitleGroup = { igdbId: number; versions: Game[]; matching: Game[] };

function sortTitles(
  titles: TitleGroup[],
  state: { sortKey: string; sortDirection: string }
): TitleGroup[] {
  if (state.sortKey === 'sizeOnDisk') {
    // A title's size is the size of all its versions
    return orderBy(
      titles,
      [
        (t) =>
          t.versions.reduce(
            (acc, v) => acc + (v.statistics?.sizeOnDisk ?? 0),
            0
          ),
        (t) => t.versions[0].sortTitle,
      ],
      [state.sortDirection === sortDirections.ASCENDING ? 'asc' : 'desc', 'asc']
    );
  }

  // Otherwise a title sorts by its best placed matching version
  const position = new Map<number, number>();

  sort(
    titles.flatMap((t) => t.matching),
    state
  ).forEach((game: Game, index: number) => {
    if (!position.has(game.igdbId)) {
      position.set(game.igdbId, index);
    }
  });

  return [...titles].sort(
    (a, b) => (position.get(a.igdbId) ?? 0) - (position.get(b.igdbId) ?? 0)
  );
}

export function groupFilterAndSortTitles(
  games: Game[],
  state: {
    selectedFilterKey?: string | number;
    filters?: Filter[];
    customFilters?: CustomFilter[];
    filterPredicates?: FilterPredicates;
    sortKey: string;
    sortDirection: string;
  }
): GameTitleItem[] {
  const selectedFilters: SelectedFilter[] = state.selectedFilterKey
    ? findSelectedFilters(
        state.selectedFilterKey,
        state.filters,
        state.customFilters
      )
    : [];

  const titles: TitleGroup[] = [];

  groupGamesByTitle(games).forEach((versions, igdbId) => {
    const matching = getMatchingVersions(
      versions,
      selectedFilters,
      state.filterPredicates
    );

    if (matching.length) {
      titles.push({ igdbId, versions, matching });
    }
  });

  const sorted = sortTitles(titles, state);

  return sorted.map(({ igdbId, versions, matching }) => {
    const sortedVersions = sortVersions(versions);

    return {
      id: igdbId,
      igdbId,
      sortTitle: sortedVersions[0].sortTitle,
      gameIds: sortedVersions.map((v) => v.id),
      matchingGameIds: sortVersions(matching).map((v) => v.id),
    };
  });
}

function createUnoptimizedSelector(uiSection: 'gameIndex') {
  return createSelector(
    (state: AppState) => state.games,
    (state: AppState) => state[uiSection],
    createCustomFiltersSelector('games', uiSection),
    (gamesState, uiState, customFilters): GameTitleCollectionState => {
      const state = { ...gamesState, ...uiState, customFilters };

      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore filterPredicates and sortPredicates aren't typed
      const items = groupFilterAndSortTitles(gamesState.items, state);

      return {
        ...state,
        items,
        totalItems: new Set(gamesState.items.map((g: Game) => g.igdbId)).size,
        totalVersions: gamesState.items.length,
      };
    }
  );
}

function hasSameTitles(a: GameTitleItem[], b: GameTitleItem[]) {
  if (a === b) {
    return true;
  }

  if (a.length !== b.length) {
    return false;
  }

  return a.every((item, index) => {
    const other = b[index];

    return (
      item.id === other.id &&
      item.sortTitle === other.sortTitle &&
      item.gameIds.join() === other.gameIds.join() &&
      item.matchingGameIds.join() === other.matchingGameIds.join()
    );
  });
}

const createTitlesEqualSelector = createSelectorCreator(
  defaultMemoize,
  (a: GameTitleCollectionState, b: GameTitleCollectionState) => {
    if (a === b) {
      return true;
    }

    // Keep the same object while only the version details changed, the cells select their own games
    const { items: aItems, ...aOther } = a;
    const { items: bItems, ...bOther } = b;

    return (
      hasSameTitles(aItems, bItems) &&
      Object.keys(aOther).every(
        (key) =>
          key === 'items' ||
          aOther[key as keyof typeof aOther] ===
            bOther[key as keyof typeof bOther]
      ) &&
      Object.keys(aOther).length === Object.keys(bOther).length
    );
  }
);

function createGameTitleCollectionSelector(
  uiSection: 'gameIndex' = 'gameIndex'
) {
  return createTitlesEqualSelector(
    createUnoptimizedSelector(uiSection),
    (state) => state
  );
}

export default createGameTitleCollectionSelector;

// Shared instance for the library index and its toolbar buttons
export const selectGameIndexTitles = createGameTitleCollectionSelector();
