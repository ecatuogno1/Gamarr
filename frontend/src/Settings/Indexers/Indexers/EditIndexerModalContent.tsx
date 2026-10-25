import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { IndexerAppState } from 'App/State/SettingsAppState';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalSection from 'Components/ModalSection';
import usePrevious from 'Helpers/Hooks/usePrevious';
import useShowAdvancedSettings from 'Helpers/Hooks/useShowAdvancedSettings';
import { inputTypes, kinds } from 'Helpers/Props';
import AdvancedSettingsButton from 'Settings/AdvancedSettingsButton';
import {
  saveIndexer,
  setIndexerFieldValue,
  setIndexerValue,
  testIndexer,
} from 'Store/Actions/settingsActions';
import { createProviderSettingsSelectorHook } from 'Store/Selectors/createProviderSettingsSelector';
import Indexer from 'typings/Indexer';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './EditIndexerModalContent.css';

export interface EditIndexerModalContentProps {
  id?: number;
  onModalClose: () => void;
  onDeleteIndexerPress?: () => void;
}

function EditIndexerModalContent({
  id,
  onModalClose,
  onDeleteIndexerPress,
}: EditIndexerModalContentProps) {
  const dispatch = useDispatch();
  const showAdvancedSettings = useShowAdvancedSettings();

  const {
    isFetching,
    error,
    isSaving,
    isTesting = false,
    saveError,
    item,
    validationErrors,
    validationWarnings,
  } = useSelector(
    createProviderSettingsSelectorHook<Indexer, IndexerAppState>('indexers', id)
  );

  const wasSaving = usePrevious(isSaving);

  const {
    implementationName = '',
    name,
    enableRss,
    enableAutomaticSearch,
    enableInteractiveSearch,
    supportsRss,
    supportsSearch,
    tags,
    fields,
    priority,
    protocol,
    downloadClientId,
  } = item;

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setIndexerValue(change));
    },
    [dispatch]
  );

  const handleFieldChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setIndexerFieldValue(change));
    },
    [dispatch]
  );

  const handleSavePress = useCallback(() => {
    dispatch(saveIndexer({ id }));
  }, [id, dispatch]);

  const handleTestPress = useCallback(() => {
    dispatch(testIndexer({ id }));
  }, [id, dispatch]);

  useEffect(() => {
    if (!isSaving && wasSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader subtitle={id ? name.previousValue ?? name.value : undefined}>
        {id
          ? translate('EditIndexerImplementation', { implementationName })
          : translate('AddIndexerImplementation', { implementationName })}
      </ModalHeader>

      <ModalBody>
        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>{translate('AddIndexerError')}</Alert>
        ) : null}

        {!isFetching && !error ? (
          <Form
            validationErrors={validationErrors}
            validationWarnings={validationWarnings}
          >
            <FormRow>
              <FormLabel>{translate('Name')}</FormLabel>

              <FormInput
                type={inputTypes.TEXT}
                name="name"
                {...name}
                onChange={handleInputChange}
              />
            </FormRow>

            <ModalSection title={translate('Search')}>
              <FormRow>
                <FormLabel>{translate('EnableRss')}</FormLabel>

                {supportsRss.value ? (
                  <FormInputHelpText text={translate('EnableRssHelpText')} />
                ) : (
                  <FormInputHelpText
                    text={translate('RssIsNotSupportedWithThisIndexer')}
                    isWarning={true}
                  />
                )}

                <FormInput
                  type={inputTypes.CHECK}
                  name="enableRss"
                  isDisabled={!supportsRss.value}
                  {...enableRss}
                  onChange={handleInputChange}
                />
              </FormRow>

              <FormRow>
                <FormLabel>{translate('EnableAutomaticSearch')}</FormLabel>

                {supportsSearch.value ? (
                  <FormInputHelpText
                    text={translate('EnableAutomaticSearchHelpText')}
                  />
                ) : (
                  <FormInputHelpText
                    text={translate('SearchIsNotSupportedWithThisIndexer')}
                    isWarning={true}
                  />
                )}

                <FormInput
                  type={inputTypes.CHECK}
                  name="enableAutomaticSearch"
                  isDisabled={!supportsSearch.value}
                  {...enableAutomaticSearch}
                  onChange={handleInputChange}
                />
              </FormRow>

              <FormRow>
                <FormLabel>{translate('EnableInteractiveSearch')}</FormLabel>

                {supportsSearch.value ? (
                  <FormInputHelpText
                    text={translate('EnableInteractiveSearchHelpText')}
                  />
                ) : (
                  <FormInputHelpText
                    text={translate('SearchIsNotSupportedWithThisIndexer')}
                    isWarning={true}
                  />
                )}

                <FormInput
                  type={inputTypes.CHECK}
                  name="enableInteractiveSearch"
                  isDisabled={!supportsSearch.value}
                  {...enableInteractiveSearch}
                  onChange={handleInputChange}
                />
              </FormRow>
            </ModalSection>

            {fields?.map((field) => {
              return (
                <ProviderFieldFormGroup
                  key={field.name}
                  advancedSettings={showAdvancedSettings}
                  provider="indexer"
                  providerData={item}
                  {...field}
                  onChange={handleFieldChange}
                />
              );
            })}

            <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
              <FormLabel>{translate('IndexerPriority')}</FormLabel>

              <FormInputHelpText text={translate('IndexerPriorityHelpText')} />

              <FormInput
                type={inputTypes.NUMBER}
                name="priority"
                min={1}
                max={50}
                {...priority}
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
              <FormLabel>{translate('DownloadClient')}</FormLabel>

              <FormInputHelpText
                text={translate('IndexerDownloadClientHelpText')}
              />

              <FormInput
                type={inputTypes.DOWNLOAD_CLIENT_SELECT}
                name="downloadClientId"
                {...downloadClientId}
                includeAny={true}
                protocol={protocol.value}
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('Tags')}</FormLabel>

              <FormInputHelpText text={translate('IndexerTagGameHelpText')} />

              <FormInput
                type={inputTypes.TAG}
                name="tags"
                {...tags}
                onChange={handleInputChange}
              />
            </FormRow>
          </Form>
        ) : null}
      </ModalBody>

      <ModalFooter>
        {id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteIndexerPress}
          >
            {translate('Delete')}
          </Button>
        ) : null}

        <AdvancedSettingsButton showLabel={false} />

        <SpinnerErrorButton
          isSpinning={isTesting}
          error={saveError}
          onPress={handleTestPress}
        >
          {translate('Test')}
        </SpinnerErrorButton>

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditIndexerModalContent;
