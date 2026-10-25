import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import SectionHeading from 'Components/SectionHeading';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import ImportGameRootFolderRowConnector from './ImportGameRootFolderRowConnector';
import styles from './ImportGameSelectFolder.css';

class ImportGameSelectFolder extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddNewRootFolderModalOpen: false
    };
  }

  //
  // Lifecycle

  onAddNewRootFolderPress = () => {
    this.setState({ isAddNewRootFolderModalOpen: true });
  };

  onNewRootFolderSelect = ({ value }) => {
    this.props.onNewRootFolderSelect(value);
  };

  onAddRootFolderModalClose = () => {
    this.setState({ isAddNewRootFolderModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      isWindows,
      isFetching,
      isPopulated,
      isSaving,
      error,
      saveError,
      items
    } = this.props;

    const hasRootFolders = items.length > 0;

    return (
      <PageContent title={translate('ImportGames')}>
        <PageContentBody>
          <PageHeading
            scope={translate('Media')}
            title={translate('ImportGames')}
          />

          {
            isFetching && !isPopulated ?
              <LoadingIndicator /> :
              null
          }

          {
            !isFetching && error ?
              <Alert kind={kinds.DANGER}>
                {translate('RootFoldersLoadError')}
              </Alert> :
              null
          }

          {
            !error && isPopulated &&
              <div>
                <SectionHeading
                  title={translate('LibraryImportGamesHeader')}
                  description={translate('ImportHeader')}
                />

                <details className={styles.tips}>
                  <summary className={styles.tipsSummary}>
                    <Icon
                      className={styles.tipsChevron}
                      name={icons.EXPAND_INDETERMINATE}
                      size={12}
                    />
                    {translate('ImportTipsMessage')}
                  </summary>

                  <ul className={styles.tipsList}>
                    <li dangerouslySetInnerHTML={{ __html: translate('ImportIncludeQuality', ['<code>Super Mario World (USA) (Rev 1).sfc</code>']) }} />
                    <li dangerouslySetInnerHTML={{ __html: translate('ImportRootPath', [`<code>${isWindows ? 'C:\\games' : '/games'}</code>`, `<code>${isWindows ? 'C:\\games\\snes\\Super Mario World (1990)' : '/games/snes/Super Mario World (1990)'}</code>`]) }} />
                    <li>{translate('ImportNotForDownloads')}</li>
                  </ul>
                </details>

                {
                  hasRootFolders ?
                    <div className={styles.rootFolders}>
                      {
                        items.map((rootFolder) => {
                          return (
                            <ImportGameRootFolderRowConnector
                              key={rootFolder.id}
                              id={rootFolder.id}
                              path={rootFolder.path}
                              accessible={rootFolder.accessible}
                              freeSpace={rootFolder.freeSpace}
                              unmappedFolders={rootFolder.unmappedFolders}
                            />
                          );
                        })
                      }
                    </div> :
                    null
                }

                {
                  !isSaving && saveError ?
                    <Alert
                      className={styles.addErrorAlert}
                      kind={kinds.DANGER}
                    >
                      {translate('AddRootFolderError')}

                      <ul>
                        {
                          Array.isArray(saveError.responseJSON) ?
                            saveError.responseJSON.map((e, index) => {
                              return (
                                <li key={index}>
                                  {e.errorMessage}
                                </li>
                              );
                            }) :
                            <li>
                              {
                                JSON.stringify(saveError.responseJSON)
                              }
                            </li>
                        }
                      </ul>
                    </Alert> :
                    null
                }

                <div className={styles.chooseFolderButtonContainer}>
                  <Button
                    kind={hasRootFolders ? kinds.DEFAULT : kinds.PRIMARY}
                    onPress={this.onAddNewRootFolderPress}
                  >
                    <Icon
                      className={styles.importButtonIcon}
                      name={icons.DRIVE}
                    />
                    {
                      hasRootFolders ?
                        translate('ChooseAnotherFolder') :
                        translate('StartImport')
                    }
                  </Button>
                </div>

                <FileBrowserModal
                  isOpen={this.state.isAddNewRootFolderModalOpen}
                  name="rootFolderPath"
                  value=""
                  onChange={this.onNewRootFolderSelect}
                  onModalClose={this.onAddRootFolderModalClose}
                />
              </div>
          }
        </PageContentBody>
      </PageContent>
    );
  }
}

ImportGameSelectFolder.propTypes = {
  isWindows: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  error: PropTypes.object,
  saveError: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onNewRootFolderSelect: PropTypes.func.isRequired,
  onDeleteRootFolderPress: PropTypes.func.isRequired
};

export default ImportGameSelectFolder;
