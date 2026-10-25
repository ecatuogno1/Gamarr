import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  fetchNamingExamples,
  fetchNamingSettings,
  setNamingSettingsValue,
} from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import { InputChanged } from 'typings/inputs';
import NamingConfig from 'typings/Settings/NamingConfig';
import translate from 'Utilities/String/translate';
import NamingModal from './NamingModal';
import styles from './Naming.css';

const SECTION = 'naming';

function createNamingSelector() {
  return createSelector(
    (state: AppState) => state.settings.advancedSettings,
    (state: AppState) => state.settings.namingExamples,
    createSettingsSectionSelector(SECTION),
    (advancedSettings, namingExamples, sectionSettings) => {
      return {
        advancedSettings,
        examples: namingExamples.item,
        examplesPopulated: namingExamples.isPopulated,
        ...sectionSettings,
      };
    }
  );
}

interface NamingModalOptions {
  name: keyof Pick<NamingConfig, 'standardGameFormat' | 'gameFolderFormat'>;
  game?: boolean;
  additional?: boolean;
}

function Naming() {
  const {
    advancedSettings,
    isFetching,
    error,
    settings,
    hasSettings,
    examples,
    examplesPopulated,
  } = useSelector(createNamingSelector());

  const dispatch = useDispatch();

  const [isNamingModalOpen, setNamingModalOpen, setNamingModalClosed] =
    useModalOpenState(false);
  const [namingModalOptions, setNamingModalOptions] =
    useState<NamingModalOptions | null>(null);
  const namingExampleTimeout = useRef<ReturnType<typeof setTimeout>>();

  useEffect(() => {
    dispatch(fetchNamingSettings());
    dispatch(fetchNamingExamples());

    return () => {
      dispatch(clearPendingChanges({ section: 'settings.naming' }));
    };
  }, [dispatch]);

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error 'setNamingSettingsValue' isn't typed yet
      dispatch(setNamingSettingsValue(change));

      if (namingExampleTimeout.current) {
        clearTimeout(namingExampleTimeout.current);
      }

      namingExampleTimeout.current = setTimeout(() => {
        dispatch(fetchNamingExamples());
      }, 1000);
    },
    [dispatch]
  );

  const onStandardNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'standardGameFormat',
      game: true,
      additional: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const onGameFolderNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'gameFolderFormat',
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const renameGames = hasSettings && settings.renameGames.value;
  const replaceIllegalCharacters =
    hasSettings && settings.replaceIllegalCharacters.value;

  const colonReplacementOptions = [
    { key: 'delete', value: translate('Delete') },
    { key: 'dash', value: translate('ReplaceWithDash') },
    { key: 'spaceDash', value: translate('ReplaceWithSpaceDash') },
    { key: 'spaceDashSpace', value: translate('ReplaceWithSpaceDashSpace') },
    {
      key: 'smart',
      value: translate('SmartReplace'),
      hint: translate('SmartReplaceHint'),
    },
  ];

  const standardGameFormatHelpTexts = [];
  const standardGameFormatErrors = [];
  const gameFolderFormatHelpTexts = [];
  const gameFolderFormatErrors = [];

  if (examplesPopulated) {
    if (examples.gameExample) {
      standardGameFormatHelpTexts.push(
        `${translate('Game')}: ${examples.gameExample}`
      );
    } else {
      standardGameFormatErrors.push({
        message: translate('GameInvalidFormat'),
      });
    }

    if (examples.gameFolderExample) {
      gameFolderFormatHelpTexts.push(
        `${translate('Example')}: ${examples.gameFolderExample}`
      );
    } else {
      gameFolderFormatErrors.push({ message: translate('InvalidFormat') });
    }
  }

  return (
    <FieldSet
      legend={translate('GameNaming')}
      caption={translate('GameNamingCaption')}
    >
      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>
          {translate('NamingSettingsLoadError')}
        </Alert>
      ) : null}

      {hasSettings && !isFetching && !error ? (
        <Form>
          <FormRow size={sizes.MEDIUM}>
            <FormLabel>{translate('RenameGames')}</FormLabel>

            <FormInputHelpText text={translate('RenameGamesHelpText')} />

            <FormInput
              type={inputTypes.CHECK}
              name="renameGames"
              onChange={handleInputChange}
              {...settings.renameGames}
            />
          </FormRow>

          <FormRow size={sizes.MEDIUM}>
            <FormLabel>{translate('ReplaceIllegalCharacters')}</FormLabel>

            <FormInputHelpText
              text={translate('ReplaceIllegalCharactersHelpText')}
            />

            <FormInput
              type={inputTypes.CHECK}
              name="replaceIllegalCharacters"
              onChange={handleInputChange}
              {...settings.replaceIllegalCharacters}
            />
          </FormRow>

          {replaceIllegalCharacters ? (
            <FormRow size={sizes.MEDIUM}>
              <FormLabel>{translate('ColonReplacement')}</FormLabel>

              <FormInputHelpText
                text={translate('ColonReplacementFormatHelpText')}
              />

              <FormInput
                type={inputTypes.SELECT}
                name="colonReplacementFormat"
                values={colonReplacementOptions}
                onChange={handleInputChange}
                {...settings.colonReplacementFormat}
              />
            </FormRow>
          ) : null}

          {renameGames ? (
            <FormRow size={sizes.LARGE}>
              <FormLabel>{translate('StandardGameFormat')}</FormLabel>

              {standardGameFormatHelpTexts.map((text, index) => (
                <FormInputHelpText key={index} text={text} />
              ))}

              <FormInput
                inputClassName={styles.namingInput}
                type={inputTypes.TEXT}
                name="standardGameFormat"
                buttons={
                  <FormInputButton onPress={onStandardNamingModalOpenClick}>
                    ?
                  </FormInputButton>
                }
                onChange={handleInputChange}
                {...settings.standardGameFormat}
                errors={[
                  ...standardGameFormatErrors,
                  ...settings.standardGameFormat.errors,
                ]}
              />
            </FormRow>
          ) : null}

          <FormRow
            advancedSettings={advancedSettings}
            isAdvanced={true}
            size={sizes.MEDIUM}
          >
            <FormLabel>{translate('GameFolderFormat')}</FormLabel>

            <FormInputHelpText text={translate('GameFolderFormatHelpText')} />

            {gameFolderFormatHelpTexts.map((text, index) => (
              <FormInputHelpText key={index} text={text} />
            ))}

            <FormInputHelpText
              text={translate('GameFolderFormatHelpTextDeprecatedWarning')}
              isWarning={true}
            />

            <FormInput
              inputClassName={styles.namingInput}
              type={inputTypes.TEXT}
              name="gameFolderFormat"
              buttons={
                <FormInputButton onPress={onGameFolderNamingModalOpenClick}>
                  ?
                </FormInputButton>
              }
              onChange={handleInputChange}
              {...settings.gameFolderFormat}
              errors={[
                ...gameFolderFormatErrors,
                ...settings.gameFolderFormat.errors,
              ]}
            />
          </FormRow>

          {namingModalOptions ? (
            <NamingModal
              isOpen={isNamingModalOpen}
              {...namingModalOptions}
              value={settings[namingModalOptions.name].value}
              onInputChange={handleInputChange}
              onModalClose={setNamingModalClosed}
            />
          ) : null}
        </Form>
      ) : null}
    </FieldSet>
  );
}

export default Naming;
