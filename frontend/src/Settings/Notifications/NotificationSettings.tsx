import React from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import SectionHeading from 'Components/SectionHeading';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import NotificationsConnector from './Notifications/NotificationsConnector';

function NotificationSettings() {
  return (
    <SettingsPage title={translate('ConnectSettings')} showSave={false}>
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading
            scope={translate('Settings')}
            title={translate('Connect')}
          />

          <div className={settingsStyles.pageSection}>
            <SectionHeading
              title={translate('Connections')}
              description={translate('NotificationsSectionDescription')}
            />

            <NotificationsConnector />
          </div>
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

export default NotificationSettings;
