import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllGamesSelector from 'Store/Selectors/createAllGamesSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import AddNewGameSearchResult from './AddNewGameSearchResult';

function createMapStateToProps() {
  return createSelector(
    (state, { igdbId }) => igdbId,
    (state, { availablePlatforms }) => availablePlatforms,
    createAllGamesSelector(),
    createDimensionsSelector(),
    (state) => state.gameFiles.items,
    (state) => state.settings.platforms.items,
    (igdbId, availablePlatforms = [], allGames, dimensions, gameFiles, platforms) => {
      // A title can be in the library once per platform
      const libraryGames = allGames.filter((game) => game.igdbId === igdbId);
      const addedPlatformIds = libraryGames.map((game) => game.platformId);
      const existingGame = libraryGames.length ? libraryGames[0] : null;

      const isExistingGame = availablePlatforms.length ?
        availablePlatforms.every((platformId) => addedPlatformIds.includes(platformId)) :
        !!existingGame;

      const existingGameId = existingGame ? existingGame.id : 0;
      const gameFile = gameFiles.find((item) => existingGameId > 0 && item.gameId === existingGameId);

      // Platforms IGDB lists for the game (enabled in Gamarr), flagged when the game is in the library on them
      const platformBadges = availablePlatforms
        .map((platformId) => platforms.find((p) => p.id === platformId))
        .filter((p) => !!p)
        .map((p) => ({
          id: p.id,
          name: p.name,
          shortName: p.shortName,
          isAdded: addedPlatformIds.includes(p.id)
        }));

      const libraryPlatformNames = libraryGames
        .map((game) => (game.platform ? game.platform.name : null))
        .filter((name) => !!name);

      return {
        existingGameId,
        platformBadges,
        libraryPlatformNames,
        existingTitleSlug: existingGame ? existingGame.titleSlug : null,
        addedPlatformIds,
        isExistingGame,
        isSmallScreen: dimensions.isSmallScreen,
        gameFile
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewGameSearchResult);
