import { useCallback, useMemo, useState } from 'react';
import { shallowEqual, useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { GAME_SEARCH, REFRESH_GAME } from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import { isCommandExecuting } from 'Utilities/Command';
import useGameTitle from './useGameTitle';

function createTitleCommandsSelector(gameIds: number[]) {
  return createSelector(
    (state: AppState) => state.commands.items,
    (commands) => {
      const executing = commands.filter((command) =>
        isCommandExecuting(command)
      );

      const isExecuting = (name: string) =>
        executing.some(
          (command) =>
            command.name === name &&
            (command.body.gameIds as number[] | undefined)?.some((id) =>
              gameIds.includes(id)
            )
        );

      return {
        isRefreshing: isExecuting(REFRESH_GAME),
        isSearching: isExecuting(GAME_SEARCH),
      };
    }
  );
}

function createQualityProfileNamesSelector(profileIds: number[]) {
  return createSelector(
    (state: AppState) => state.settings.qualityProfiles.items,
    (qualityProfiles) =>
      profileIds
        .map((id) => qualityProfiles.find((p) => p.id === id)?.name)
        .filter((name): name is string => !!name)
  );
}

// State and actions of a title (all its platform versions) in the library index
function useGameTitleIndexItem(gameIds: number[]) {
  const dispatch = useDispatch();
  const summary = useGameTitle(gameIds);
  const key = gameIds.join();

  const commandsSelector = useMemo(
    () => createTitleCommandsSelector(gameIds),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [key]
  );
  const { isRefreshing, isSearching } = useSelector(
    commandsSelector,
    shallowEqual
  );

  const profileIds = useMemo(
    () => [
      ...new Set((summary?.versions ?? []).map((v) => v.game.qualityProfileId)),
    ],
    [summary]
  );
  const profileKey = profileIds.join();
  const profileNamesSelector = useMemo(
    () => createQualityProfileNamesSelector(profileIds),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [profileKey]
  );
  const qualityProfileNames = useSelector(profileNamesSelector, shallowEqual);

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);

  const onRefreshPress = useCallback(() => {
    dispatch(executeCommand({ name: REFRESH_GAME, gameIds }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [key, dispatch]);

  const onSearchPress = useCallback(() => {
    // Search the monitored versions, or all of them when none is monitored
    const versions = summary?.versions ?? [];
    const monitored = versions.filter((v) => v.game.monitored);

    dispatch(
      executeCommand({
        name: GAME_SEARCH,
        gameIds: (monitored.length ? monitored : versions).map(
          (v) => v.game.id
        ),
      })
    );
  }, [summary, dispatch]);

  const onEditPress = useCallback(() => setIsEditModalOpen(true), []);
  const onEditModalClose = useCallback(() => setIsEditModalOpen(false), []);
  const onDeletePress = useCallback(() => {
    setIsEditModalOpen(false);
    setIsDeleteModalOpen(true);
  }, []);
  const onDeleteModalClose = useCallback(() => setIsDeleteModalOpen(false), []);

  return {
    summary,
    qualityProfileNames,
    isRefreshing,
    isSearching,
    isEditModalOpen,
    isDeleteModalOpen,
    onRefreshPress,
    onSearchPress,
    onEditPress,
    onEditModalClose,
    onDeletePress,
    onDeleteModalClose,
  };
}

export default useGameTitleIndexItem;
