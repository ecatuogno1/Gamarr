import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { REFRESH_GAME } from 'Commands/commandNames';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { selectGameIndexTitles } from 'Game/Title/createGameTitleCollectionSelector';
import useSelectedGameIds from 'Game/Title/useSelectedGameIds';
import { icons } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import translate from 'Utilities/String/translate';

interface GameIndexRefreshGameButtonProps {
  isSelectMode: boolean;
  selectedFilterKey: string;
}

function GameIndexRefreshGameButton(props: GameIndexRefreshGameButtonProps) {
  const isRefreshing = useSelector(
    createCommandExecutingSelector(REFRESH_GAME)
  );
  const { items, totalItems } = useSelector(selectGameIndexTitles);

  const dispatch = useDispatch();
  const { isSelectMode, selectedFilterKey } = props;
  const selectedGameIds = useSelectedGameIds();

  const gamesToRefresh =
    isSelectMode && selectedGameIds.length > 0
      ? selectedGameIds
      : items.flatMap((m) => m.matchingGameIds);

  const refreshIndexLabel =
    selectedFilterKey === 'all'
      ? translate('UpdateAll')
      : translate('UpdateFiltered');

  const refreshSelectLabel =
    selectedGameIds.length > 0
      ? translate('UpdateSelected')
      : translate('UpdateAll');

  const onPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: REFRESH_GAME,
        gameIds: gamesToRefresh,
      })
    );
  }, [dispatch, gamesToRefresh]);

  return (
    <PageToolbarButton
      label={isSelectMode ? refreshSelectLabel : refreshIndexLabel}
      isSpinning={isRefreshing}
      isDisabled={!totalItems}
      iconName={icons.REFRESH}
      onPress={onPress}
    />
  );
}

export default GameIndexRefreshGameButton;
