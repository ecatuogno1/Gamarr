import React from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import SectionHeading from 'Components/SectionHeading';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import DelayProfilesConnector from './Delay/DelayProfilesConnector';
import ProfilesDndProvider from './ProfilesDndProvider';
import QualityProfilesConnector from './Quality/QualityProfilesConnector';
import ReleaseProfiles from './Release/ReleaseProfiles';

// Only a single DragDrop Context can exist so it's done here to allow editing
// quality profiles and reordering delay profiles to work. Sonarr v5 moved to
// dnd-kit; Gamarr keeps react-dnd with the v5 styling.

function Profiles() {
  return (
    <SettingsPage title={translate('Profiles')} showSave={false}>
      <PageContentBody>
        <ProfilesDndProvider>
          <div className={settingsStyles.section}>
            <PageHeading
              scope={translate('Settings')}
              title={translate('Profiles')}
            />

            <div className={settingsStyles.pageSection}>
              <SectionHeading
                title={translate('QualityProfiles')}
                description={translate('QualityProfilesSectionDescription')}
              />

              <QualityProfilesConnector />
            </div>

            <div className={settingsStyles.pageSection}>
              <SectionHeading
                title={translate('DelayProfiles')}
                description={translate('DelayProfilesSectionDescription')}
              />

              <DelayProfilesConnector />
            </div>

            <div className={settingsStyles.pageSection}>
              <SectionHeading
                title={translate('ReleaseProfiles')}
                description={translate('ReleaseProfilesSectionDescription')}
              />

              <ReleaseProfiles />
            </div>
          </div>
        </ProfilesDndProvider>
      </PageContentBody>
    </SettingsPage>
  );
}

export default Profiles;
