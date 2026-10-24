import { sortBy } from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteGameFile, setGameFilesSort, setGameFilesTableOption } from 'Store/Actions/gameFileActions';
import { fetchLanguages, fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import getQualities from 'Utilities/Quality/getQualities';
import GameFileEditorTableContent from './GameFileEditorTableContent';

const PLATFORM_COLUMN = 'platform';

function createMapStateToProps() {
  return createSelector(
    (state, { gameIds }) => gameIds,
    createClientSideCollectionSelector('gameFiles'),
    (state) => state.settings.languages,
    (state) => state.settings.qualityProfiles,
    (state) => state.games,
    (
      gameIds,
      gameFiles,
      languageProfiles,
      qualityProfiles,
      games
    ) => {
      const languages = languageProfiles.items;
      const qualities = getQualities(qualityProfiles.schema.items);
      const filesForGames = gameFiles.items.filter((file) => gameIds.includes(file.gameId));

      // The files of several platform versions of a title get a Platform column, grouped by platform
      const showPlatform = gameIds.length > 1;
      const platformNames = {};

      filesForGames.forEach((file) => {
        const game = games.items[games.itemMap[file.gameId]];

        platformNames[file.id] = game?.platform?.name;
      });

      return {
        items: showPlatform ? sortBy(filesForGames, (file) => platformNames[file.id] ?? '') : filesForGames,
        allColumns: gameFiles.columns,
        columns: showPlatform ?
          gameFiles.columns :
          gameFiles.columns.filter((column) => column.name !== PLATFORM_COLUMN),
        platformNames,
        sortKey: gameFiles.sortKey,
        sortDirection: gameFiles.sortDirection,
        isDeleting: gameFiles.isDeleting,
        isSaving: gameFiles.isSaving,
        error: null,
        languages,
        qualities
      };
    }
  );
}

const mapDispatchToProps = {
  fetchQualityProfileSchema,
  fetchLanguages,
  deleteGameFile,
  setGameFilesTableOption,
  setGameFilesSort
};

class GameFileEditorTableContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchLanguages();
    this.props.fetchQualityProfileSchema();
  }

  //
  // Listeners

  onDeletePress = (gameFileId) => {
    this.props.deleteGameFile({
      id: gameFileId
    });
  };

  onTableOptionChange = (payload) => {
    const { allColumns } = this.props;

    // The platform column is hidden for a single platform, keep it in the saved columns
    if (payload.columns && !payload.columns.some((column) => column.name === PLATFORM_COLUMN)) {
      const index = allColumns.findIndex((column) => column.name === PLATFORM_COLUMN);

      if (index > -1) {
        const columns = [...payload.columns];
        columns.splice(Math.min(index, columns.length), 0, allColumns[index]);

        this.props.setGameFilesTableOption({ ...payload, columns });

        return;
      }
    }

    this.props.setGameFilesTableOption(payload);
  };

  onSortPress = (sortKey, sortDirection) => {
    this.props.setGameFilesSort({
      sortKey,
      sortDirection
    });
  };

  //
  // Render

  render() {
    return (
      <GameFileEditorTableContent
        {...this.props}
        onDeletePress={this.onDeletePress}
        onTableOptionChange={this.onTableOptionChange}
        onSortPress={this.onSortPress}
      />
    );
  }
}

GameFileEditorTableContentConnector.propTypes = {
  gameIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  allColumns: PropTypes.arrayOf(PropTypes.object).isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchLanguages: PropTypes.func.isRequired,
  fetchQualityProfileSchema: PropTypes.func.isRequired,
  deleteGameFile: PropTypes.func.isRequired,
  setGameFilesTableOption: PropTypes.func.isRequired,
  setGameFilesSort: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(GameFileEditorTableContentConnector);
