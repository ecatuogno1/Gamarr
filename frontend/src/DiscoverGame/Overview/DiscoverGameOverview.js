import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
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
import DiscoverGameOverviewInfo from './DiscoverGameOverviewInfo';
import styles from './DiscoverGameOverview.css';

// Vertical chrome around a v5 overview row; must match .content in the CSS.
export const ROW_VERTICAL_PADDING = 16;
export const ROW_BORDER = 1;

const SYNOPSIS_FONT_SIZE = 13.5;
const SYNOPSIS_LINE_HEIGHT = 1.6;
const TITLE_HEIGHT = 38;
const CHIP_STRIP_HEIGHT = 28;

class DiscoverGameOverview extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
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
      folder,
      year,
      overview,
      images,
      lists,
      posterWidth,
      posterHeight,
      rowHeight,
      isExisting,
      isExcluded,
      isRecommendation,
      isPopular,
      isTrending,
      isSelected,
      overviewOptions,
      ...otherProps
    } = this.props;

    const {
      isNewAddGameModalOpen,
      isExcludeGameModalOpen
    } = this.state;

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`
    };

    const linkProps = isExisting ? { to: `/game/${igdbId}` } : { onPress: this.onPress };

    const contentHeight = rowHeight - (ROW_VERTICAL_PADDING * 2 + ROW_BORDER);
    const synopsisLines = Math.max(
      1,
      Math.floor(
        (contentHeight - TITLE_HEIGHT - CHIP_STRIP_HEIGHT) /
          (SYNOPSIS_FONT_SIZE * SYNOPSIS_LINE_HEIGHT)
      )
    );

    return (
      <div>
        <div className={styles.content}>
          <div className={styles.poster}>
            <div className={styles.posterContainer}>
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

              <Link
                className={styles.link}
                style={elementStyle}
                {...linkProps}
              >
                <GamePoster
                  className={styles.posterImage}
                  style={elementStyle}
                  images={images}
                  size={250}
                  lazy={false}
                  overflow={true}
                />
              </Link>
            </div>
          </div>

          <div className={styles.info} style={{ maxHeight: contentHeight }}>
            <div className={styles.titleRow}>
              <Link
                className={styles.title}
                {...linkProps}
              >
                {title}

                {
                  year && !title.includes(String(year)) ?
                    <span className={styles.year}>({year})</span> :
                    null
                }
                {
                  isExisting ?
                    <LibraryPlatformBadges
                      className={styles.platforms}
                      igdbId={igdbId}
                    /> : null
                }
              </Link>

              <div className={styles.actions}>
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

                <IconButton
                  className={styles.action}
                  name={icons.REMOVE}
                  title={isExcluded ? translate('GameAlreadyExcluded') : translate('ExcludeGame')}
                  isDisabled={isExcluded}
                  onPress={this.onExcludeGamePress}
                />
              </div>
            </div>

            <Link
              className={styles.synopsis}
              style={{ WebkitLineClamp: synopsisLines }}
              {...linkProps}
            >
              {overview}
            </Link>

            <DiscoverGameOverviewInfo
              year={year}
              lists={lists}
              isExisting={isExisting}
              isExcluded={isExcluded}
              isRecommendation={isRecommendation}
              isPopular={isPopular}
              isTrending={isTrending}
              {...overviewOptions}
              {...otherProps}
            />
          </div>
        </div>

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

DiscoverGameOverview.propTypes = {
  igdbId: PropTypes.number.isRequired,
  igdbSlug: PropTypes.string,
  youTubeTrailerId: PropTypes.string,
  title: PropTypes.string.isRequired,
  folder: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  overview: PropTypes.string.isRequired,
  status: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  rowHeight: PropTypes.number.isRequired,
  overviewOptions: PropTypes.object.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isExisting: PropTypes.bool.isRequired,
  isExcluded: PropTypes.bool.isRequired,
  isRecommendation: PropTypes.bool.isRequired,
  isPopular: PropTypes.bool.isRequired,
  isTrending: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  lists: PropTypes.arrayOf(PropTypes.number).isRequired,
  onSelectedChange: PropTypes.func.isRequired
};

DiscoverGameOverview.defaultProps = {
  lists: []
};

export default DiscoverGameOverview;
