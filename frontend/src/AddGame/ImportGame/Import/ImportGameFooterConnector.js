import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { cancelLookupGame, lookupUnsearchedGames } from 'Store/Actions/importGameActions';
import createAllGamesSelector from 'Store/Selectors/createAllGamesSelector';
import ImportGameFooter from './ImportGameFooter';

function isMixed(items, selectedIds, defaultValue, key) {
  return _.some(items, (game) => {
    return selectedIds.indexOf(game.id) > -1 && game[key] !== defaultValue;
  });
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.addGame,
    (state) => state.importGame,
    (state, { selectedIds }) => selectedIds,
    createAllGamesSelector(),
    (addGame, importGame, selectedIds, allGames) => {
      const {
        monitor: defaultMonitor,
        qualityProfileId: defaultQualityProfileId,
        minimumAvailability: defaultMinimumAvailability
      } = addGame.defaults;

      const {
        isLookingUpGame,
        isImporting,
        items,
        importError
      } = importGame;

      const isMonitorMixed = isMixed(items, selectedIds, defaultMonitor, 'monitor');
      const isQualityProfileIdMixed = isMixed(items, selectedIds, defaultQualityProfileId, 'qualityProfileId');
      const isMinimumAvailabilityMixed = isMixed(items, selectedIds, defaultMinimumAvailability, 'minimumAvailability');
      const unsearchedCount = items.filter((item) => !item.isPopulated).length;
      const hasUnsearchedItems = !isLookingUpGame && unsearchedCount > 0;
      const existingIgdbIds = new Set(allGames.map((game) => game.igdbId));

      let readyCount = 0;
      let existingCount = 0;

      items.forEach((item) => {
        if (!item.selectedGame) {
          return;
        }

        if (existingIgdbIds.has(item.selectedGame.igdbId)) {
          existingCount++;
        } else {
          readyCount++;
        }
      });

      return {
        selectedCount: selectedIds.length,
        totalCount: items.length,
        readyCount,
        existingCount,
        unsearchedCount,
        isLookingUpGame,
        isImporting,
        defaultMonitor,
        defaultQualityProfileId,
        defaultMinimumAvailability,
        isMonitorMixed,
        isQualityProfileIdMixed,
        isMinimumAvailabilityMixed,
        importError,
        hasUnsearchedItems
      };
    }
  );
}

const mapDispatchToProps = {
  onLookupPress: lookupUnsearchedGames,
  onCancelLookupPress: cancelLookupGame
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportGameFooter);
