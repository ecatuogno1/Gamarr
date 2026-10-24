import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import DeleteGameModal from 'Game/Delete/DeleteGameModal';
import EditGameModal from 'Game/Edit/EditGameModal';
import BulkDeleteGameModal from 'Game/Index/Select/Delete/DeleteGameModal';
import EditGamesModal from 'Game/Index/Select/Edit/EditGamesModal';
import { saveGameEditor } from 'Store/Actions/gameActions';

interface GameTitleModalsProps {
  gameIds: number[];
  isEditModalOpen: boolean;
  isDeleteModalOpen: boolean;
  onEditModalClose: () => void;
  onDeletePress: () => void;
  onDeleteModalClose: () => void;
}

// Edit and delete from the library index: the game's own modals for one platform, the bulk editor for several
function GameTitleModals({
  gameIds,
  isEditModalOpen,
  isDeleteModalOpen,
  onEditModalClose,
  onDeletePress,
  onDeleteModalClose,
}: GameTitleModalsProps) {
  const dispatch = useDispatch();

  const onSavePress = useCallback(
    (payload: object) => {
      onEditModalClose();
      dispatch(saveGameEditor({ ...payload, gameIds }));
    },
    [gameIds, dispatch, onEditModalClose]
  );

  if (gameIds.length === 1) {
    return (
      <>
        <EditGameModal
          isOpen={isEditModalOpen}
          gameId={gameIds[0]}
          onModalClose={onEditModalClose}
          onDeleteGamePress={onDeletePress}
        />

        <DeleteGameModal
          isOpen={isDeleteModalOpen}
          gameId={gameIds[0]}
          onModalClose={onDeleteModalClose}
        />
      </>
    );
  }

  return (
    <>
      <EditGamesModal
        isOpen={isEditModalOpen}
        gameIds={gameIds}
        onSavePress={onSavePress}
        onModalClose={onEditModalClose}
      />

      <BulkDeleteGameModal
        isOpen={isDeleteModalOpen}
        gameIds={gameIds}
        onModalClose={onDeleteModalClose}
      />
    </>
  );
}

export default GameTitleModals;
