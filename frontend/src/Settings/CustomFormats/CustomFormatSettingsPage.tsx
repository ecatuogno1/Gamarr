import React, { useCallback, useState } from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import { OverflowDivider } from 'Components/Page/Toolbar/Overflow';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import SectionHeading from 'Components/SectionHeading';
import { icons } from 'Helpers/Props';
import ParseModal from 'Parse/ParseModal';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import CustomFormatsConnector from './CustomFormats/CustomFormatsConnector';
import ManageCustomFormatsModal from './CustomFormats/Manage/ManageCustomFormatsModal';

function CustomFormatSettingsPage() {
  const [isManageCustomFormatsOpen, setIsManageCustomFormatsOpen] =
    useState(false);

  const handleManageCustomFormatsPress = useCallback(() => {
    setIsManageCustomFormatsOpen(true);
  }, []);

  const handleManageCustomFormatsClose = useCallback(() => {
    setIsManageCustomFormatsOpen(false);
  }, []);

  const [isParseModalOpen, setIsParseModalOpen] = useState(false);

  const handleParsePress = useCallback(() => {
    setIsParseModalOpen(true);
  }, []);

  const handleParseModalClose = useCallback(() => {
    setIsParseModalOpen(false);
  }, []);

  return (
    <SettingsPage
      title={translate('CustomFormatsSettings')}
      showSave={false}
      toolbarChildren={
        <>
          <OverflowDivider groupId="extras">
            <PageToolbarSeparator />
          </OverflowDivider>

          <ToolbarItem
            id="test-parsing"
            priority={1}
            groupId="extras"
            label={translate('TestParsing')}
            iconName={icons.PARSE}
            onPress={handleParsePress}
          />

          <ToolbarItem
            id="manage-custom-formats"
            priority={1}
            groupId="extras"
            label={translate('ManageFormats')}
            iconName={icons.MANAGE}
            onPress={handleManageCustomFormatsPress}
          />
        </>
      }
    >
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading
            scope={translate('Settings')}
            title={translate('CustomFormats')}
          />

          <div className={settingsStyles.pageSection}>
            <SectionHeading
              title={translate('CustomFormats')}
              description={translate('CustomFormatsSectionDescription')}
            />

            <CustomFormatsConnector />
          </div>
        </div>
      </PageContentBody>

      <ManageCustomFormatsModal
        isOpen={isManageCustomFormatsOpen}
        onModalClose={handleManageCustomFormatsClose}
      />

      <ParseModal
        isOpen={isParseModalOpen}
        onModalClose={handleParseModalClose}
      />
    </SettingsPage>
  );
}

export default CustomFormatSettingsPage;
