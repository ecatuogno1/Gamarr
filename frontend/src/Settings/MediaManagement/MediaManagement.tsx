import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import SectionHeading from 'Components/SectionHeading';
import useShowAdvancedSettings from 'Helpers/Hooks/useShowAdvancedSettings';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import RootFolders from 'RootFolder/RootFolders';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  fetchMediaManagementSettings,
  saveMediaManagementSettings,
  saveNamingSettings,
  setMediaManagementSettingsValue,
} from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import useIsWindows from 'System/useIsWindows';
import { InputChanged } from 'typings/inputs';
import isEmpty from 'Utilities/Object/isEmpty';
import translate from 'Utilities/String/translate';
import Naming from './Naming/Naming';
import AddRootFolder from './RootFolder/AddRootFolder';

const SECTION = 'mediaManagement';

const rescanAfterRefreshOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: 'always',
    get value() {
      return translate('Always');
    },
  },
  {
    key: 'afterManual',
    get value() {
      return translate('AfterManualRefresh');
    },
  },
  {
    key: 'never',
    get value() {
      return translate('Never');
    },
  },
];

const downloadPropersAndRepacksOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: 'preferAndUpgrade',
    get value() {
      return translate('PreferAndUpgrade');
    },
  },
  {
    key: 'doNotUpgrade',
    get value() {
      return translate('DoNotUpgradeAutomatically');
    },
  },
  {
    key: 'doNotPrefer',
    get value() {
      return translate('DoNotPrefer');
    },
  },
];

const fileDateOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: 'none',
    get value() {
      return translate('None');
    },
  },
  {
    key: 'release',
    get value() {
      return translate('ReleaseDate');
    },
  },
];

