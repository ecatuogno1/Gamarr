import classNames from 'classnames';
import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import Popover from 'Components/Tooltip/Popover';
import GameDetailsLinks from 'Game/Details/GameDetailsLinks';
import { GameStatus } from 'Game/Game';
import GamePoster from 'Game/GamePoster';
import GameIndexPosterSelect from 'Game/Index/Select/GameIndexPosterSelect';
import GamePlatformBadges from 'Game/Title/GamePlatformBadges';
import { getTitleLink } from 'Game/Title/gameTitle';
import GameTitleModals from 'Game/Title/GameTitleModals';
import GameTitleProgressBar from 'Game/Title/GameTitleProgressBar';
import useGameTitleIndexItem from 'Game/Title/useGameTitleIndexItem';
import useMeasure from 'Helpers/Hooks/useMeasure';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import GameIndexOverviewInfo from './GameIndexOverviewInfo';
import { ROW_BORDER, ROW_VERTICAL_PADDING } from './overviewLayout';
import selectOverviewOptions from './selectOverviewOptions';
import styles from './GameIndexOverview.css';

// Mirrors .synopsis font-size/line-height in GameIndexOverview.css.
const SYNOPSIS_FONT_SIZE = 13.5;
const SYNOPSIS_LINE_HEIGHT = 1.6;

const TITLE_HEIGHT = 38;
const CHIP_STRIP_HEIGHT = 28;

// Sonarr v5 shows the series status as a pill on the overview poster; games
// map tba/announced/released/deleted onto the same status colours.
function getStatusBadge(status: GameStatus) {
  switch (status) {
    case 'deleted':
      return { label: translate('Deleted'), statusClass: styles.deleted };
    case 'tba':
      return { label: translate('Tba'), statusClass: styles.upcoming };
    case 'announced':
      return { label: translate('Announced'), statusClass: styles.upcoming };
    case 'released':
      return { label: translate('Released'), statusClass: styles.ended };
    default:
      return null;
  }
}

interface GameIndexOverviewProps {
  gameIds: number[];
  sortKey: string;
  posterWidth: number;
  posterHeight: number;
  rowHeight: number;
  isSelectMode: boolean;
}

function GameIndexOverview(props: GameIndexOverviewProps) {
  const {
    gameIds,
    sortKey,
    posterWidth,
    posterHeight,
    rowHeight,
    isSelectMode,
  } = props;

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

  const overviewOptions = useSelector(selectOverviewOptions);

  const contentHeight = useMemo(() => {
    return rowHeight - (ROW_VERTICAL_PADDING * 2 + ROW_BORDER);
  }, [rowHeight]);

  const [chipStripRef, chipStripBounds] = useMeasure();
  const chipStripHeight = chipStripBounds.height || CHIP_STRIP_HEIGHT;

  const synopsisLines = Math.max(
    1,
    Math.floor(
      (contentHeight - TITLE_HEIGHT - chipStripHeight) /
        (SYNOPSIS_FONT_SIZE * SYNOPSIS_LINE_HEIGHT)
    )
  );

  if (!summary) {
    return null;
  }

  const {
    title,
    status,
    path,
    overview,
    images,
    tags = [],
    igdbId,
    igdbSlug,
    added,
    youTubeTrailerId,
  } = summary.primary;

  const link = getTitleLink(igdbId);

  const elementStyle = {
    width: `${posterWidth}px`,
    height: `${posterHeight}px`,
  };

  const statusBadge = getStatusBadge(status);

  return (
    <div>
      <div className={styles.content}>
        <div className={styles.poster}>
          <div className={styles.posterContainer}>
            {isSelectMode ? (
              <GameIndexPosterSelect gameId={igdbId} titleSlug={`${igdbId}`} />
            ) : null}

            {statusBadge ? (
              <div
                className={classNames(
                  styles.statusPill,
                  statusBadge.statusClass
                )}
              >
                <span className={styles.statusLabel}>{statusBadge.label}</span>
              </div>
            ) : null}

            <Link className={styles.link} style={elementStyle} to={link}>
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

          <GameTitleProgressBar
            summary={summary}
            width={posterWidth}
            detailedProgressBar={overviewOptions.detailedProgressBar}
            bottomRadius={false}
          />
        </div>

        <div className={styles.info} style={{ maxHeight: contentHeight }}>
          <div className={styles.titleRow}>
            <Link className={styles.title} to={link}>
              {title}
            </Link>

            {overviewOptions.showPlatform ? (
              <GamePlatformBadges
                className={styles.platforms}
                versions={summary.versions}
              />
            ) : null}

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

              <SpinnerIconButton
                className={styles.action}
                name={icons.REFRESH}
                title={translate('RefreshGame')}
                isSpinning={isRefreshingGame}
                onPress={onRefreshPress}
              />

              {overviewOptions.showSearchAction ? (
                <SpinnerIconButton
                  className={styles.action}
                  name={icons.SEARCH}
                  title={translate('SearchForGame')}
                  isSpinning={isSearchingGame}
                  onPress={onSearchPress}
                />
              ) : null}

              <IconButton
                className={styles.action}
                name={icons.EDIT}
                title={translate('EditGame')}
                aria-label={translate('EditGame')}
                onPress={onEditGamePress}
              />
            </div>
          </div>

          <Link
            className={styles.synopsis}
            to={link}
            style={{ WebkitLineClamp: synopsisLines }}
          >
            {overview}
          </Link>

          <GameIndexOverviewInfo
            ref={chipStripRef}
            monitored={summary.monitored}
            qualityProfile={{ name: qualityProfileNames.join(', ') }}
            platform={summary.versions
              .map((v) => v.game.platform?.name)
              .filter((name) => !!name)
              .join(', ')}
            sizeOnDisk={summary.sizeOnDisk}
            added={added}
            path={path}
            tags={tags}
            sortKey={sortKey}
            showPlatform={overviewOptions.showPlatform}
            showMonitored={overviewOptions.showMonitored}
            showQualityProfile={overviewOptions.showQualityProfile}
            showAdded={overviewOptions.showAdded}
            showPath={overviewOptions.showPath}
            showSizeOnDisk={overviewOptions.showSizeOnDisk}
            showTags={overviewOptions.showTags}
          />
        </div>
      </div>

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

export default GameIndexOverview;
