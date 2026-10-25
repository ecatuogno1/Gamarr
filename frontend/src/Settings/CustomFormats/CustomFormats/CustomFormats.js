import PropTypes from 'prop-types';
import React, { Component } from 'react';
import PageSectionContent from 'Components/Page/PageSectionContent';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import translate from 'Utilities/String/translate';
import CustomFormat from './CustomFormat';
import EditCustomFormatModalConnector from './EditCustomFormatModalConnector';

class CustomFormats extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isCustomFormatModalOpen: false,
      tagsFromId: undefined
    };
  }

  //
  // Listeners

  onCloneCustomFormatPress = (id) => {
    this.props.onCloneCustomFormatPress(id);
    this.setState({
      isCustomFormatModalOpen: true,
      tagsFromId: id
    });
  };

  onEditCustomFormatPress = () => {
    this.setState({ isCustomFormatModalOpen: true });
  };

  onModalClose = () => {
    this.setState({
      isCustomFormatModalOpen: false,
      tagsFromId: undefined
    });
  };

  //
  // Render

  render() {
    const {
      items,
      isDeleting,
      onConfirmDeleteCustomFormat,
      onCloneCustomFormatPress,
      ...otherProps
    } = this.props;

    return (
      <PageSectionContent
        errorMessage={translate('CustomFormatsLoadError')}
        {...otherProps}
      >
        <div className={settingsCardStyles.grid}>
          {
            items.map((item) => {
              return (
                <CustomFormat
                  key={item.id}
                  {...item}
                  isDeleting={isDeleting}
                  onConfirmDeleteCustomFormat={onConfirmDeleteCustomFormat}
                  onCloneCustomFormatPress={this.onCloneCustomFormatPress}
                />
              );
            })
          }

          <AddCard
            label={translate('AddCustomFormat')}
            onPress={this.onEditCustomFormatPress}
          />
        </div>

        <EditCustomFormatModalConnector
          isOpen={this.state.isCustomFormatModalOpen}
          tagsFromId={this.state.tagsFromId}
          onModalClose={this.onModalClose}
        />
      </PageSectionContent>
    );
  }
}

CustomFormats.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isDeleting: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteCustomFormat: PropTypes.func.isRequired,
  onCloneCustomFormatPress: PropTypes.func.isRequired
};

export default CustomFormats;