function MediaManagement() {
  const dispatch = useDispatch();
  const showAdvancedSettings = useShowAdvancedSettings();
  const hasNamingPendingChanges = !isEmpty(
    useSelector((state: AppState) => state.settings.naming.pendingChanges)
  );
  const isWindows = useIsWindows();
  const {
    isFetching,
    isPopulated,
    isSaving,
    error,
    settings,
    hasSettings,
    hasPendingChanges,
    validationErrors,
    validationWarnings,
  } = useSelector(createSettingsSectionSelector(SECTION));

  const handleSavePress = useCallback(() => {
    dispatch(saveMediaManagementSettings());
    dispatch(saveNamingSettings());
  }, [dispatch]);

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setMediaManagementSettingsValue(change));
    },
    [dispatch]
  );

  useEffect(() => {
    dispatch(fetchMediaManagementSettings());

    return () => {
      dispatch(clearPendingChanges({ section: `settings.${SECTION}` }));
    };
  }, [dispatch]);

  return (
    <SettingsPage
      title={translate('MediaManagementSettings')}
      isSaving={isSaving}
      hasPendingChanges={hasNamingPendingChanges || hasPendingChanges}
      onSavePress={handleSavePress}
    >
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading
            scope={translate('Settings')}
            title={translate('MediaManagement')}
          />

          <Naming />

          {isFetching ? (
            <FieldSet legend={translate('NamingSettings')}>
              <LoadingIndicator />
            </FieldSet>
          ) : null}

          {!isFetching && error ? (
            <FieldSet legend={translate('NamingSettings')}>
              <Alert kind={kinds.DANGER}>
                {translate('MediaManagementSettingsLoadError')}
              </Alert>
            </FieldSet>
          ) : null}

          {hasSettings && isPopulated && !error ? (
            <Form
              id="mediaManagementSettings"
              validationErrors={validationErrors}
              validationWarnings={validationWarnings}
            >
              {showAdvancedSettings ? (
                <FieldSet
                  legend={translate('Folders')}
                  caption={translate('FoldersCaption')}
                >
                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>{translate('CreateEmptyGameFolders')}</FormLabel>

                    <FormInputHelpText
                      text={translate('CreateEmptyGameFoldersHelpText')}
                    />

                    <FormInput
                      type={inputTypes.CHECK}
                      isDisabled={
                        settings.deleteEmptyFolders.value &&
                        !settings.createEmptyGameFolders.value
                      }
                      name="createEmptyGameFolders"
                      onChange={handleInputChange}
                      {...settings.createEmptyGameFolders}
                    />
                  </FormRow>

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>{translate('DeleteEmptyFolders')}</FormLabel>

                    <FormInputHelpText
                      text={translate('DeleteEmptyGameFoldersHelpText')}
                    />

                    <FormInput
                      type={inputTypes.CHECK}
                      isDisabled={
                        settings.createEmptyGameFolders.value &&
                        !settings.deleteEmptyFolders.value
                      }
                      name="deleteEmptyFolders"
                      onChange={handleInputChange}
                      {...settings.deleteEmptyFolders}
                    />
                  </FormRow>
                </FieldSet>
              ) : null}

              {showAdvancedSettings ? (
                <FieldSet
                  legend={translate('Importing')}
                  caption={translate('ImportingCaption')}
                >
                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>{translate('SkipFreeSpaceCheck')}</FormLabel>

                    <FormInputHelpText
                      text={translate('SkipFreeSpaceCheckHelpText')}
                    />

                    <FormInput
                      type={inputTypes.CHECK}
                      name="skipFreeSpaceCheckWhenImporting"
                      onChange={handleInputChange}
                      {...settings.skipFreeSpaceCheckWhenImporting}
                    />
                  </FormRow>

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>{translate('MinimumFreeSpace')}</FormLabel>

                    <FormInputHelpText
                      text={translate('MinimumFreeSpaceHelpText')}
                    />

                    <FormInput
                      type={inputTypes.NUMBER}
                      unit="MB"
                      name="minimumFreeSpaceWhenImporting"
                      onChange={handleInputChange}
                      {...settings.minimumFreeSpaceWhenImporting}
                    />
                  </FormRow>

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>
                      {translate('UseHardlinksInsteadOfCopy')}
                    </FormLabel>

                    <FormInputHelpText
                      text={translate('CopyUsingHardlinksGameHelpText')}
                    />

                    <FormInputHelpText
                      text={translate('CopyUsingHardlinksHelpTextWarning')}
                      isWarning={true}
                    />
                    <FormInput
                      type={inputTypes.CHECK}
                      name="copyUsingHardlinks"
                      onChange={handleInputChange}
                      {...settings.copyUsingHardlinks}
                    />
                  </FormRow>

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>{translate('ImportUsingScript')}</FormLabel>

                    <FormInputHelpText
                      text={translate('ImportUsingScriptHelpText')}
                    />

                    <FormInput
                      type={inputTypes.CHECK}
                      name="useScriptImport"
                      onChange={handleInputChange}
                      {...settings.useScriptImport}
                    />
                  </FormRow>

                  {settings.useScriptImport.value ? (
                    <FormRow
                      advancedSettings={showAdvancedSettings}
                      isAdvanced={true}
                    >
                      <FormLabel>{translate('ImportScriptPath')}</FormLabel>

                      <FormInputHelpText
                        text={translate('ImportScriptPathHelpText')}
                      />

                      <FormInput
                        type={inputTypes.PATH}
                        includeFiles={true}
                        name="scriptImportPath"
                        onChange={handleInputChange}
                        {...settings.scriptImportPath}
                      />
                    </FormRow>
                  ) : null}

                  <FormRow size={sizes.MEDIUM}>
                    <FormLabel>{translate('ImportExtraFiles')}</FormLabel>

                    <FormInputHelpText
                      text={translate('ImportExtraFilesGameHelpText')}
                    />

                    <FormInput
                      type={inputTypes.CHECK}
                      name="importExtraFiles"
                      onChange={handleInputChange}
                      {...settings.importExtraFiles}
                    />
                  </FormRow>

                  {settings.importExtraFiles.value ? (
                    <FormRow
                      advancedSettings={showAdvancedSettings}
                      isAdvanced={true}
                    >
                      <FormLabel>{translate('ImportExtraFiles')}</FormLabel>

                      <FormInputHelpText
                        text={translate('ExtraFileExtensionsHelpText')}
                      />
                      <FormInputHelpText
                        text={translate('ExtraFileExtensionsHelpTextsExamples')}
                      />
                      <FormInput
                        type={inputTypes.TEXT}
                        name="extraFileExtensions"
                        onChange={handleInputChange}
                        {...settings.extraFileExtensions}
                      />
                    </FormRow>
                  ) : null}
                </FieldSet>
              ) : null}

              <FieldSet
                legend={translate('FileManagement')}
                caption={translate('FileManagementCaption')}
              >
                <FormRow size={sizes.MEDIUM}>
                  <FormLabel>{translate('UnmonitorDeletedGames')}</FormLabel>

                  <FormInputHelpText
                    text={translate('UnmonitorDeletedGamesHelpText')}
                  />

                  <FormInput
                    type={inputTypes.CHECK}
                    name="autoUnmonitorPreviouslyDownloadedGames"
                    onChange={handleInputChange}
                    {...settings.autoUnmonitorPreviouslyDownloadedGames}
                  />
                </FormRow>

                <FormRow
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                  size={sizes.MEDIUM}
                >
                  <FormLabel>
                    {translate('DownloadPropersAndRepacks')}
                  </FormLabel>

                  <FormInputHelpText
                    text={translate('DownloadPropersAndRepacksHelpText')}
                  />
                  <FormInputHelpText
                    text={translate(
                      'DownloadPropersAndRepacksHelpTextCustomFormat'
                    )}
                  />
                  {settings.downloadPropersAndRepacks.value ===
                  'doNotPrefer' ? (
                    <FormInputHelpText
                      text={translate(
                        'DownloadPropersAndRepacksHelpTextWarning'
                      )}
                      isWarning={true}
                    />
                  ) : null}
                  <FormInput
                    type={inputTypes.SELECT}
                    name="downloadPropersAndRepacks"
                    values={downloadPropersAndRepacksOptions}
                    onChange={handleInputChange}
                    {...settings.downloadPropersAndRepacks}
                  />
                </FormRow>

                <FormRow
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>
                    {translate('RescanGameFolderAfterRefresh')}
                  </FormLabel>

                  <FormInputHelpText
                    text={translate('RescanAfterRefreshGameHelpText')}
                  />

                  <FormInputHelpText
                    text={translate('RescanAfterRefreshHelpTextWarning')}
                    isWarning={true}
                  />
                  <FormInput
                    type={inputTypes.SELECT}
                    name="rescanAfterRefresh"
                    values={rescanAfterRefreshOptions}
                    onChange={handleInputChange}
                    {...settings.rescanAfterRefresh}
                  />
                </FormRow>

                <FormRow
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>{translate('ChangeFileDate')}</FormLabel>

                  <FormInputHelpText
                    text={translate('ChangeFileDateHelpText')}
                  />

                  <FormInput
                    type={inputTypes.SELECT}
                    name="fileDate"
                    values={fileDateOptions}
                    onChange={handleInputChange}
                    {...settings.fileDate}
                  />
                </FormRow>

                <FormRow
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>{translate('RecyclingBin')}</FormLabel>

                  <FormInputHelpText text={translate('RecyclingBinHelpText')} />

                  <FormInput
                    type={inputTypes.PATH}
                    name="recycleBin"
                    includeFiles={false}
                    onChange={handleInputChange}
                    {...settings.recycleBin}
                  />
                </FormRow>

                <FormRow
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>{translate('RecyclingBinCleanup')}</FormLabel>

                  <FormInputHelpText
                    text={translate('RecyclingBinCleanupHelpText')}
                  />

                  <FormInputHelpText
                    text={translate('RecyclingBinCleanupHelpTextWarning')}
                    isWarning={true}
                  />
                  <FormInput
                    type={inputTypes.NUMBER}
                    name="recycleBinCleanupDays"
                    min={0}
                    onChange={handleInputChange}
                    {...settings.recycleBinCleanupDays}
                  />
                </FormRow>
              </FieldSet>

              {showAdvancedSettings && !isWindows ? (
                <FieldSet
                  legend={translate('Permissions')}
                  caption={translate('PermissionsCaption')}
                >
                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>{translate('SetPermissions')}</FormLabel>

                    <FormInputHelpText
                      text={translate('SetPermissionsLinuxHelpText')}
                    />

                    <FormInputHelpText
                      text={translate('SetPermissionsLinuxHelpTextWarning')}
                      isWarning={true}
                    />
                    <FormInput
                      type={inputTypes.CHECK}
                      name="setPermissionsLinux"
                      onChange={handleInputChange}
                      {...settings.setPermissionsLinux}
                    />
                  </FormRow>

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>{translate('ChmodFolder')}</FormLabel>

                    <FormInputHelpText
                      text={translate('ChmodFolderHelpText')}
                    />
                    <FormInputHelpText
                      text={translate('ChmodFolderHelpTextWarning')}
                      isWarning={true}
                    />

                    <FormInput
                      type={inputTypes.UMASK}
                      name="chmodFolder"
                      onChange={handleInputChange}
                      {...settings.chmodFolder}
                    />
                  </FormRow>

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>{translate('ChownGroup')}</FormLabel>

                    <FormInputHelpText text={translate('ChownGroupHelpText')} />
                    <FormInputHelpText
                      text={translate('ChownGroupHelpTextWarning')}
                      isWarning={true}
                    />

                    <FormInput
                      type={inputTypes.TEXT}
                      name="chownGroup"
                      onChange={handleInputChange}
                      {...settings.chownGroup}
                    />
                  </FormRow>
                </FieldSet>
              ) : null}
            </Form>
          ) : null}

          <SectionHeading
            title={translate('RootFolders')}
            description={translate('RootFoldersCaption')}
          />

          <RootFolders />

          <AddRootFolder />
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

export default MediaManagement;
