import React, { useCallback, useState } from 'react';
import FormInput from 'Components/Form/FormInput';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalSection from 'Components/ModalSection';
import { inputTypes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './ManageIndexersEditModalContent.css';

interface SavePayload {
  enableRss?: boolean;
  enableAutomaticSearch?: boolean;
  enableInteractiveSearch?: boolean;
  priority?: number;
}

interface ManageIndexersEditModalContentProps {
  indexerIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

const NO_CHANGE = 'noChange';

const enableOptions = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    isDisabled: true,
  },
  {
    key: 'enabled',
    get value() {
      return translate('Enabled');
    },
  },
  {
    key: 'disabled',
    get value() {
      return translate('Disabled');
    },
  },
];

function ManageIndexersEditModalContent(
  props: ManageIndexersEditModalContentProps
) {
  const { indexerIds, onSavePress, onModalClose } = props;

  const [enableRss, setEnableRss] = useState(NO_CHANGE);
  const [enableAutomaticSearch, setEnableAutomaticSearch] = useState(NO_CHANGE);
  const [enableInteractiveSearch, setEnableInteractiveSearch] =
    useState(NO_CHANGE);
  const [priority, setPriority] = useState<null | number>(null);

  const save = useCallback(() => {
    let hasChanges = false;
    const payload: SavePayload = {};

    if (enableRss !== NO_CHANGE) {
      hasChanges = true;
      payload.enableRss = enableRss === 'enabled';
    }

    if (enableAutomaticSearch !== NO_CHANGE) {
      hasChanges = true;
      payload.enableAutomaticSearch = enableAutomaticSearch === 'enabled';
    }

    if (enableInteractiveSearch !== NO_CHANGE) {
      hasChanges = true;
      payload.enableInteractiveSearch = enableInteractiveSearch === 'enabled';
    }

    if (priority !== null) {
      hasChanges = true;
      payload.priority = priority as number;
    }

    if (hasChanges) {
      onSavePress(payload);
    }

    onModalClose();
  }, [
    enableRss,
    enableAutomaticSearch,
    enableInteractiveSearch,
    priority,
    onSavePress,
    onModalClose,
  ]);

  const onInputChange = useCallback(({ name, value }: InputChanged) => {
    switch (name) {
      case 'enableRss':
        setEnableRss(value as string);
        break;
      case 'enableAutomaticSearch':
        setEnableAutomaticSearch(value as string);
        break;
      case 'enableInteractiveSearch':
        setEnableInteractiveSearch(value as string);
        break;
      case 'priority':
        setPriority(value as number);
        break;
      default:
        console.warn(`EditIndexersModalContent Unknown Input: '${name}'`);
    }
  }, []);

  const selectedCount = indexerIds.length;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EditSelectedIndexers')}</ModalHeader>
      <ModalBody>
        <ModalSection
          title={translate('ManageIndexersEditModalContentSearchSection')}
        >
          <FormRow>
            <FormLabel>{translate('EnableRss')}</FormLabel>

            <FormInput
              type={inputTypes.SELECT}
              name="enableRss"
              value={enableRss}
              values={enableOptions}
              onChange={onInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('EnableAutomaticSearch')}</FormLabel>

            <FormInput
              type={inputTypes.SELECT}
              name="enableAutomaticSearch"
              value={enableAutomaticSearch}
              values={enableOptions}
              onChange={onInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('EnableInteractiveSearch')}</FormLabel>

            <FormInput
              type={inputTypes.SELECT}
              name="enableInteractiveSearch"
              value={enableInteractiveSearch}
              values={enableOptions}
              onChange={onInputChange}
            />
          </FormRow>
        </ModalSection>

        <FormRow>
          <FormLabel>{translate('Priority')}</FormLabel>

          <FormInput
            type={inputTypes.NUMBER}
            name="priority"
            value={priority}
            min={1}
            max={50}
            onChange={onInputChange}
          />
        </FormRow>
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div className={styles.selected}>
          {translate('CountIndexersSelected', { count: selectedCount })}
        </div>

        <div className={styles.buttons}>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <Button onPress={save}>{translate('ApplyChanges')}</Button>
        </div>
      </ModalFooter>
    </ModalContent>
  );
}

export default ManageIndexersEditModalContent;
