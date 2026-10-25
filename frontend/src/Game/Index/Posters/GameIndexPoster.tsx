import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import { useSelector } from 'react-redux';
import CriticRating from 'Components/CriticRating';
import Icon from 'Components/Icon';
import IgdbRating from 'Components/IgdbRating';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import Popover from 'Components/Tooltip/Popover';
import GameDetailsLinks from 'Game/Details/GameDetailsLinks';
import GamePoster from 'Game/GamePoster';
import GameIndexTags from 'Game/Index/GameIndexTags';
import GameIndexPosterSelect from 'Game/Index/Select/GameIndexPosterSelect';
import GamePlatformBadges from 'Game/Title/GamePlatformBadges';
import { getMonitoredText, getTitleLink } from 'Game/Title/gameTitle';
import GameTitleModals from 'Game/Title/GameTitleModals';
import GameTitleProgressBar from 'Game/Title/GameTitleProgressBar';
import useGameTitleIndexItem from 'Game/Title/useGameTitleIndexItem';
import { icons } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import formatDate from 'Utilities/Date/formatDate';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import translate from 'Utilities/String/translate';
import GameIndexPosterInfo from './GameIndexPosterInfo';
import selectPosterOptions from './selectPosterOptions';
import styles from './GameIndexPoster.css';

interface GameIndexPosterProps {
  gameIds: number[];
  sortKey: string;
  isSelectMode: boolean;
  posterWidth: number;
  posterHeight: number;
}

