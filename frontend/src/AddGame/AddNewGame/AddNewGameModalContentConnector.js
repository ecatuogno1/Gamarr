import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { addGame, setAddGameDefault } from 'Store/Actions/addGameActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import AddNewGameModalContent from './AddNewGameModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addGame,
    createDimensionsSelector(),
    createSystemStatusSelector(),
    (state) => state.settings.platforms.items,
    (state, { availablePlatforms }) => availablePlatforms,
    (state, { addedPlatformIds }) => addedPlatformIds,
    (addGameState, dimensions, systemStatus, allPlatforms, availablePlatforms = [], addedPlatformIds = []) => {
      const {
        isAdding,
        addError,
        defaults
      } = addGameState;

      const {
        settings,
        validationErrors,
        validationWarnings
      } = selectSettings(defaults, {}, addError);

      // The platforms IGDB lists for the game, already added platforms can't be picked again
      const platformOptions = allPlatforms
        .filter((platform) => platform.enabled && (!availablePlatforms.length || availablePlatforms.includes(platform.id)))
        .map((platform) => ({
          key: platform.id,
          value: platform.name,
          isDisabled: addedPlatformIds.includes(platform.id)
        }));

      return {
        isAdding,
        addError,
        platformOptions,
        isSmallScreen: dimensions.isSmallScreen,
        validationErrors,
        validationWarnings,
        isWindows: systemStatus.isWindows,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  setAddGameDefault,
  addGame
};

class AddNewGameModalContentConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    // Default to the lookup's platform (the first platform IGDB lists), unless it's already in the library
    const firstAvailable = props.platformOptions.find((option) => !option.isDisabled);
    const defaultOption = props.platformOptions.find((option) => option.key === props.platformId && !option.isDisabled);

    this.state = {
      selectedPlatformId: (defaultOption || firstAvailable || {}).key || 0
    };
  }

  //
  // Listeners

  onPlatformChange = ({ value }) => {
    this.setState({ selectedPlatformId: parseInt(value) });
  };

  onInputChange = ({ name, value }) => {
    this.props.setAddGameDefault({ [name]: value });
  };

  onAddGamePress = () => {
    const {
      igdbId,
      rootFolderPath,
      monitor,
      qualityProfileId,
      minimumAvailability,
      searchForGame,
      tags
    } = this.props;

    this.props.addGame({
      igdbId,
      platformId: this.state.selectedPlatformId,
      rootFolderPath: rootFolderPath.value,
      monitor: monitor.value,
      qualityProfileId: qualityProfileId.value,
      minimumAvailability: minimumAvailability.value,
      searchForGame: searchForGame.value,
      tags: tags.value
    });
  };

  //
  // Render

  render() {
    return (
      <AddNewGameModalContent
        {...this.props}
        platformId={this.state.selectedPlatformId}
        onPlatformChange={this.onPlatformChange}
        onInputChange={this.onInputChange}
        onAddGamePress={this.onAddGamePress}
      />
    );
  }
}

AddNewGameModalContentConnector.propTypes = {
  igdbId: PropTypes.number.isRequired,
  platformId: PropTypes.number,
  platformOptions: PropTypes.arrayOf(PropTypes.object).isRequired,
  rootFolderPath: PropTypes.object,
  monitor: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.object,
  minimumAvailability: PropTypes.object.isRequired,
  searchForGame: PropTypes.object.isRequired,
  tags: PropTypes.object.isRequired,
  onModalClose: PropTypes.func.isRequired,
  setAddGameDefault: PropTypes.func.isRequired,
  addGame: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddNewGameModalContentConnector);
