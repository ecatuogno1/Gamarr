import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useShowAdvancedSettings from 'Helpers/Hooks/useShowAdvancedSettings';
import { inputTypes, kinds } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  fetchIndexerOptions,
  saveIndexerOptions,
  setIndexerOptionsValue,
} from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import { InputChanged } from 'typings/inputs';
import {
  OnChildStateChange,
  SetChildSave,
} from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';

const SECTION = 'indexerOptions';

interface IndexerOptionsProps {
  setChildSave: SetChildSave;
  onChildStateChange: OnChildStateChange;
}

function IndexerOptions({
  setChildSave,
  onChildStateChange,
}: IndexerOptionsProps) {
  const dispatch = useDispatch();
  const {
    isFetching,
    isPopulated,
    isSaving,
    error,
    settings,
    hasSettings,
    hasPendingChanges,
  } = useSelector(createSettingsSectionSelector(SECTION));

  const showAdvancedSettings = useShowAdvancedSettings();

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions aren't typed
      dispatch(setIndexerOptionsValue(change));
    },
    [dispatch]
  );

  useEffect(() => {
    dispatch(fetchIndexerOptions());
    setChildSave(() => dispatch(saveIndexerOptions()));
  }, [dispatch, setChildSave]);

  useEffect(() => {
    onChildStateChange({
      isSaving,
      hasPendingChanges,
    });
  }, [hasPendingChanges, isSaving, onChildStateChange]);

  useEffect(() => {
    return () => {
      dispatch(clearPendingChanges({ section: `settings.${SECTION}` }));
    };
  }, [dispatch]);

  return (
    <FieldSet
      legend={translate('Options')}
      caption={translate('IndexerOptionsCaption')}
    >
      {isFetching ? <LoadingIndicator /> : null}
      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>
          {translate('IndexerOptionsLoadError')}
        </Alert>
      ) : null}

      {hasSettings && isPopulated && !error ? (
        <Form>
          <FormRow>
            <FormLabel>{translate('MinimumAge')}</FormLabel>
            <FormInputHelpText text={translate('MinimumAgeHelpText')} />
            <FormInput
              type={inputTypes.NUMBER}
              name="minimumAge"
              min={0}
              unit="minutes"
              onChange={handleInputChange}
              {...settings.minimumAge}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('Retention')}</FormLabel>
            <FormInputHelpText text={translate('RetentionHelpText')} />
            <FormInput
              type={inputTypes.NUMBER}
              name="retention"
              min={0}
              unit="days"
              onChange={handleInputChange}
              {...settings.retention}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('MaximumSize')}</FormLabel>
            <FormInputHelpText text={translate('MaximumSizeHelpText')} />
            <FormInput
              type={inputTypes.NUMBER}
              name="maximumSize"
              min={0}
              unit="MB"
              onChange={handleInputChange}
              {...settings.maximumSize}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('PreferIndexerFlags')}</FormLabel>
            <FormInputHelpText
              text={translate('PreferIndexerFlagsHelpText')}
              link="https://github.com/ecatuogno1/Gamarr/wiki/settings#indexer-flags"
            />
            <FormInput
              type={inputTypes.CHECK}
              name="preferIndexerFlags"
              onChange={handleInputChange}
              {...settings.preferIndexerFlags}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('AvailabilityDelay')}</FormLabel>
            <FormInputHelpText text={translate('AvailabilityDelayHelpText')} />
            <FormInput
              type={inputTypes.NUMBER}
              name="availabilityDelay"
              unit="days"
              onChange={handleInputChange}
              {...settings.availabilityDelay}
            />
          </FormRow>

          <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('RssSyncInterval')}</FormLabel>
            <FormInputHelpText
              text={translate('RssSyncIntervalHelpText')}
              link="https://github.com/ecatuogno1/Gamarr/wiki/faq#how-does-gamarr-work"
            />
            <FormInputHelpText
              text={translate('RssSyncIntervalHelpTextWarning')}
              isWarning={true}
            />
            <FormInput
              type={inputTypes.NUMBER}
              name="rssSyncInterval"
              min={0}
              max={120}
              unit="minutes"
              onChange={handleInputChange}
              {...settings.rssSyncInterval}
            />
          </FormRow>
        </Form>
      ) : null}
    </FieldSet>
  );
}

export default IndexerOptions;
