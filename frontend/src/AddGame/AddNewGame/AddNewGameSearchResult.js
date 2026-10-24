import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CriticRating from 'Components/CriticRating';
import Icon from 'Components/Icon';
import IgdbRating from 'Components/IgdbRating';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import Tooltip from 'Components/Tooltip/Tooltip';
import GameDetailsLinks from 'Game/Details/GameDetailsLinks';
import GameStatusLabel from 'Game/Details/GameStatusLabel';
import GameGenres from 'Game/GameGenres';
import GamePoster from 'Game/GamePoster';
import GameIndexProgressBar from 'Game/Index/ProgressBar/GameIndexProgressBar';
import { icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AddNewGameModal from './AddNewGameModal';
import styles from './AddNewGameSearchResult.css';

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
      titleSlug,
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
      existingTitleSlug,
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

    const linkProps = isExistingGame ? { to: `/game/${existingTitleSlug || titleSlug}` } : { onPress: this.onPress };
    const posterWidth = 167;
    const posterHeight = 250;

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`
    };

    return (
      <div className={styles.searchResult}>
        <Link
          className={styles.underlay}
          {...linkProps}
        />

        <div className={styles.overlay}>
          {
            isSmallScreen ?
              null :
              <div>
                <div className={styles.posterContainer}>
                  <GamePoster
                    className={styles.poster}
                    style={elementStyle}
                    images={images}
                    size={250}
                    overflow={true}
                    lazy={false}
                  />
                </div>

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
                <div>
                  {
                    isExistingGame &&
                      <Icon
                        className={styles.alreadyExistsIcon}
                        name={icons.CHECK_CIRCLE}
                        size={36}
                        title={translate('AlreadyInYourLibrary')}
                      />
                  }

                  {
                    isExcluded &&
                      <Icon
                        className={styles.exclusionIcon}
                        name={icons.DANGER}
                        size={36}
                        title={translate('GameIsOnImportExclusionList')}
                      />
                  }
                </div>
              </div>
            </div>

            <div className={styles.platforms}>
              {
                platformBadges.length ?
                  platformBadges.map((badge) => (
                    <span
                      key={badge.id}
                      className={badge.isAdded ? styles.addedPlatform : styles.platform}
                      title={badge.isAdded ? translate('GameIsInLibraryOnPlatform', { platform: badge.name }) : badge.name}
                    >
                      {badge.isAdded ? <Icon name={icons.CHECK} size={12} /> : null} {badge.shortName}
                    </span>
                  )) :
                  (
                    !!platform &&
                      <span className={styles.platform}>
                        {platform.shortName}
                      </span>
                  )
              }
            </div>

            {
              libraryPlatformNames.length && !isExistingGame ?
                <div className={styles.inLibrary}>
                  {translate('AlreadyInLibraryOn', { platforms: libraryPlatformNames.join(', ') })}
                </div> :
                null
            }

            <div>
              <Label size={sizes.LARGE}>
                <IgdbRating
                  ratings={ratings}
                  iconSize={13}
                />
              </Label>

              {
                ratings.critic ?
                  <Label size={sizes.LARGE}>
                    <CriticRating
                      ratings={ratings}
                      iconSize={13}
                    />
                  </Label> :
                  null
              }

              {
                originalLanguage?.name ?
                  <Label size={sizes.LARGE}>
                    <Icon
                      name={icons.LANGUAGE}
                      size={13}
                    />
                    <span className={styles.originalLanguage}>
                      {originalLanguage.name}
                    </span>
                  </Label> :
                  null
              }

              {
                developers.length ?
                  <Label size={sizes.LARGE}>
                    <Icon
                      name={icons.STUDIO}
                      size={13}
                    />
                    <span className={styles.studio}>
                      {developers.join(', ')}
                    </span>
                  </Label> :
                  null
              }

              {
                genres.length > 0 ?
                  <Label size={sizes.LARGE}>
                    <Icon
                      name={icons.GENRE}
                      size={13}
                    />
                    <GameGenres className={styles.genres} genres={genres} />
                  </Label> :
                  null
              }

              <Tooltip
                anchor={
                  <Label
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.EXTERNAL_LINK}
                      size={13}
                    />

                    <span className={styles.links}>
                      {translate('Links')}
                    </span>
                  </Label>
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
  existingTitleSlug: PropTypes.string,
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
