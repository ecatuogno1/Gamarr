import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import { icons, kinds } from 'Helpers/Props';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import AddNewGameSearchResultConnector from './AddNewGameSearchResultConnector';
import styles from './AddNewGame.css';

class AddNewGame extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.searchInputRef = React.createRef();

    this.state = {
      term: props.term || '',
      isFetching: false
    };
  }

  componentDidMount() {
    const term = this.state.term;

    if (term) {
      this.props.onGameLookupChange(term);
    }
  }

  componentDidUpdate(prevProps) {
    const {
      term,
      isFetching
    } = this.props;

    if (term && term !== prevProps.term) {
      this.setState({
        term,
        isFetching: true
      });
      this.props.onGameLookupChange(term);
    } else if (isFetching !== prevProps.isFetching) {
      this.setState({
        isFetching
      });
    }
  }

  //
  // Listeners

  onSearchInputChange = ({ value }) => {
    const hasValue = !!value.trim();

    this.setState({ term: value, isFetching: hasValue }, () => {
      if (hasValue) {
        this.props.onGameLookupChange(value);
      } else {
        this.props.onClearGameLookup();
      }
    });
  };

  onClearGameLookupPress = () => {
    this.setState({ term: '', isFetching: false });
    this.props.onClearGameLookup();

    if (this.searchInputRef.current) {
      this.searchInputRef.current.focus();
    }
  };

  //
  // Render

  render() {
    const {
      error,
      items,
      hasExistingGames
    } = this.props;

    const term = this.state.term;
    const isFetching = this.state.isFetching;

    return (
      <PageContent title={translate('AddNewGame')}>
        <PageContentBody>
          <PageHeading
            scope={translate('Media')}
            title={translate('AddNewGame')}
          />

          <div className={styles.searchSticky}>
            <div className={styles.searchWrap}>
              <Icon
                className={styles.searchIcon}
                name={icons.SEARCH}
                size={18}
              />

              <TextInput
                ref={this.searchInputRef}
                className={styles.searchInput}
                name="gameLookup"
                value={term}
                placeholder="e.g. Super Metroid, igdb:1103, https://www.igdb.com/games/super-metroid"
                autoFocus={true}
                onChange={this.onSearchInputChange}
              />

              {
                term ?
                  <Button
                    className={styles.clearLookupButton}
                    onPress={this.onClearGameLookupPress}
                  >
                    <Icon
                      name={icons.REMOVE}
                      size={14}
                    />
                  </Button> :
                  null
              }
            </div>
          </div>

          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error ?
              <div className={styles.emptyState}>
                <div className={styles.emptyTitle}>
                  {translate('FailedLoadingSearchResults')}
                </div>

                <p className={styles.error}>{getErrorMessage(error)}</p>

                <div>
                  {
                    error.status === 503 ?
                      <Link className={styles.emptyLink} to="/settings/metadata">
                        {translate('IgdbOpenMetadataSettings')}
                      </Link> :
                      <Link
                        className={styles.emptyLink}
                        to="https://github.com/ecatuogno1/Gamarr/wiki/troubleshooting#invalid-response-received-from-igdb"
                      >
                        {translate('WhySearchesCouldBeFailing')}
                      </Link>
                  }
                </div>
              </div> : null
          }

          {
            !isFetching && !error && !!items.length &&
              <div className={styles.searchResults}>
                <div className={styles.resultsLabel}>
                  {
                    items.length === 1 ?
                      translate('CountResult', { count: items.length }) :
                      translate('CountResults', { count: items.length })
                  }
                </div>

                {
                  items.map((item) => {
                    return (
                      <AddNewGameSearchResultConnector
                        key={item.igdbId}
                        {...item}
                      />
                    );
                  })
                }
              </div>
          }

          {
            !isFetching && !error && !items.length && !!term &&
              <div className={styles.emptyState}>
                <div className={styles.emptyTitle}>
                  {translate('CouldNotFindResults', { term })}
                </div>

                <div className={styles.emptyBody}>
                  {translate('YouCanAlsoSearch')}
                </div>

                <div>
                  <Link
                    className={styles.emptyLink}
                    to="https://github.com/ecatuogno1/Gamarr/wiki/faq#why-can-i-not-add-a-new-game-to-gamarr"
                  >
                    {translate('CantFindGame')}
                  </Link>
                </div>
              </div>
          }

          {
            term ?
              null :
              <div className={styles.emptyState}>
                <div className={styles.emptyTitle}>
                  {translate('AddNewMessage')}
                </div>

                <div className={styles.emptyBody}>
                  {translate('AddNewIgdbIdMessage')}
                </div>

                {
                  hasExistingGames ?
                    null :
                    <>
                      <div className={styles.emptyDivider} />

                      <div className={styles.emptyNoLibrary}>
                        {translate('HaveNotAddedGames')}
                      </div>

                      <Button
                        to="/add/import"
                        kind={kinds.PRIMARY}
                      >
                        {translate('ImportExistingGames')}
                      </Button>
                    </>
                }
              </div>
          }
        </PageContentBody>
      </PageContent>
    );
  }
}

AddNewGame.propTypes = {
  term: PropTypes.string,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isAdding: PropTypes.bool.isRequired,
  addError: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  hasExistingGames: PropTypes.bool.isRequired,
  onGameLookupChange: PropTypes.func.isRequired,
  onClearGameLookup: PropTypes.func.isRequired
};

export default AddNewGame;
