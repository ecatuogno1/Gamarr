import PropTypes from 'prop-types';
import React, { Component } from 'react';
import PageSectionContent from 'Components/Page/PageSectionContent';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import translate from 'Utilities/String/translate';
import AddDownloadClientModal from './AddDownloadClientModal';
import DownloadClient from './DownloadClient';
import EditDownloadClientModalConnector from './EditDownloadClientModalConnector';

class DownloadClients extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddDownloadClientModalOpen: false,
      isEditDownloadClientModalOpen: false
    };
  }

  //
  // Listeners

  onAddDownloadClientPress = () => {
    this.setState({ isAddDownloadClientModalOpen: true });
  };

  onAddDownloadClientModalClose = ({ downloadClientSelected = false } = {}) => {
    this.setState({
      isAddDownloadClientModalOpen: false,
      isEditDownloadClientModalOpen: downloadClientSelected
    });
  };

  onEditDownloadClientModalClose = () => {
    this.setState({ isEditDownloadClientModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      items,
      onConfirmDeleteDownloadClient,
      tagList,
      ...otherProps
    } = this.props;

    const {
      isAddDownloadClientModalOpen,
      isEditDownloadClientModalOpen
    } = this.state;

    return (
      <PageSectionContent
        errorMessage={translate('DownloadClientsLoadError')}
        {...otherProps}
      >
        <div className={settingsCardStyles.grid}>
          {
            items.map((item) => {
              return (
                <DownloadClient
                  key={item.id}
                  {...item}
                  tagList={tagList}
                  onConfirmDeleteDownloadClient={onConfirmDeleteDownloadClient}
                />
              );
            })
          }

          <AddCard
            label={translate('AddDownloadClient')}
            onPress={this.onAddDownloadClientPress}
          />
        </div>

        <AddDownloadClientModal
          isOpen={isAddDownloadClientModalOpen}
          onModalClose={this.onAddDownloadClientModalClose}
        />

        <EditDownloadClientModalConnector
          isOpen={isEditDownloadClientModalOpen}
          onModalClose={this.onEditDownloadClientModalClose}
        />
      </PageSectionContent>
    );
  }
}

DownloadClients.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteDownloadClient: PropTypes.func.isRequired
};

export default DownloadClients;
