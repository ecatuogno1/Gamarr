import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CriticRating from 'Components/CriticRating';
import Icon from 'Components/Icon';
import IgdbRating from 'Components/IgdbRating';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import Popover from 'Components/Tooltip/Popover';
import AddNewDiscoverGameModal from 'DiscoverGame/AddNewDiscoverGameModal';
import ExcludeGameModal from 'DiscoverGame/Exclusion/ExcludeGameModal';
import GameDetailsLinks from 'Game/Details/GameDetailsLinks';
import GamePoster from 'Game/GamePoster';
import LibraryPlatformBadges from 'Game/Title/LibraryPlatformBadges';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import DiscoverGamePosterInfo from './DiscoverGamePosterInfo';
import styles from './DiscoverGamePoster.css';

class DiscoverGamePoster extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false,
      isNewAddGameModalOpen: false,
      isExcludeGameModalOpen: false
    };
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddGameModalOpen: true });
  };

  onAddGameModalClose = () => {
    this.setState({ isNewAddGameModalOpen: false });
  };

  onExcludeGamePress = () => {
    this.setState({ isExcludeGameModalOpen: true });
  };

  onExcludeGameModalClose = () => {
    this.setState({ isExcludeGameModalOpen: false });
  };

  onPosterLoad = () => {
    if (this.state.hasPosterError) {
      this.setState({ hasPosterError: false });
    }
  };

  onPosterLoadError = () => {
    if (!this.state.hasPosterError) {
      this.setState({ hasPosterError: true });
    }
  };

  onSelectPress = (event) => {
    const {
      igdbId,
      isSelected,
      onSelectedChange
    } = this.props;

    onSelectedChange({
      id: igdbId,
      value: !isSelected,
      shiftKey: !!event?.nativeEvent?.shiftKey
    });
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
      overview,
      folder,
      images,
      posterWidth,
      posterHeight,
      showTitle,
      showIgdbRating,
      showCriticRating,
      ratings,
      isExisting,
      isExcluded,
      isSelected,
      showRelativeDates,
      shortDateFormat,
      timeFormat,
      ...otherProps
    } = this.props;

    const {
      hasPosterError,
      isNewAddGameModalOpen,
      isExcludeGameModalOpen
    } = this.state;

    const linkProps = isExisting ? { to: `/game/${igdbId}` } : { onPress: this.onPress };

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`
    };

    return (
      <div className={styles.content}>
        <div className={styles.posterContainer} title={title}>
          <Link
            className={styles.selectButton}
            aria-label={translate('Select')}
            onPress={this.onSelectPress}
          >
            <Icon
              className={isSelected ? styles.selected : styles.unselected}
              name={isSelected ? icons.CHECK_CIRCLE : icons.CIRCLE}
              size={20}
            />
          </Link>

          <div className={styles.controls}>
            <IconButton
              className={styles.action}
              name={icons.REMOVE}
              title={isExcluded ? translate('GameAlreadyExcluded') : translate('ExcludeGame')}
              tabIndex={-1}
              isDisabled={isExcluded}
              onPress={this.onExcludeGamePress}
            />

            <Popover
              className={styles.action}
              anchor={<Icon name={icons.EXTERNAL_LINK} />}
              title={translate('Links')}
              body={
                <GameDetailsLinks
                  igdbId={igdbId}
                  igdbSlug={igdbSlug}
                  youTubeTrailerId={youTubeTrailerId}
                />
              }
            />
          </div>

          {
            isExisting || isExcluded ?
              <div
                className={classNames(
                  styles.status,
                  isExisting ? styles.existing : styles.excluded
                )}
              >
                <span className={styles.statusLabel}>
                  {isExisting ? translate('Existing') : translate('Excluded')}
                </span>
              </div> :
              null
          }

          {
            isExisting &&
              <LibraryPlatformBadges
                className={styles.platforms}
                igdbId={igdbId}
              />
          }

          <Link
            className={styles.link}
            style={elementStyle}
            {...linkProps}
          >
            <GamePoster
              style={elementStyle}
              images={images}
              size={250}
              lazy={false}
              overflow={true}
              onError={this.onPosterLoadError}
              onLoad={this.onPosterLoad}
            />

            {
              hasPosterError &&
                <div className={styles.overlayTitle}>
                  {title}
                </div>
            }
          </Link>
        </div>

        {showTitle ?
          <div className={styles.title} title={title}>
            {title}
          </div> :
          null}

        {showIgdbRating && !!ratings.igdb ? (
          <div className={styles.meta}>
            <IgdbRating ratings={ratings} iconSize={12} />
          </div>
        ) : null}

        {showCriticRating && !!ratings.critic ? (
          <div className={styles.meta}>
            <CriticRating ratings={ratings} iconSize={12} />
          </div>
        ) : null}

        <DiscoverGamePosterInfo
          showRelativeDates={showRelativeDates}
          shortDateFormat={shortDateFormat}
          timeFormat={timeFormat}
          ratings={ratings}
          showIgdbRating={showIgdbRating}
          showCriticRating={showCriticRating}
          {...otherProps}
        />

        <AddNewDiscoverGameModal
          isOpen={isNewAddGameModalOpen && !isExisting}
          igdbId={igdbId}
          title={title}
          year={year}
          overview={overview}
          folder={folder}
          images={images}
          onModalClose={this.onAddGameModalClose}
        />

        <ExcludeGameModal
          isOpen={isExcludeGameModalOpen}
          igdbId={igdbId}
          title={title}
          year={year}
          onModalClose={this.onExcludeGameModalClose}
        />
      </div>
    );
  }
}

DiscoverGamePoster.propTypes = {
  igdbId: PropTypes.number.isRequired,
  igdbSlug: PropTypes.string,
  youTubeTrailerId: PropTypes.string,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  overview: PropTypes.string.isRequired,
  folder: PropTypes.string.isRequired,
  status: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  showTitle: PropTypes.bool.isRequired,
  showIgdbRating: PropTypes.bool.isRequired,
  showCriticRating: PropTypes.bool.isRequired,
  ratings: PropTypes.object.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  isExisting: PropTypes.bool.isRequired,
  isExcluded: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default DiscoverGamePoster;
