import PropTypes from 'prop-types';
import React, { Component } from 'react';
import PageSectionContent from 'Components/Page/PageSectionContent';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import translate from 'Utilities/String/translate';
import AddNotificationModal from './AddNotificationModal';
import EditNotificationModalConnector from './EditNotificationModalConnector';
import Notification from './Notification';

class Notifications extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddNotificationModalOpen: false,
      isEditNotificationModalOpen: false
    };
  }

  //
  // Listeners

  onAddNotificationPress = () => {
    this.setState({ isAddNotificationModalOpen: true });
  };

  onAddNotificationModalClose = ({ notificationSelected = false } = {}) => {
    this.setState({
      isAddNotificationModalOpen: false,
      isEditNotificationModalOpen: notificationSelected
    });
  };

  onEditNotificationModalClose = () => {
    this.setState({ isEditNotificationModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      items,
      tagList,
      onConfirmDeleteNotification,
      ...otherProps
    } = this.props;

    const {
      isAddNotificationModalOpen,
      isEditNotificationModalOpen
    } = this.state;

    return (
      <PageSectionContent
        errorMessage={translate('NotificationsLoadError')}
        {...otherProps}
      >
        <div className={settingsCardStyles.grid}>
          {
            items.map((item) => {
              return (
                <Notification
                  key={item.id}
                  {...item}
                  tagList={tagList}
                  onConfirmDeleteNotification={onConfirmDeleteNotification}
                />
              );
            })
          }

          <AddCard
            label={translate('AddConnection')}
            onPress={this.onAddNotificationPress}
          />
        </div>

        <AddNotificationModal
          isOpen={isAddNotificationModalOpen}
          onModalClose={this.onAddNotificationModalClose}
        />

        <EditNotificationModalConnector
          isOpen={isEditNotificationModalOpen}
          onModalClose={this.onEditNotificationModalClose}
        />
      </PageSectionContent>
    );
  }
}

Notifications.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteNotification: PropTypes.func.isRequired
};

export default Notifications;
