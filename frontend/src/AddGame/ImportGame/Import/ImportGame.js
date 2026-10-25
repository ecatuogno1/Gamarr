import { reduce } from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import ImportGameFooterConnector from './ImportGameFooterConnector';
import ImportGameList from './ImportGameList';
import styles from './ImportGame.css';

class ImportGame extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.scrollerRef = React.createRef();

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      contentBody: null
    };
  }

  //
  // Listeners

  getSelectedIds = () => {
    const { items = [] } = this.props;

    return reduce(
      this.state.selectedState,
      (result, value, id) => {
        // Imported items are removed from the store while the list is
        // remounting (root folders refetch), so ignore ids that are gone.
        if (value && items.some((item) => item.id === id)) {
          result.push(id);
        }

        return result;
      },
      []
    );
  };

  onSelectAllChange = ({ value }) => {
    // Only select non-dupes
    this.setState(selectAll(this.state.selectedState, value));
  };

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey);
    });
  };

  onRemoveSelectedStateItem = (id) => {
    this.setState((state) => {
      const selectedState = Object.assign({}, state.selectedState);
      delete selectedState[id];

      return {
        ...state,
        selectedState
      };
    });
  };

  onInputChange = ({ name, value }) => {
    this.props.onInputChange(this.getSelectedIds(), name, value);
  };

  onImportPress = () => {
    this.props.onImportPress(this.getSelectedIds());
  };

  //
  // Render

  render() {
    const {
      path,
      rootFoldersFetching,
      rootFoldersError,
      rootFoldersPopulated,
      unmappedFolders
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState
    } = this.state;

    return (
      <PageContent title={translate('ImportGames')}>
        <PageContentBody ref={this.scrollerRef} >
          <div className={styles.content}>
            <PageHeading
              scope={translate('Media')}
              title={translate('ImportGames')}
            />

            {
              rootFoldersFetching ? <LoadingIndicator /> : null
            }

            {
              !rootFoldersFetching && !!rootFoldersError ?
                <Alert kind={kinds.DANGER}>
                  {translate('RootFoldersLoadError')}
                </Alert> :
                null
            }

            {
              !rootFoldersError &&
              !rootFoldersFetching &&
              rootFoldersPopulated &&
              !unmappedFolders.length ?
                <Alert kind={kinds.INFO}>
                  {translate('AllGamesInPathHaveBeenImported', { path })}
                </Alert> :
                null
            }

            {
              !rootFoldersError &&
              !rootFoldersFetching &&
              rootFoldersPopulated &&
              !!unmappedFolders.length &&
              this.scrollerRef.current ?
                <ImportGameList
                  unmappedFolders={unmappedFolders}
                  selectedState={selectedState}
                  scrollerRef={this.scrollerRef}
                  onSelectedChange={this.onSelectedChange}
                  onRemoveSelectedStateItem={this.onRemoveSelectedStateItem}
                /> :
                null
            }
          </div>
        </PageContentBody>

        {
          !rootFoldersError &&
          !rootFoldersFetching &&
          !!unmappedFolders.length ?
            <ImportGameFooterConnector
              selectedIds={this.getSelectedIds()}
              allSelected={allSelected}
              allUnselected={allUnselected}
              onSelectAllChange={this.onSelectAllChange}
              onInputChange={this.onInputChange}
              onImportPress={this.onImportPress}
            /> :
            null
        }
      </PageContent>
    );
  }
}

ImportGame.propTypes = {
  rootFolderId: PropTypes.number.isRequired,
  path: PropTypes.string,
  rootFoldersFetching: PropTypes.bool.isRequired,
  rootFoldersPopulated: PropTypes.bool.isRequired,
  rootFoldersError: PropTypes.object,
  unmappedFolders: PropTypes.arrayOf(PropTypes.object),
  items: PropTypes.arrayOf(PropTypes.object),
  onInputChange: PropTypes.func.isRequired,
  onImportPress: PropTypes.func.isRequired
};

ImportGame.defaultProps = {
  unmappedFolders: []
};

export default ImportGame;