function GameIndexPoster(props: GameIndexPosterProps) {
  const { gameIds, sortKey, isSelectMode, posterWidth, posterHeight } = props;

  const {
    summary,
    qualityProfileNames,
    isRefreshing: isRefreshingGame,
    isSearching: isSearchingGame,
    isEditModalOpen: isEditGameModalOpen,
    isDeleteModalOpen: isDeleteGameModalOpen,
    onRefreshPress,
    onSearchPress,
    onEditPress: onEditGamePress,
    onEditModalClose: onEditGameModalClose,
    onDeletePress: onDeleteGamePress,
    onDeleteModalClose: onDeleteGameModalClose,
  } = useGameTitleIndexItem(gameIds);

  const {
    detailedProgressBar,
    showPlatform,
    showTitle,
    showMonitored,
    showQualityProfile,
    showReleaseDate,
    showIgdbRating,
    showCriticRating,
    showTags,
    showSearchAction,
  } = useSelector(selectPosterOptions);

  const { showRelativeDates, shortDateFormat, longDateFormat, timeFormat } =
    useSelector(createUISettingsSelector());

  const [hasPosterError, setHasPosterError] = useState(false);

  const onPosterLoadError = useCallback(() => {
    setHasPosterError(true);
  }, [setHasPosterError]);

  const onPosterLoad = useCallback(() => {
    setHasPosterError(false);
  }, [setHasPosterError]);

  if (!summary) {
    return null;
  }

  const {
    title,
    status,
    images,
    igdbId,
    igdbSlug,
    youTubeTrailerId,
    added,
    year,
    releaseDate,
    path,
    ratings,
    originalTitle,
    originalLanguage,
    tags = [],
  } = summary.primary;

  const qualityProfileName = qualityProfileNames.join(', ');
  const platformNames = summary.versions
    .map((v) => v.game.platform?.name)
    .filter((name) => !!name)
    .join(', ');

  const link = getTitleLink(igdbId);

  const elementStyle = {
    width: `${posterWidth}px`,
    height: `${posterHeight}px`,
  };

  const hasQualityProfile = showQualityProfile && !!qualityProfileName;

  return (
    <div className={styles.content}>
      <div className={styles.posterContainer} title={title}>
        {isSelectMode ? (
          <GameIndexPosterSelect gameId={igdbId} titleSlug={`${igdbId}`} />
        ) : null}

        <div className={styles.controls}>
          <SpinnerIconButton
            className={styles.action}
            name={icons.REFRESH}
            title={translate('RefreshGame')}
            isSpinning={isRefreshingGame}
            tabIndex={-1}
            onPress={onRefreshPress}
          />

          {showSearchAction ? (
            <SpinnerIconButton
              className={styles.action}
              name={icons.SEARCH}
              title={translate('SearchForGame')}
              isSpinning={isSearchingGame}
              tabIndex={-1}
              onPress={onSearchPress}
            />
          ) : null}

          <IconButton
            className={styles.action}
            name={icons.EDIT}
            title={translate('EditGame')}
            aria-label={translate('EditGame')}
            tabIndex={-1}
            onPress={onEditGamePress}
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

        {status === 'deleted' ? (
          <div
            className={classNames(styles.status, styles.deleted)}
            title={translate('Deleted')}
          >
            <span className={styles.statusLabel}>{translate('Deleted')}</span>
          </div>
        ) : null}

        {showPlatform ? (
          <GamePlatformBadges
            className={styles.platforms}
            versions={summary.versions}
          />
        ) : null}

        <Link className={styles.link} style={elementStyle} to={link}>
          <GamePoster
            style={elementStyle}
            images={images}
            size={250}
            lazy={false}
            overflow={true}
            onError={onPosterLoadError}
            onLoad={onPosterLoad}
          />

          {hasPosterError ? (
            <div className={styles.overlayTitle}>{title}</div>
          ) : null}
        </Link>
      </div>

      <GameTitleProgressBar
        summary={summary}
        width={posterWidth}
        detailedProgressBar={detailedProgressBar}
        bottomRadius={false}
      />

      {showTitle ? (
        <div className={styles.title} title={title}>
          {title}
        </div>
      ) : null}

      {showMonitored || hasQualityProfile ? (
        <div className={styles.meta}>
          {showMonitored ? getMonitoredText(summary) : null}

          {showMonitored && hasQualityProfile ? (
            <span className={styles.metaSep}> · </span>
          ) : null}

          {hasQualityProfile ? qualityProfileName : null}
        </div>
      ) : null}

      {showReleaseDate && releaseDate ? (
        <div
          className={styles.nextAiring}
          title={`${translate('ReleaseDate')}: ${formatDate(
            releaseDate,
            longDateFormat
          )}`}
        >
          <Icon className={styles.lineIcon} name={icons.CALENDAR} size={12} />
          {getRelativeDate({
            date: releaseDate,
            shortDateFormat,
            showRelativeDates,
            timeFormat,
            timeForToday: false,
          })}
        </div>
      ) : null}

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

      {showTags && tags.length ? (
        <div className={styles.tags}>
          <GameIndexTags tags={tags} />
        </div>
      ) : null}

      <GameIndexPosterInfo
        platform={platformNames}
        qualityProfile={{ name: qualityProfileName }}
        added={added}
        year={year}
        showQualityProfile={showQualityProfile}
        showReleaseDate={showReleaseDate}
        showRelativeDates={showRelativeDates}
        shortDateFormat={shortDateFormat}
        longDateFormat={longDateFormat}
        timeFormat={timeFormat}
        releaseDate={releaseDate}
        ratings={ratings}
        sizeOnDisk={summary.sizeOnDisk}
        sortKey={sortKey}
        path={path}
        originalTitle={originalTitle}
        originalLanguage={originalLanguage}
        tags={tags}
        showIgdbRating={showIgdbRating}
        showCriticRating={showCriticRating}
        showTags={showTags}
      />

      <GameTitleModals
        gameIds={gameIds}
        isEditModalOpen={isEditGameModalOpen}
        isDeleteModalOpen={isDeleteGameModalOpen}
        onEditModalClose={onEditGameModalClose}
        onDeletePress={onDeleteGamePress}
        onDeleteModalClose={onDeleteGameModalClose}
      />
    </div>
  );
}

export default GameIndexPoster;
