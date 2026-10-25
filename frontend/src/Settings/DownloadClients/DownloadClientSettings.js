import PropTypes from 'prop-types';
import React, { Component, Fragment } from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import { OverflowDivider } from 'Components/Page/Toolbar/Overflow';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import SectionHeading from 'Components/SectionHeading';
import { icons } from 'Helpers/Props';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import DownloadClientsConnector from './DownloadClients/DownloadClientsConnector';
import ManageDownloadClientsModal from './DownloadClients/Manage/ManageDownloadClientsModal';
import DownloadClientOptionsConnector from './Options/DownloadClientOptionsConnector';
import RemotePathMappingsConnector from './RemotePathMappings/RemotePathMappingsConnector';

class DownloadClientSettings extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._saveCallback = null;

    this.state = {
      isSaving: false,
      hasPendingChanges: false,
      isManageDownloadClientsOpen: false
    };
  }

  //
  // Listeners

  onChildMounted = (saveCallback) => {
    this._saveCallback = saveCallback;
  };

  onChildStateChange = (payload) => {
    this.setState(payload);
  };

  onManageDownloadClientsPress = () => {
    this.setState({ isManageDownloadClientsOpen: true });
  };

  onManageDownloadClientsModalClose = () => {
    this.setState({ isManageDownloadClientsOpen: false });
  };

  onSavePress = () => {
    if (this._saveCallback) {
      this._saveCallback();
    }
  };

  //
  // Render

  render() {
    const {
      isTestingAll,
      dispatchTestAllDownloadClients
    } = this.props;

    const {
      isSaving,
      hasPendingChanges,
      isManageDownloadClientsOpen
    } = this.state;

    return (
      <SettingsPage
        title={translate('DownloadClientSettings')}
        isSaving={isSaving}
        hasPendingChanges={hasPendingChanges}
        toolbarChildren={
          <Fragment>
            <OverflowDivider groupId="extras">
              <PageToolbarSeparator />
            </OverflowDivider>

            <ToolbarItem
              id="test-all"
              priority={1}
              groupId="extras"
              label={translate('TestAllClients')}
              iconName={icons.TEST}
              isSpinning={isTestingAll}
              onPress={dispatchTestAllDownloadClients}
            />

            <ToolbarItem
              id="manage"
              priority={1}
              groupId="extras"
              label={translate('ManageClients')}
              iconName={icons.MANAGE}
              onPress={this.onManageDownloadClientsPress}
            />
          </Fragment>
        }
        onSavePress={this.onSavePress}
      >
        <PageContentBody>
          <div className={settingsStyles.section}>
            <PageHeading
              scope={translate('Settings')}
              title={translate('DownloadClients')}
            />

            <div className={settingsStyles.pageSection}>
              <SectionHeading
                title={translate('DownloadClients')}
                description={translate('DownloadClientsSectionDescription')}
              />

              <DownloadClientsConnector />
            </div>

            <DownloadClientOptionsConnector
              onChildMounted={this.onChildMounted}
              onChildStateChange={this.onChildStateChange}
            />

            <RemotePathMappingsConnector />

            <ManageDownloadClientsModal
              isOpen={isManageDownloadClientsOpen}
              onModalClose={this.onManageDownloadClientsModalClose}
            />
          </div>
        </PageContentBody>
      </SettingsPage>
    );
  }
}

DownloadClientSettings.propTypes = {
  isTestingAll: PropTypes.bool.isRequired,
  dispatchTestAllDownloadClients: PropTypes.func.isRequired
};

export default DownloadClientSettings;
