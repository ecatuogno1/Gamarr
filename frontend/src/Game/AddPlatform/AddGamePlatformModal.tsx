import React from 'react';
import Modal from 'Components/Modal/Modal';
import AddGamePlatformModalContent, {
  AddGamePlatformModalContentProps,
} from './AddGamePlatformModalContent';

interface AddGamePlatformModalProps extends AddGamePlatformModalContentProps {
  isOpen: boolean;
}

function AddGamePlatformModal({
  isOpen,
  onModalClose,
  ...otherProps
}: AddGamePlatformModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <AddGamePlatformModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default AddGamePlatformModal;
