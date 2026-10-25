import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import SettingsCardStatus from 'Components/SettingsCard/SettingsCardStatus';
import TagList from 'Components/TagList';
import { kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditNotificationModalConnector from './EditNotificationModalConnector';

class Notification extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditNotificationModalOpen: false,
      isDeleteNotificationModalOpen: false
    };
  }

  //
  // Listeners

  onEditNotificationPress = () => {
    this.setState({ isEditNotificationModalOpen: true });
  };

  onEditNotificationModalClose = () => {
    this.setState({ isEditNotificationModalOpen: false });
  };

  onDeleteNotificationPress = () => {
    this.setState({
      isEditNotificationModalOpen: false,
      isDeleteNotificationModalOpen: true
    });
  };

  onDeleteNotificationModalClose= () => {
    this.setState({ isDeleteNotificationModalOpen: false });
  };

  onConfirmDeleteNotification = () => {
    this.props.onConfirmDeleteNotification(this.props.id);
  };

  //
  // Render

  render() {
    const {
      id,
      name,
      onGrab,
      onDownload,
      onUpgrade,
      onRename,
      onGameAdded,
      onGameDelete,
      onGameFileDelete,
      onGameFileDeleteForUpgrade,
      onHealthIssue,
      onHealthRestored,
      onApplicationUpdate,
      onManualInteractionRequired,
      supportsOnGrab,
      supportsOnDownload,
      supportsOnUpgrade,
      supportsOnRename,
      supportsOnGameAdded,
      supportsOnGameDelete,
      supportsOnGameFileDelete,
      supportsOnGameFileDeleteForUpgrade,
      supportsOnHealthIssue,
      supportsOnHealthRestored,
      supportsOnApplicationUpdate,
      supportsOnManualInteractionRequired,
      tags,
      tagList
    } = this.props;

    const enabled = onGrab || onDownload || onRename || onHealthIssue || onHealthRestored || onApplicationUpdate || onGameAdded || onGameDelete || onGameFileDelete || onManualInteractionRequired;

    return (
      <SettingsCard
        name={name}
        aria-label={translate('EditConnectionName', { name })}
        onPress={this.onEditNotificationPress}
      >
        <SettingsCardStatus
          dot={enabled ? 'active' : 'muted'}
          segments={[translate(enabled ? 'Enabled' : 'Disabled')]}
        />

        <div className={settingsCardStyles.labels}>
          {
            supportsOnGrab && onGrab ?
              <Label dot={false} kind={kinds.DEFAULT}
                size={sizes.MEDIUM}
              >
                {translate('OnGrab')}
              </Label> :
              null
          }

          {
            supportsOnDownload && onDownload ?
              <Label dot={false} kind={kinds.DEFAULT}
                size={sizes.MEDIUM}
              >
                {translate('OnFileImport')}
              </Label> :
              null
          }

          {
            supportsOnUpgrade && onDownload && onUpgrade ?
              <Label dot={false} kind={kinds.DEFAULT}
                size={sizes.MEDIUM}
              >
                {translate('OnFileUpgrade')}
              </Label> :
              null
          }

          {
            supportsOnRename && onRename ?
              <Label dot={false} kind={kinds.DEFAULT}
                size={sizes.MEDIUM}
              >
                {translate('OnRename')}
              </Label> :
              null
          }

          {
            supportsOnGameAdded && onGameAdded ?
              <Label dot={false} kind={kinds.DEFAULT}
                size={sizes.MEDIUM}
              >
                {translate('OnGameAdded')}
              </Label> :
              null
          }

          {
            supportsOnHealthIssue && onHealthIssue ?
              <Label dot={false} kind={kinds.DEFAULT}
                size={sizes.MEDIUM}
              >
                {translate('OnHealthIssue')}
              </Label> :
              null
          }

          {
            supportsOnHealthRestored && onHealthRestored ?
              <Label dot={false} kind={kinds.DEFAULT}
                size={sizes.MEDIUM}
              >
                {translate('OnHealthRestored')}
              </Label> :
              null
          }

          {
            supportsOnApplicationUpdate && onApplicationUpdate ?
              <Label dot={false} kind={kinds.DEFAULT}
                size={sizes.MEDIUM}
              >
                {translate('OnApplicationUpdate')}
              </Label> :
              null
          }

          {
            supportsOnGameDelete && onGameDelete ?
              <Label dot={false} kind={kinds.DEFAULT}
                size={sizes.MEDIUM}
              >
                {translate('OnGameDelete')}
              </Label> :
              null
          }

          {
            supportsOnGameFileDelete && onGameFileDelete ?
              <Label dot={false} kind={kinds.DEFAULT}
                size={sizes.MEDIUM}
              >
                {translate('OnGameFileDelete')}
              </Label> :
              null
          }

          {
            supportsOnGameFileDeleteForUpgrade && onGameFileDelete && onGameFileDeleteForUpgrade ?
              <Label dot={false} kind={kinds.DEFAULT}
                size={sizes.MEDIUM}
              >
                {translate('OnGameFileDeleteForUpgrade')}
              </Label> :
              null
          }

          {
            supportsOnManualInteractionRequired && onManualInteractionRequired ?
              <Label dot={false} kind={kinds.DEFAULT}
                size={sizes.MEDIUM}
              >
                {translate('OnManualInteractionRequired')}
              </Label> :
              null
          }
        </div>

        {
          tags.length > 0 ?
            <TagList
              tags={tags}
              tagList={tagList}
            /> :
            null
        }
        <EditNotificationModalConnector
          id={id}
          isOpen={this.state.isEditNotificationModalOpen}
          onModalClose={this.onEditNotificationModalClose}
          onDeleteNotificationPress={this.onDeleteNotificationPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteNotificationModalOpen}
          kind={kinds.DANGER}
          title={translate('DeleteNotification')}
          message={translate('DeleteNotificationMessageText', { name })}
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmDeleteNotification}
          onCancel={this.onDeleteNotificationModalClose}
        />
      </SettingsCard>
    );
  }
}

Notification.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  onGrab: PropTypes.bool.isRequired,
  onDownload: PropTypes.bool.isRequired,
  onUpgrade: PropTypes.bool.isRequired,
  onRename: PropTypes.bool.isRequired,
  onGameAdded: PropTypes.bool.isRequired,
  onGameDelete: PropTypes.bool.isRequired,
  onGameFileDelete: PropTypes.bool.isRequired,
  onGameFileDeleteForUpgrade: PropTypes.bool.isRequired,
  onHealthIssue: PropTypes.bool.isRequired,
  onHealthRestored: PropTypes.bool.isRequired,
  onApplicationUpdate: PropTypes.bool.isRequired,
  onManualInteractionRequired: PropTypes.bool.isRequired,
  supportsOnGrab: PropTypes.bool.isRequired,
  supportsOnDownload: PropTypes.bool.isRequired,
  supportsOnGameDelete: PropTypes.bool.isRequired,
  supportsOnGameFileDelete: PropTypes.bool.isRequired,
  supportsOnGameFileDeleteForUpgrade: PropTypes.bool.isRequired,
  supportsOnUpgrade: PropTypes.bool.isRequired,
  supportsOnRename: PropTypes.bool.isRequired,
  supportsOnGameAdded: PropTypes.bool.isRequired,
  supportsOnHealthIssue: PropTypes.bool.isRequired,
  supportsOnHealthRestored: PropTypes.bool.isRequired,
  supportsOnApplicationUpdate: PropTypes.bool.isRequired,
  supportsOnManualInteractionRequired: PropTypes.bool.isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteNotification: PropTypes.func.isRequired
};

export default Notification;
