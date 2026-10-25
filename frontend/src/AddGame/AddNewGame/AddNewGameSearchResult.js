import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CriticRating from 'Components/CriticRating';
import Icon from 'Components/Icon';
import IgdbRating from 'Components/IgdbRating';
import Link from 'Components/Link/Link';
import MetadataAttribution from 'Components/MetadataAttribution';
import Tooltip from 'Components/Tooltip/Tooltip';
import GameDetailsLinks from 'Game/Details/GameDetailsLinks';
import GameStatusLabel from 'Game/Details/GameStatusLabel';
import GameGenres from 'Game/GameGenres';
import GamePoster from 'Game/GamePoster';
import GameIndexProgressBar from 'Game/Index/ProgressBar/GameIndexProgressBar';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AddNewGameModal from './AddNewGameModal';
import styles from './AddNewGameSearchResult.css';

function getStatusChip(status) {
  switch (status) {
    case 'announced':
      return { dot: styles.chipDotUpcoming, label: translate('Announced') };
    case 'tba':
      return { dot: styles.chipDotUpcoming, label: translate('Tba') };
    case 'released':
      return { dot: styles.chipDotFinished, label: translate('Released') };
    case 'deleted':
      return { dot: styles.chipDotEnded, label: translate('Deleted') };
    default:
      return null;
  }
}

class AddNewGameSearchResult extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNewAddGameModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    if (!prevProps.isExistingGame && this.props.isExistingGame) {
      this.onAddGameModalClose();
    }
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddGameModalOpen: true });
  };

  onAddGameModalClose = () => {
    this.setState({ isNewAddGameModalOpen: false });
  };

  onExternalLinkPress = (event) => {
    event.stopPropagation();
  };

  //
  // Render

  render() {
    const {
      igdbId,
      igdbSlug,
      youTubeTrailerId,
      title,
      year,
      platform,
      availablePlatforms,
      addedPlatformIds,
      platformBadges,
      libraryPlatformNames,
      developers,
      originalLanguage,
      genres,
      status,
      overview,
      ratings,
      folder,
      images,
      existingGameId,
      isExistingGame,
      isExcluded,
      isSmallScreen,
      monitored,
      isAvailable,
      gameFile
    } = this.props;

    const {
      isNewAddGameModalOpen
    } = this.state;
    const hasGameFile = !!gameFile;

    const linkProps = isExistingGame ? { to: `/game/${igdbId}` } : { onPress: this.onPress };
    const posterWidth = 114;
    const statusChip = getStatusChip(status);

    return (
      <div
        className={classNames(
          styles.searchResult,
          isExcluded && styles.searchResultExcluded
        )}
      >
        <Link
          className={styles.underlay}
          {...linkProps}
        />

        <div className={styles.overlay}>
          {
            isSmallScreen ?
              null :
              <div className={styles.posterContainer}>
                <GamePoster
                  className={styles.poster}
                  images={images}
                  size={250}
                  overflow={true}
                  lazy={false}
                />

                {
                  isExistingGame &&
                    <GameIndexProgressBar
                      gameId={existingGameId}
                      gameFile={gameFile}
                      monitored={monitored}
                      hasFile={hasGameFile}
                      status={status}
                      width={posterWidth}
                      detailedProgressBar={true}
                      isAvailable={isAvailable}
                    />
                }
              </div>
          }

          <div className={styles.content}>
            <div className={styles.titleRow}>
              <div className={styles.titleContainer}>
                <div className={styles.title}>
                  {title}

                  {
                    !title.contains(year) && !!year ?
                      <span className={styles.year}>
                        ({year})
                      </span> :
                      null
                  }
                </div>
              </div>

              <div className={styles.icons}>
                {
                  isExistingGame &&
                    <Icon
                      className={styles.alreadyExistsIcon}
                      name={icons.CHECK_CIRCLE}
                      size={24}
                      title={translate('AlreadyInYourLibrary')}
                    />
                }

                {
                  isExcluded &&
                    <Icon
                      className={styles.excludedIcon}
                      name={icons.DANGER}
                      size={24}
                      title={translate('GameIsOnImportExclusionList')}
                    />
                }

                <Tooltip
                  anchor={
                    <span
                      className={styles.externalLink}
                      aria-label={translate('Links')}
                    >
                      <Icon
                        name={icons.EXTERNAL_LINK}
                        size={16}
                      />
                    </span>
                  }
                  tooltip={
                    <GameDetailsLinks
                      igdbId={igdbId}
                      igdbSlug={igdbSlug}
                      youTubeTrailerId={youTubeTrailerId}
                    />
                  }
                  canFlip={true}
                  kind={kinds.INVERSE}
                  position={tooltipPositions.TOP}
                />
              </div>
            </div>

            {
              platformBadges.length || platform ?
                <div className={styles.platforms}>
                  {
                    platformBadges.length ?
                      platformBadges.map((badge) => (
                        <span
                          key={badge.id}
                          className={badge.isAdded ? styles.addedPlatform : styles.platform}
                          title={badge.isAdded ? translate('GameIsInLibraryOnPlatform', { platform: badge.name }) : badge.name}
                        >
                          {badge.isAdded ? <Icon name={icons.CHECK} size={11} /> : null} {badge.shortName}
                        </span>
                      )) :
                      <span className={styles.platform}>
                        {platform.shortName}
                      </span>
                  }
                </div> :
                null
            }

            {
              libraryPlatformNames.length && !isExistingGame ?
                <div className={styles.inLibrary}>
                  {translate('AlreadyInLibraryOn', { platforms: libraryPlatformNames.join(', ') })}
                </div> :
                null
            }

            <div className={styles.chips}>
              {
                statusChip ?
                  <span className={styles.chip}>
                    <span className={classNames(styles.chipDot, statusChip.dot)} />
                    {statusChip.label}
                  </span> :
                  null
              }

              {
                ratings.igdb ?
                  <span className={styles.chip}>
                    <IgdbRating
                      ratings={ratings}
                      iconSize={11}
                    />
                  </span> :
                  null
              }

              {
                ratings.critic ?
                  <span className={styles.chip}>
                    <CriticRating
                      ratings={ratings}
                      iconSize={11}
                    />
                  </span> :
                  null
              }

              {
                developers.length ?
                  <span className={styles.chip} title={translate('Developers')}>
                    {developers.join(', ')}
                  </span> :
                  null
              }

              {
                originalLanguage?.name ?
                  <span className={styles.chip}>
                    {originalLanguage.name}
                  </span> :
                  null
              }

              {
                genres.length > 0 ?
                  <span className={styles.chip}>
                    <GameGenres className={styles.genres} genres={genres} />
                  </span> :
                  null
              }

              {
                isExistingGame && isSmallScreen &&
                  <GameStatusLabel
                    gameId={existingGameId}
                    monitored={monitored}
                    isAvailable={isAvailable}
                    hasGameFiles={hasGameFile}
                    status={status}
                    useLabel={true}
                  />
              }
            </div>

            <div className={styles.overview}>
              {overview}
            </div>

            <MetadataAttribution />
          </div>
        </div>

        <AddNewGameModal
          isOpen={isNewAddGameModalOpen && !isExistingGame}
          igdbId={igdbId}
          title={title}
          year={year}
          platformId={platform ? platform.id : 0}
          availablePlatforms={availablePlatforms}
          addedPlatformIds={addedPlatformIds}
          overview={overview}
          folder={folder}
          images={images}
          onModalClose={this.onAddGameModalClose}
        />
      </div>
    );
  }
}

