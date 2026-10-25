import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import GameMinimumAvailabilityPopoverContent from 'AddGame/GameMinimumAvailabilityPopoverContent';
import { ImportListAppState } from 'App/State/SettingsAppState';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalSection from 'Components/ModalSection';
import Popover from 'Components/Tooltip/Popover';
import usePrevious from 'Helpers/Hooks/usePrevious';
import useShowAdvancedSettings from 'Helpers/Hooks/useShowAdvancedSettings';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import AdvancedSettingsButton from 'Settings/AdvancedSettingsButton';
import {
  saveImportList,
  setImportListFieldValue,
  setImportListValue,
  testImportList,
} from 'Store/Actions/settingsActions';
import { createProviderSettingsSelectorHook } from 'Store/Selectors/createProviderSettingsSelector';
import ImportList from 'typings/ImportList';
import { InputChanged } from 'typings/inputs';
import formatShortTimeSpan from 'Utilities/Date/formatShortTimeSpan';
import translate from 'Utilities/String/translate';
import styles from './EditImportListModalContent.css';

export interface EditImportListModalContentProps {
  id?: number;
  onModalClose: () => void;
  onDeleteImportListPress?: () => void;
}

function EditImportListModalContent({
  id,
  onModalClose,
  onDeleteImportListPress,
}: EditImportListModalContentProps) {
  const dispatch = useDispatch();
  const showAdvancedSettings = useShowAdvancedSettings();

  const {
    isFetching,
    isSaving,
    isTesting = false,
    error,
    saveError,
    item,
    validationErrors,
    validationWarnings,
  } = useSelector(
    createProviderSettingsSelectorHook<ImportList, ImportListAppState>(
      'importLists',
      id
    )
  );

  const wasSaving = usePrevious(isSaving);

  const {
    implementationName,
    name,
    enabled,
    enableAuto,
    minRefreshInterval,
    monitor,
    minimumAvailability,
    rootFolderPath,
    qualityProfileId,
    platformId,
    searchOnAdd,
    tags,
    fields,
  } = item;

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setImportListValue(change));
    },
    [dispatch]
  );

  const handleFieldChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setImportListFieldValue(change));
    },
    [dispatch]
  );

  const handleTestPress = useCallback(() => {
    dispatch(testImportList({ id }));
  }, [id, dispatch]);

  const handleSavePress = useCallback(() => {
    dispatch(saveImportList({ id }));
  }, [id, dispatch]);

  useEffect(() => {
    if (wasSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader subtitle={id ? name.previousValue ?? name.value : undefined}>
        {id
          ? translate('EditImportListImplementation', { implementationName })
          : translate('AddImportListImplementation', { implementationName })}
      </ModalHeader>

      <ModalBody>
        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <Alert kind={kinds.DANGER}>{translate('AddListError')}</Alert>
        ) : null}

        {!isFetching && !error ? (
          <Form
            validationErrors={validationErrors}
            validationWarnings={validationWarnings}
          >
            <Alert kind={kinds.INFO} className={styles.message}>
              {translate('ListWillRefreshEveryInterval', {
                refreshInterval: formatShortTimeSpan(minRefreshInterval.value),
              })}
            </Alert>

            <FormRow>
              <FormLabel>{translate('Name')}</FormLabel>

              <FormInput
                type={inputTypes.TEXT}
                name="name"
                {...name}
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('Enable')}</FormLabel>
              <FormInputHelpText text={translate('ListEnabledHelpText')} />
              <FormInput
                type={inputTypes.CHECK}
                name="enabled"
                {...enabled}
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('EnableAutomaticAdd')}</FormLabel>
              <FormInputHelpText
                text={translate('EnableAutomaticAddGameHelpText')}
              />
              <FormInput
                type={inputTypes.CHECK}
                name="enableAuto"
                {...enableAuto}
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('SearchOnAdd')}</FormLabel>
              <FormInputHelpText
                text={translate('ListSearchOnAddGameHelpText')}
              />
              <FormInput
                type={inputTypes.CHECK}
                name="searchOnAdd"
                {...searchOnAdd}
                onChange={handleInputChange}
              />
            </FormRow>

            <ModalSection title={translate('Game')}>
              <FormRow>
                <FormLabel>{translate('Monitor')}</FormLabel>
                <FormInputHelpText
                  text={translate('ListMonitorGameHelpText')}
                />
                <FormInput
                  type={inputTypes.MONITOR_GAMES_SELECT}
                  name="monitor"
                  {...monitor}
                  onChange={handleInputChange}
                />
              </FormRow>

              <FormRow>
                <FormLabel>
                  {translate('MinimumAvailability')}

                  <Popover
                    anchor={
                      <Icon className={styles.labelIcon} name={icons.INFO} />
                    }
                    title={translate('MinimumAvailability')}
                    body={<GameMinimumAvailabilityPopoverContent />}
                    position={tooltipPositions.RIGHT}
                  />
                </FormLabel>
                <FormInputHelpText
                  text={translate('MoreInfo')}
                  link="https://github.com/ecatuogno1/Gamarr/wiki/faq#what-is-minimum-availability"
                />
                <FormInput
                  type={inputTypes.AVAILABILITY_SELECT}
                  name="minimumAvailability"
                  {...minimumAvailability}
                  onChange={handleInputChange}
                />
              </FormRow>

              <FormRow>
                <FormLabel>{translate('RootFolder')}</FormLabel>
                <FormInputHelpText text={translate('ListRootFolderHelpText')} />
                <FormInput
                  type={inputTypes.ROOT_FOLDER_SELECT}
                  name="rootFolderPath"
                  {...rootFolderPath}
                  includeMissingValue={true}
                  onChange={handleInputChange}
                />
              </FormRow>

              <FormRow>
                <FormLabel>{translate('Platform')}</FormLabel>
                <FormInputHelpText text={translate('ListPlatformHelpText')} />
                <FormInput
                  type={inputTypes.PLATFORM_SELECT}
                  name="platformId"
                  includeAny={true}
                  {...platformId}
                  onChange={handleInputChange}
                />
              </FormRow>

              <FormRow>
                <FormLabel>{translate('QualityProfile')}</FormLabel>
                <FormInputHelpText
                  text={translate('ListQualityProfileHelpText')}
                />
                <FormInput
                  type={inputTypes.QUALITY_PROFILE_SELECT}
                  name="qualityProfileId"
                  {...qualityProfileId}
                  onChange={handleInputChange}
                />
              </FormRow>
            </ModalSection>

            {fields?.map((field) => {
              return (
                <ProviderFieldFormGroup
                  key={field.name}
                  {...field}
                  advancedSettings={showAdvancedSettings}
                  provider="importList"
                  providerData={item}
                  onChange={handleFieldChange}
                />
              );
            })}

            <FormRow>
              <FormLabel>{translate('GamarrTags')}</FormLabel>
              <FormInputHelpText text={translate('ListTagsHelpText')} />
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
            onPress={onDeleteImportListPress}
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

export default EditImportListModalContent;
