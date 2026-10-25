import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Navigation } from 'swiper/modules';
import { Swiper, SwiperSlide } from 'swiper/react';
import EditGameCollectionModal from 'Collection/Edit/EditGameCollectionModal';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import { icons } from 'Helpers/Props';
import QualityProfileName from 'Settings/Profiles/Quality/QualityProfileName';
import translate from 'Utilities/String/translate';
import CollectionGameConnector from './CollectionGameConnector';
import CollectionGameLabelConnector from './CollectionGameLabelConnector';
import styles from './CollectionOverview.css';

// Import Swiper styles
import 'swiper/css';
import 'swiper/css/navigation';

// Vertical layout of a v5 collection row; must match CollectionOverview.css.
export const ROW_VERTICAL_PADDING = 16;
export const ROW_BORDER = 1;
export const ROW_GAP = 10;
export const HEADER_HEIGHT = 32;
export const CHIP_STRIP_HEIGHT = 22;
export const OVERVIEW_LINES = 2;
export const OVERVIEW_HEIGHT = 44;
export const LABELS_HEIGHT = 64;

class CollectionOverview extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditCollectionModalOpen: false,
      isNewAddGameModalOpen: false
    };
  }

  //
  // Control

  setSliderPrevRef = (ref) => {
    this._swiperPrevRef = ref;
  };

  setSliderNextRef = (ref) => {
    this._swiperNextRef = ref;
  };

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddGameModalOpen: true });
  };

  onEditCollectionPress = () => {
    this.setState({ isEditCollectionModalOpen: true });
  };

  onEditCollectionModalClose = () => {
    this.setState({ isEditCollectionModalOpen: false });
  };

  onAddGameModalClose = () => {
    this.setState({ isNewAddGameModalOpen: false });
  };

  onSelectPress = (event) => {
    const {
      id,
      isSelected,
      onSelectedChange
    } = this.props;

    onSelectedChange({
      id,
      value: !isSelected,
      shiftKey: !!event?.nativeEvent?.shiftKey
    });
  };

  //
  // Render

  render() {
    const {
      monitored,
      qualityProfileId,
      rootFolderPath,
      genres,
      id,
      title,
      games,
      overview,
      missingGames,
      posterHeight,
      posterWidth,
      isSmallScreen,
      isSelected,
      onMonitorTogglePress
    } = this.props;

    const {
      showDetails,
      showOverview,
      showPosters,
      detailedProgressBar
    } = this.props.overviewOptions;

    const {
      isEditCollectionModalOpen
    } = this.state;

    return (
      <div>
        <div className={styles.content}>
          <div className={styles.titleRow}>
            <Link
              className={styles.selectButton}
              aria-label={translate('Select')}
              onPress={this.onSelectPress}
            >
              <Icon
                className={isSelected ? styles.selected : styles.unselected}
                name={isSelected ? icons.CHECK_CIRCLE : icons.CIRCLE}
                size={18}
              />
            </Link>

            <MonitorToggleButton
              className={styles.monitorToggleButton}
              monitored={monitored}
              size={isSmallScreen ? 16 : 18}
              onPress={onMonitorTogglePress}
            />

            <div className={styles.title} title={title}>
              {title}
            </div>

            <div className={styles.actions}>
              <IconButton
                className={styles.action}
                name={icons.EDIT}
                title={translate('EditCollection')}
                onPress={this.onEditCollectionPress}
              />

              {
                showPosters ?
                  <>
                    <span ref={this.setSliderPrevRef} className={styles.navigationButton}>
                      <IconButton
                        className={styles.action}
                        name={icons.ARROW_LEFT}
                        title={translate('ScrollGames')}
                      />
                    </span>

                    <span ref={this.setSliderNextRef} className={styles.navigationButton}>
                      <IconButton
                        className={styles.action}
                        name={icons.ARROW_RIGHT}
                        title={translate('ScrollGames')}
                      />
                    </span>
                  </> :
                  null
              }
            </div>
          </div>

          {
            showDetails ?
              <div className={styles.chipStrip}>
                <span
                  className={classNames(
                    styles.chip,
                    styles.chipStatus,
                    missingGames > 0 ? styles.missing : styles.complete
                  )}
                >
                  {translate('CountMissingGamesFromLibrary', { count: missingGames })}
                </span>

                {
                  isSmallScreen ?
                    null :
                    <span className={styles.chip}>
                      <QualityProfileName qualityProfileId={qualityProfileId} />
                    </span>
                }

                {
                  !isSmallScreen && genres.length ?
                    <span className={styles.chip}>
                      {genres.slice(0, 3).join(', ')}
                    </span> :
                    null
                }

                {
                  isSmallScreen ?
                    null :
                    <span className={styles.chipPath} title={rootFolderPath}>
                      {rootFolderPath}
                    </span>
                }
              </div> :
              null
          }

          {
            showOverview ?
              <div
                className={styles.synopsis}
                style={{ WebkitLineClamp: OVERVIEW_LINES }}
              >
                {overview}
              </div> :
              null
          }

          {
            showPosters ?
              <div className={styles.sliderContainer}>
                <Swiper
                  slidesPerView='auto'
                  spaceBetween={12}
                  slidesPerGroup={3}
                  loop={false}
                  className="mySwiper"
                  modules={[Navigation]}
                  onInit={(swiper) => {
                    swiper.params.navigation.prevEl = this._swiperPrevRef;
                    swiper.params.navigation.nextEl = this._swiperNextRef;
                    swiper.navigation.init();
                    swiper.navigation.update();
                  }}
                >
                  {games.map((game) => (
                    <SwiperSlide key={game.igdbId} style={{ width: posterWidth }}>
                      <CollectionGameConnector
                        key={game.igdbId}
                        posterWidth={posterWidth}
                        posterHeight={posterHeight}
                        detailedProgressBar={detailedProgressBar}
                        collectionId={id}
                        {...game}
                      />
                    </SwiperSlide>
                  ))}
                </Swiper>
              </div> :
              <div className={styles.labelsContainer}>
                {games.map((game) => (
                  <CollectionGameLabelConnector
                    key={game.igdbId}
                    collectionId={id}
                    {...game}
                  />
                ))}
              </div>
          }
        </div>

        <EditGameCollectionModal
          isOpen={isEditCollectionModalOpen}
          collectionId={id}
          onModalClose={this.onEditCollectionModalClose}
        />
      </div>
    );
  }
}

CollectionOverview.propTypes = {
  id: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  minimumAvailability: PropTypes.string.isRequired,
  searchOnAdd: PropTypes.bool.isRequired,
  rootFolderPath: PropTypes.string.isRequired,
  igdbId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  overview: PropTypes.string.isRequired,
  games: PropTypes.arrayOf(PropTypes.object).isRequired,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  missingGames: PropTypes.number.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  rowHeight: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  posterWidth: PropTypes.number.isRequired,
  overviewOptions: PropTypes.object.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  onMonitorTogglePress: PropTypes.func.isRequired,
  onSelectedChange: PropTypes.func.isRequired
};

export default CollectionOverview;
