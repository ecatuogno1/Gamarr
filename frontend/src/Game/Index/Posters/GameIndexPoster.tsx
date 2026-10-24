import React, { useCallback, useState } from 'react';
import { useSelector } from 'react-redux';
import CriticRating from 'Components/CriticRating';
import GameTagList from 'Components/GameTagList';
import Icon from 'Components/Icon';
import IgdbRating from 'Components/IgdbRating';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import Popover from 'Components/Tooltip/Popover';
import GameDetailsLinks from 'Game/Details/GameDetailsLinks';
import GamePoster from 'Game/GamePoster';
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

  return (
    <div className={styles.content}>
      <div className={styles.posterContainer} title={title}>
        {isSelectMode ? (
          <GameIndexPosterSelect gameId={igdbId} titleSlug={`${igdbId}`} />
        ) : null}

        <Label className={styles.controls}>
          <SpinnerIconButton
            name={icons.REFRESH}
            title={translate('RefreshGame')}
            isSpinning={isRefreshingGame}
            onPress={onRefreshPress}
          />

          {showSearchAction ? (
            <SpinnerIconButton
              className={styles.action}
              name={icons.SEARCH}
              title={translate('SearchForGame')}
              isSpinning={isSearchingGame}
              onPress={onSearchPress}
            />
          ) : null}

          <IconButton
            name={icons.EDIT}
            title={translate('EditGame')}
            onPress={onEditGamePress}
          />

          <span className={styles.externalLinks}>
            <Popover
              anchor={<Icon name={icons.EXTERNAL_LINK} size={12} />}
              title={translate('Links')}
              body={
                <GameDetailsLinks
                  igdbId={igdbId}
                  igdbSlug={igdbSlug}
                  youTubeTrailerId={youTubeTrailerId}
                />
              }
            />
          </span>
        </Label>

        {status === 'deleted' ? (
          <div className={styles.deleted} title={translate('Deleted')} />
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

      {showMonitored ? (
        <div className={styles.title}>{getMonitoredText(summary)}</div>
      ) : null}

      {showQualityProfile && !!qualityProfileName ? (
        <div className={styles.title} title={translate('QualityProfile')}>
          {qualityProfileName}
        </div>
      ) : null}

      {showReleaseDate && releaseDate ? (
        <div
          className={styles.title}
          title={`${translate('ReleaseDate')}: ${formatDate(
            releaseDate,
            longDateFormat
          )}`}
        >
          <Icon name={icons.CALENDAR} />{' '}
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
        <div className={styles.title}>
          <IgdbRating ratings={ratings} iconSize={12} />
        </div>
      ) : null}

      {showCriticRating && !!ratings.critic ? (
        <div className={styles.title}>
          <CriticRating ratings={ratings} iconSize={12} />
        </div>
      ) : null}

      {showTags && tags.length ? (
        <div className={styles.tags}>
          <div className={styles.tagsList}>
            <GameTagList tags={tags} />
          </div>
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
