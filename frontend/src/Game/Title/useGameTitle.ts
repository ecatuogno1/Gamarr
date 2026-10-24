import { useMemo } from 'react';
import { shallowEqual, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Game from 'Game/Game';
import createDeepEqualSelector from 'Store/Selectors/createDeepEqualSelector';
import { GameTitleSummary, getGameTitleSummary } from './gameTitle';

function createVersionsSelector(gameIds: number[]) {
  return createSelector(
    (state: AppState) => state.games.itemMap,
    (state: AppState) => state.games.items,
    (itemMap, allGames) =>
      gameIds
        .map((id) => allGames[itemMap[id]])
        .filter((game): game is Game => !!game)
  );
}

function createDownloadingSelector(gameIds: number[]) {
  return createDeepEqualSelector(
    createSelector(
      (state: AppState) => state.queue.details.items,
      (queueItems) =>
        queueItems
          .filter(
            (item) =>
              item.trackedDownloadState !== 'imported' &&
              !!item.gameId &&
              gameIds.includes(item.gameId)
          )
          .map((item) => item.gameId as number)
    ),
    (ids) => ids
  );
}

export function useGameTitleVersions(gameIds: number[]) {
  const key = gameIds.join();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const selector = useMemo(() => createVersionsSelector(gameIds), [key]);

  return useSelector(selector, shallowEqual);
}

function useGameTitle(gameIds: number[]): GameTitleSummary | undefined {
  const key = gameIds.join();
  const versions = useGameTitleVersions(gameIds);
  const downloadingSelector = useMemo(
    () => createDownloadingSelector(gameIds),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [key]
  );
  const downloadingGameIds: number[] = useSelector(downloadingSelector);

  return useMemo(
    () => getGameTitleSummary(versions, new Set(downloadingGameIds)),
    [versions, downloadingGameIds]
  );
}

export default useGameTitle;
