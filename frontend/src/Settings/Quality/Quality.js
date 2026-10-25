import PropTypes from 'prop-types';
import React, { Component } from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import { OverflowDivider } from 'Components/Page/Toolbar/Overflow';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import { icons } from 'Helpers/Props';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import QualityDefinitionsConnector from './Definition/QualityDefinitionsConnector';
import ResetQualityDefinitionsModal from './Reset/ResetQualityDefinitionsModal';

class Quality extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._saveCallback = null;

    this.state = {
      isSaving: false,
      hasPendingChanges: false,
      isConfirmQualityDefinitionResetModalOpen: false
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

  onResetQualityDefinitionsPress = () => {
    this.setState({ isConfirmQualityDefinitionResetModalOpen: true });
  };

  onCloseResetQualityDefinitionsModal = () => {
    this.setState({ isConfirmQualityDefinitionResetModalOpen: false });
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
      isSaving,
      isResettingQualityDefinitions,
      hasPendingChanges
    } = this.state;

    return (
      <SettingsPage
        title={translate('QualitySettings')}
        isSaving={isSaving}
        hasPendingChanges={hasPendingChanges}
        toolbarChildren={
          <>
            <OverflowDivider groupId="extras">
              <PageToolbarSeparator />
            </OverflowDivider>

            <ToolbarItem
              id="reset-definitions"
              priority={1}
              groupId="extras"
              label={translate('ResetDefinitions')}
              iconName={icons.REFRESH}
              isSpinning={isResettingQualityDefinitions}
              isDisabled={isResettingQualityDefinitions}
              onPress={this.onResetQualityDefinitionsPress}
            />
          </>
        }
        onSavePress={this.onSavePress}
      >
        <PageContentBody>
          <div className={settingsStyles.section}>
            <PageHeading
              scope={translate('Settings')}
              title={translate('Quality')}
            />

            <QualityDefinitionsConnector
              onChildMounted={this.onChildMounted}
              onChildStateChange={this.onChildStateChange}
            />
          </div>
        </PageContentBody>

        <ResetQualityDefinitionsModal
          isOpen={this.state.isConfirmQualityDefinitionResetModalOpen}
          onModalClose={this.onCloseResetQualityDefinitionsModal}
        />
      </SettingsPage>
    );
  }
}

Quality.propTypes = {
  isResettingQualityDefinitions: PropTypes.bool.isRequired
};

export default Quality;
