import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import {
  cancelFetchReleases,
  clearReleases,
} from 'Store/Actions/releaseActions';
import GameInteractiveSearchModalContent, {
  GameInteractiveSearchModalContentProps,
} from './GameInteractiveSearchModalContent';

interface GameInteractiveSearchModalProps
  extends GameInteractiveSearchModalContentProps {
  isOpen: boolean;
}

function GameInteractiveSearchModal({
  isOpen,
  gameId,
  onModalClose,
}: GameInteractiveSearchModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    onModalClose();

    dispatch(cancelFetchReleases());
    dispatch(clearReleases());
  }, [dispatch, onModalClose]);

  return (
    <Modal
      isOpen={isOpen}
      closeOnBackgroundClick={false}
      size={sizes.EXTRA_EXTRA_LARGE}
      onModalClose={handleModalClose}
    >
      <GameInteractiveSearchModalContent
        gameId={gameId}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default GameInteractiveSearchModal;
