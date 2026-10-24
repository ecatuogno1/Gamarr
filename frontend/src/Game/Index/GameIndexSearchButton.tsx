import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { GAME_SEARCH } from 'Commands/commandNames';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { selectGameIndexTitles } from 'Game/Title/createGameTitleCollectionSelector';
import useSelectedGameIds from 'Game/Title/useSelectedGameIds';
import { icons, kinds } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import translate from 'Utilities/String/translate';

interface GameIndexSearchButtonProps {
  isSelectMode: boolean;
  selectedFilterKey: string;
  overflowComponent: React.FunctionComponent<never>;
}

function GameIndexSearchButton(props: GameIndexSearchButtonProps) {
  const isSearching = useSelector(createCommandExecutingSelector(GAME_SEARCH));
  const { items } = useSelector(selectGameIndexTitles);

  const dispatch = useDispatch();
  const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);

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
    setIsConfirmModalOpen(false);

    dispatch(
      executeCommand({
        name: GAME_SEARCH,
        gameIds: gamesToSearch,
      })
    );
  }, [dispatch, gamesToSearch]);

  const onConfirmPress = useCallback(() => {
    setIsConfirmModalOpen(true);
  }, [setIsConfirmModalOpen]);

  const onConfirmModalClose = useCallback(() => {
    setIsConfirmModalOpen(false);
  }, [setIsConfirmModalOpen]);

  return (
    <>
      <PageToolbarButton
        label={isSelectMode ? searchSelectLabel : searchIndexLabel}
        isSpinning={isSearching}
        isDisabled={!items.length}
        iconName={icons.SEARCH}
        onPress={gamesToSearch.length > 5 ? onConfirmPress : onPress}
      />

      <ConfirmModal
        isOpen={isConfirmModalOpen}
        kind={kinds.DANGER}
        title={isSelectMode ? searchSelectLabel : searchIndexLabel}
        message={translate('SearchGamesConfirmationMessageText', {
          count: gamesToSearch.length,
        })}
        confirmLabel={isSelectMode ? searchSelectLabel : searchIndexLabel}
        onConfirm={onPress}
        onCancel={onConfirmModalClose}
      />
    </>
  );
}

export default GameIndexSearchButton;