AddNewGameSearchResult.propTypes = {
  igdbId: PropTypes.number.isRequired,
  igdbSlug: PropTypes.string,
  youTubeTrailerId: PropTypes.string,
  title: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  platform: PropTypes.object,
  availablePlatforms: PropTypes.arrayOf(PropTypes.number),
  addedPlatformIds: PropTypes.arrayOf(PropTypes.number),
  platformBadges: PropTypes.arrayOf(PropTypes.object).isRequired,
  libraryPlatformNames: PropTypes.arrayOf(PropTypes.string).isRequired,
  developers: PropTypes.arrayOf(PropTypes.string),
  originalLanguage: PropTypes.object,
  genres: PropTypes.arrayOf(PropTypes.string),
  status: PropTypes.string.isRequired,
  overview: PropTypes.string,
  ratings: PropTypes.object.isRequired,
  folder: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  existingGameId: PropTypes.number,
  isExistingGame: PropTypes.bool.isRequired,
  isExcluded: PropTypes.bool,
  isSmallScreen: PropTypes.bool.isRequired,
  monitored: PropTypes.bool.isRequired,
  isAvailable: PropTypes.bool.isRequired,
  gameFile: PropTypes.object
};

AddNewGameSearchResult.defaultProps = {
  genres: [],
  developers: [],
  availablePlatforms: [],
  addedPlatformIds: [],
  isExcluded: false
};

export default AddNewGameSearchResult;
