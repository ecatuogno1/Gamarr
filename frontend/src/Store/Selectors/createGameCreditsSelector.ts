import _ from 'lodash';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { GameCreditType } from 'typings/GameCredit';

function createGameCreditsSelector(gameCreditTypes: GameCreditType[]) {
  return createSelector(
    (state: AppState) => state.gameCredits.items,
    (gameCredits) => {
      const credits = gameCredits.filter(({ type }) =>
        gameCreditTypes.includes(type)
      );

      const sortedCredits = credits.sort((a, b) => a.order - b.order);

      return {
        items: _.uniqBy(
          sortedCredits,
          (credit) => `${credit.companyIgdbId}-${credit.type}`
        ),
      };
    }
  );
}

export default createGameCreditsSelector;
