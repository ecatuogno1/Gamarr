import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { GAME_SEARCH } from 'Commands/commandNames';
import PageToolbarOverflowMenuItem from 'Components/Page/Toolbar/PageToolbarOverflowMenuItem';
import { selectGameIndexTitles } from 'Game/Title/createGameTitleCollectionSelector';
import useSelectedGameIds from 'Game/Title/useSelectedGameIds';
import { icons } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import translate from 'Utilities/String/translate';

interface GameIndexSearchMenuItemProps {
  isSelectMode: boolean;
  selectedFilterKey: string;
}

function GameIndexSearchMenuItem(props: GameIndexSearchMenuItemProps) {
  const isSearching = useSelector(createCommandExecutingSelector(GAME_SEARCH));
  const { items } = useSelector(selectGameIndexTitles);

  const dispatch = useDispatch();

  const { isSelectMode, selectedFilterKey } = props;
  const selectedGameIds = useSelectedGameIds();

  const gamesToSearch =
    isSelectMode && selectedGameIds.length > 0
      ? selectedGameIds
      : items.flatMap((m) => m.matchingGameIds);

  const searchIndexLabel =
    selectedFilterKey === 'all'
      ? translate('SearchAll')
      : translate('SearchFiltered');

  const searchSelectLabel =
    selectedGameIds.length > 0
      ? translate('SearchSelected')
      : translate('SearchAll');

  const onPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: GAME_SEARCH,
        gameIds: gamesToSearch,
      })
    );
  }, [dispatch, gamesToSearch]);

  return (
    <PageToolbarOverflowMenuItem
      label={isSelectMode ? searchSelectLabel : searchIndexLabel}
      isSpinning={isSearching}
      isDisabled={!items.length}
      iconName={icons.SEARCH}
      onPress={onPress}
    />
  );
}

export default GameIndexSearchMenuItem;
