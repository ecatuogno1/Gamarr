import React, { useCallback, useMemo, useState } from 'react';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './RemoveQueueItemModal.css';

export interface RemovePressProps {
  remove: boolean;
  changeCategory: boolean;
  blocklist: boolean;
  skipRedownload: boolean;
}

interface RemoveQueueItemModalProps {
  isOpen: boolean;
  sourceTitle?: string;
  canChangeCategory: boolean;
  canIgnore: boolean;
  isPending: boolean;
  selectedCount?: number;
  onRemovePress(props: RemovePressProps): void;
  onModalClose: () => void;
}

type RemovalMethod = 'removeFromClient' | 'changeCategory' | 'ignore';
type BlocklistMethod =
  | 'doNotBlocklist'
  | 'blocklistAndSearch'
  | 'blocklistOnly';

function RemoveQueueItemModal(props: RemoveQueueItemModalProps) {
  const {
    isOpen,
    sourceTitle = '',
    canIgnore,
    canChangeCategory,
    isPending,
    selectedCount,
    onRemovePress,
    onModalClose,
  } = props;

  const multipleSelected = selectedCount && selectedCount > 1;

  const [removalMethod, setRemovalMethod] =
    useState<RemovalMethod>('removeFromClient');
  const [blocklistMethod, setBlocklistMethod] =
    useState<BlocklistMethod>('doNotBlocklist');

  const { title, message } = useMemo(() => {
    if (!selectedCount) {
      return {
        title: translate('RemoveQueueItem', { sourceTitle }),
        message: translate('RemoveQueueItemConfirmation', { sourceTitle }),
      };
    }

    if (selectedCount === 1) {
      return {
        title: translate('RemoveSelectedItem'),
        message: translate('RemoveSelectedItemQueueMessageText'),
      };
    }

    return {
      title: translate('RemoveSelectedItems'),
      message: translate('RemoveSelectedItemsQueueMessageText', {
        selectedCount,
      }),
    };
  }, [sourceTitle, selectedCount]);

  const removalMethodOptions = useMemo(() => {
    return [
      {
        key: 'removeFromClient',
        value: translate('RemoveFromDownloadClient'),
        hint: multipleSelected
          ? translate('RemoveMultipleFromDownloadClientHint')
          : translate('RemoveFromDownloadClientHint'),
      },
      {
        key: 'changeCategory',
        value: translate('ChangeCategory'),
        isDisabled: !canChangeCategory,
        hint: multipleSelected
          ? translate('ChangeCategoryMultipleHint')
          : translate('ChangeCategoryHint'),
      },
      {
        key: 'ignore',
        value: multipleSelected
          ? translate('IgnoreDownloads')
          : translate('IgnoreDownload'),
        isDisabled: !canIgnore,
        hint: multipleSelected
          ? translate('IgnoreDownloadsHint')
          : translate('IgnoreDownloadHint'),
      },
    ];
  }, [canChangeCategory, canIgnore, multipleSelected]);

  const blocklistMethodOptions = useMemo(() => {
    return [
      {
        key: 'doNotBlocklist',
        value: translate('DoNotBlocklist'),
        hint: translate('DoNotBlocklistHint'),
      },
      {
        key: 'blocklistAndSearch',
        value: translate('BlocklistAndSearch'),
        isDisabled: isPending,
        hint: multipleSelected
          ? translate('BlocklistAndSearchMultipleHint')
          : translate('BlocklistAndSearchHint'),
      },
      {
        key: 'blocklistOnly',
        value: translate('BlocklistOnly'),
        hint: multipleSelected
          ? translate('BlocklistMultipleOnlyHint')
          : translate('BlocklistOnlyHint'),
      },
    ];
  }, [isPending, multipleSelected]);

  const handleRemovalMethodChange = useCallback(
    ({ value }: { value: RemovalMethod }) => {
      setRemovalMethod(value);
    },
    [setRemovalMethod]
  );

  const handleBlocklistMethodChange = useCallback(
    ({ value }: { value: BlocklistMethod }) => {
      setBlocklistMethod(value);
    },
    [setBlocklistMethod]
  );

  const handleConfirmRemove = useCallback(() => {
    onRemovePress({
      remove: removalMethod === 'removeFromClient',
      changeCategory: removalMethod === 'changeCategory',
      blocklist: blocklistMethod !== 'doNotBlocklist',
      skipRedownload: blocklistMethod === 'blocklistOnly',
    });

    setRemovalMethod('removeFromClient');
    setBlocklistMethod('doNotBlocklist');
  }, [
    removalMethod,
    blocklistMethod,
    setRemovalMethod,
    setBlocklistMethod,
    onRemovePress,
  ]);

  const handleModalClose = useCallback(() => {
    setRemovalMethod('removeFromClient');
    setBlocklistMethod('doNotBlocklist');

    onModalClose();
  }, [setRemovalMethod, setBlocklistMethod, onModalClose]);

  return (
    <Modal isOpen={isOpen} size={sizes.MEDIUM} onModalClose={handleModalClose}>
      <ModalContent onModalClose={handleModalClose}>
        <ModalHeader>{title}</ModalHeader>

        <ModalBody>
          <p className={styles.intro}>{message}</p>

          {isPending ? null : (
            <FormRow>
              <FormLabel>{translate('RemoveQueueItemRemovalMethod')}</FormLabel>

              <FormInputHelpText
                text={translate('RemoveQueueItemRemovalMethodHelpTextWarning')}
                isWarning={true}
              />

              <FormInput
                type={inputTypes.SELECT}
                name="removalMethod"
                value={removalMethod}
                values={removalMethodOptions}
                isDisabled={!canChangeCategory && !canIgnore}
                onChange={handleRemovalMethodChange}
              />
            </FormRow>
          )}

          <FormRow>
            <FormLabel>
              {multipleSelected
                ? translate('BlocklistReleases')
                : translate('BlocklistRelease')}
            </FormLabel>

            <FormInputHelpText text={translate('BlocklistReleaseHelpText')} />

            <FormInput
              type={inputTypes.SELECT}
              name="blocklistMethod"
              value={blocklistMethod}
              values={blocklistMethodOptions}
              onChange={handleBlocklistMethodChange}
            />
          </FormRow>
        </ModalBody>

        <ModalFooter>
          <Button onPress={handleModalClose}>{translate('Close')}</Button>

          <Button kind={kinds.DANGER} onPress={handleConfirmRemove}>
            {translate('Remove')}
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

export default RemoveQueueItemModal;
