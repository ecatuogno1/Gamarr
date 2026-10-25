import React, { useCallback, useRef, useState } from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import SectionHeading from 'Components/SectionHeading';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import Metadatas from './Metadata/Metadatas';
import MetadataOptionsConnector from './Options/MetadataOptionsConnector';

interface ChildState {
  isSaving: boolean;
  hasPendingChanges: boolean;
}

function MetadataSettings() {
  const saveCallback = useRef<(() => void) | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const [hasPendingChanges, setHasPendingChanges] = useState(false);

  const handleChildMounted = useCallback((callback: () => void) => {
    saveCallback.current = callback;
  }, []);

  const handleChildStateChange = useCallback(
    ({ isSaving, hasPendingChanges }: ChildState) => {
      setIsSaving(isSaving);
      setHasPendingChanges(hasPendingChanges);
    },
    []
  );

  const handleSavePress = useCallback(() => {
    saveCallback.current?.();
  }, []);

  return (
    <SettingsPage
      title={translate('MetadataSettings')}
      isSaving={isSaving}
      hasPendingChanges={hasPendingChanges}
      onSavePress={handleSavePress}
    >
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading
            scope={translate('Settings')}
            title={translate('Metadata')}
          />

          <div className={settingsStyles.pageSection}>
            <SectionHeading
              title={translate('Metadata')}
              description={translate('MetadataSectionDescription')}
            />

            <Metadatas />
          </div>

          <MetadataOptionsConnector
            onChildMounted={handleChildMounted}
            onChildStateChange={handleChildStateChange}
          />
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

export default MetadataSettings;
