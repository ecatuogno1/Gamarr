import { createAction } from 'redux-actions';
import { filterBuilderTypes, filterBuilderValueTypes, sortDirections } from 'Helpers/Props';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import { filterPredicates, filters, sortPredicates } from './gameActions';

//
// Variables

export const section = 'gameIndex';

//
// State

export const defaultState = {
  isSaving: false,
  saveError: null,
  isDeleting: false,
  deleteError: null,
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortTitle',
  secondarySortDirection: sortDirections.ASCENDING,
  view: 'posters',

  posterOptions: {
    detailedProgressBar: true,
    size: 'large',
    showTitle: false,
    showMonitored: true,
    showQualityProfile: true,
    showReleaseDate: false,
    showIgdbRating: false,
    showCriticRating: false,
    showTags: false,
    showSearchAction: false
  },

  overviewOptions: {
    detailedProgressBar: true,
    size: 'medium',
    showMonitored: true,
    showPlatform: true,
    showQualityProfile: true,
    showAdded: false,
    showPath: false,
    showSizeOnDisk: false,
    showTags: false,
    showSearchAction: false
  },

  tableOptions: {
    showSearchAction: false
  },

  columns: [
    {
      name: 'select',
      columnLabel: 'Select',
      isSortable: false,
      isVisible: true,
      isModifiable: false,
      isHidden: true
    },
    {
      name: 'status',
      columnLabel: () => translate('ReleaseStatus'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'sortTitle',
      label: () => translate('GameTitle'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'originalTitle',
      label: () => translate('OriginalTitle'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'collection',
      label: () => translate('Collection'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'platform',
      label: () => translate('Platform'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'developers',
      label: () => translate('Developers'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'qualityProfileId',
      label: () => translate('QualityProfile'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'originalLanguage',
      label: () => translate('OriginalLanguage'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'added',
      label: () => translate('Added'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'year',
      label: () => translate('Year'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'releaseDate',
      label: () => translate('ReleaseDate'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'minimumAvailability',
      label: () => translate('MinimumAvailability'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'path',
      label: () => translate('Path'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'sizeOnDisk',
      label: () => translate('SizeOnDisk'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'genres',
      label: () => translate('Genres'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'keywords',
      label: () => translate('Keywords'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'gameStatus',
      label: () => translate('Status'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'igdbRating',
      label: () => translate('IgdbRating'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'criticRating',
      label: () => translate('CriticRating'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'popularity',
      label: () => translate('Popularity'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'releaseGroups',
      label: () => translate('ReleaseGroup'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'gameFileQualities',
      label: () => translate('GameFileQualities'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'tags',
      label: () => translate('Tags'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'actions',
      columnLabel: () => translate('Actions'),
      isVisible: true,
      isModifiable: false
    }
  ],

  sortPredicates: {
    ...sortPredicates,

    platform: function(item) {
      const { platform } = item;

      return platform ? platform.name.toLowerCase() : '';
    },

    developers: function(item) {
      const { developers = [] } = item;

      return developers.length ? developers[0].toLowerCase() : '';
    },

    collection: function(item) {
      const { collection ={} } = item;

      return collection.title;
    },

    originalLanguage: function(item) {
      const { originalLanguage ={} } = item;

      return originalLanguage.name;
    },

    releaseGroups: function(item) {
      const { statistics = {} } = item;
      const { releaseGroups = [] } = statistics;

      return releaseGroups.length ?
        releaseGroups
          .map((group) => group.toLowerCase())
          .sort((a, b) => a.localeCompare(b)) :
        undefined;
    },

    igdbRating: function({ ratings = {} }) {
      return ratings.igdb ? ratings.igdb.value : 0;
    },

    criticRating: function({ ratings = {} }) {
      return ratings.critic ? ratings.critic.value : 0;
    }

  },

  selectedFilterKey: 'all',

  filters,
  filterPredicates,

  filterBuilderProps: [
    {
      name: 'monitored',
      label: () => translate('Monitored'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'isAvailable',
      label: () => translate('ConsideredAvailable'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'minimumAvailability',
      label: () => translate('MinimumAvailability'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.MINIMUM_AVAILABILITY
    },
    {
      name: 'title',
      label: () => translate('Title'),
      type: filterBuilderTypes.STRING
    },
    {
      name: 'originalTitle',
      label: () => translate('OriginalTitle'),
      type: filterBuilderTypes.STRING
    },
    {
      name: 'originalLanguage',
      label: () => translate('OriginalLanguage'),
      type: filterBuilderTypes.EXACT,
      optionsSelector: function(items) {
        const collectionList = items.reduce((acc, game) => {
          if (game.originalLanguage) {
            acc.push({
              id: game.originalLanguage.name,
              name: game.originalLanguage.name
            });
          }

          return acc;
        }, []);

        return collectionList.sort(sortByProp('name'));
      }
    },
    {
      name: 'releaseGroups',
      label: () => translate('ReleaseGroups'),
      type: filterBuilderTypes.ARRAY,
      optionsSelector: function(items) {
        const groupList = items.reduce((acc, game) => {
          const { statistics = {} } = game;
          const { releaseGroups = [] } = statistics;

          releaseGroups.forEach((releaseGroup) => {
            acc.push({
              id: releaseGroup,
              name: releaseGroup
            });
          });

          return acc;
        }, []);

        return groupList.sort(sortByProp('name'));
      }
    },
    {
      name: 'gameFileQualities',
      label: () => translate('GameFileQualities'),
      type: filterBuilderTypes.ARRAY,
      valueType: filterBuilderValueTypes.QUALITY
    },
    {
      name: 'status',
      label: () => translate('ReleaseStatus'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.RELEASE_STATUS
    },
    {
      name: 'platformId',
      label: () => translate('Platform'),
      type: filterBuilderTypes.EXACT,
      optionsSelector: function(items) {
        const platformList = items.reduce((acc, game) => {
          if (game.platform && acc.findIndex((a) => a.id === game.platform.id) === -1) {
            acc.push({
              id: game.platform.id,
              name: game.platform.name
            });
          }

          return acc;
        }, []);

        return platformList.sort(sortByProp('name'));
      }
    },
    {
      name: 'developers',
      label: () => translate('Developers'),
      type: filterBuilderTypes.ARRAY,
      optionsSelector: function(items) {
        const developerList = items.reduce((acc, { developers = [] }) => {
          developers.forEach((developer) => {
            if (acc.findIndex((a) => a.id === developer) === -1) {
              acc.push({
                id: developer,
                name: developer
              });
            }
          });

          return acc;
        }, []);

        return developerList.sort(sortByProp('name'));
      }
    },
    {
      name: 'collection',
      label: () => translate('Collection'),
      type: filterBuilderTypes.ARRAY,
      optionsSelector: function(items) {
        const collectionList = items.reduce((acc, game) => {
          if (game.collection && game.collection.title) {
            acc.push({
              id: game.collection.title,
              name: game.collection.title
            });
          }

          return acc;
        }, []);

        return collectionList.sort(sortByProp('name'));
      }
    },
    {
      name: 'qualityProfileId',
      label: () => translate('QualityProfile'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.QUALITY_PROFILE
    },
    {
      name: 'added',
      label: () => translate('Added'),
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'year',
      label: () => translate('Year'),
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'releaseDate',
      label: () => translate('ReleaseDate'),
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'path',
      label: () => translate('Path'),
      type: filterBuilderTypes.STRING
    },
    {
      name: 'sizeOnDisk',
      label: () => translate('SizeOnDisk'),
      type: filterBuilderTypes.NUMBER,
      valueType: filterBuilderValueTypes.BYTES
    },
    {
      name: 'genres',
      label: () => translate('Genres'),
      type: filterBuilderTypes.ARRAY,
      optionsSelector: function(items) {
        const genreList = items.reduce((acc, { genres = [] }) => {
          genres.forEach((genre) => {
            acc.push({
              id: genre,
              name: genre
            });
          });

          return acc;
        }, []);

        return genreList.sort(sortByProp('name'));
      }
    },
    {
      name: 'keywords',
      label: () => translate('Keywords'),
      type: filterBuilderTypes.ARRAY,
      optionsSelector: function(items) {
        const keywordList = items.reduce((acc, { keywords = [] }) => {
          keywords.forEach((keyword) => {
            if (acc.findIndex((a) => a.id === keyword) === -1) {
              acc.push({
                id: keyword,
                name: keyword
              });
            }
          });

          return acc;
        }, []);

        return keywordList.sort(sortByProp('name'));
      }
    },
    {
      name: 'igdbRating',
      label: () => translate('IgdbRating'),
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'igdbVotes',
      label: () => translate('IgdbVotes'),
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'criticRating',
      label: () => translate('CriticRating'),
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'popularity',
      label: () => translate('Popularity'),
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'tags',
      label: () => translate('Tags'),
      type: filterBuilderTypes.ARRAY,
      valueType: filterBuilderValueTypes.TAG
    }
  ]
};

export const persistState = [
  'gameIndex.sortKey',
  'gameIndex.sortDirection',
  'gameIndex.selectedFilterKey',
  'gameIndex.customFilters',
  'gameIndex.view',
  'gameIndex.columns',
  'gameIndex.posterOptions',
  'gameIndex.overviewOptions',
  'gameIndex.tableOptions'
];

//
// Actions Types

export const SET_GAME_SORT = 'gameIndex/setGameSort';
export const SET_GAME_FILTER = 'gameIndex/setGameFilter';
export const SET_GAME_VIEW = 'gameIndex/setGameView';
export const SET_GAME_TABLE_OPTION = 'gameIndex/setGameTableOption';
export const SET_GAME_POSTER_OPTION = 'gameIndex/setGamePosterOption';
export const SET_GAME_OVERVIEW_OPTION = 'gameIndex/setGameOverviewOption';

//
// Action Creators

export const setGameSort = createAction(SET_GAME_SORT);
export const setGameFilter = createAction(SET_GAME_FILTER);
export const setGameView = createAction(SET_GAME_VIEW);
export const setGameTableOption = createAction(SET_GAME_TABLE_OPTION);
export const setGamePosterOption = createAction(SET_GAME_POSTER_OPTION);
export const setGameOverviewOption = createAction(SET_GAME_OVERVIEW_OPTION);

//
// Reducers

export const reducers = createHandleActions({
  [SET_GAME_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_GAME_FILTER]: createSetClientSideCollectionFilterReducer(section),

  [SET_GAME_VIEW]: function(state, { payload }) {
    return Object.assign({}, state, { view: payload.view });
  },

  [SET_GAME_TABLE_OPTION]: createSetTableOptionReducer(section),

  [SET_GAME_POSTER_OPTION]: function(state, { payload }) {
    const posterOptions = state.posterOptions;

    return {
      ...state,
      posterOptions: {
        ...posterOptions,
        ...payload
      }
    };
  },

  [SET_GAME_OVERVIEW_OPTION]: function(state, { payload }) {
    const overviewOptions = state.overviewOptions;

    return {
      ...state,
      overviewOptions: {
        ...overviewOptions,
        ...payload
      }
    };
  }

}, defaultState, section);
