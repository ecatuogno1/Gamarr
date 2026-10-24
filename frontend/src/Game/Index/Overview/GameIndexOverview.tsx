import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import TextTruncate from 'react-text-truncate';
import GameTagList from 'Components/GameTagList';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import Popover from 'Components/Tooltip/Popover';
import GameDetailsLinks from 'Game/Details/GameDetailsLinks';
import GamePoster from 'Game/GamePoster';
import GameIndexPosterSelect from 'Game/Index/Select/GameIndexPosterSelect';
import GamePlatformBadges from 'Game/Title/GamePlatformBadges';
import { getTitleLink } from 'Game/Title/gameTitle';
import GameTitleModals from 'Game/Title/GameTitleModals';
import GameTitleProgressBar from 'Game/Title/GameTitleProgressBar';
import useGameTitleIndexItem from 'Game/Title/useGameTitleIndexItem';
import { icons } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import fonts from 'Styles/Variables/fonts';
import translate from 'Utilities/String/translate';
import GameIndexOverviewInfo from './GameIndexOverviewInfo';
import selectOverviewOptions from './selectOverviewOptions';
import styles from './GameIndexOverview.css';

const columnPadding = parseInt(dimensions.gameIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(
  dimensions.gameIndexColumnPaddingSmallScreen
);
const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

// Hardcoded height beased on line-height of 32 + bottom margin of 10.
// Less side-effecty than using react-measure.
const titleRowHeight = 42;

interface GameIndexOverviewProps {
  gameIds: number[];
  sortKey: string;
  posterWidth: number;
  posterHeight: number;
  rowHeight: number;
  isSelectMode: boolean;
  isSmallScreen: boolean;
}

function GameIndexOverview(props: GameIndexOverviewProps) {
  const {
    gameIds,
    sortKey,
    posterWidth,
    posterHeight,
    rowHeight,
    isSelectMode,
    isSmallScreen,
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
    const padding = isSmallScreen ? columnPaddingSmallScreen : columnPadding;

    return rowHeight - padding;
  }, [rowHeight, isSmallScreen]);

  if (!summary) {
    return null;
  }

  const {
    title,
    status,
    path,
    overview,
    images,
    tags,
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

  const overviewHeight = contentHeight - titleRowHeight;

  return (
    <div>
      <div className={styles.content}>
        <div className={styles.poster}>
          <div className={styles.posterContainer}>
            {isSelectMode ? (
              <GameIndexPosterSelect gameId={igdbId} titleSlug={`${igdbId}`} />
            ) : null}

            {status === 'deleted' ? (
              <div className={styles.deleted} title={translate('Deleted')} />
            ) : null}

            <Link className={styles.link} style={elementStyle} to={link}>
              <GamePoster
                className={styles.poster}
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

              <SpinnerIconButton
                name={icons.REFRESH}
                title={translate('RefreshGame')}
                isSpinning={isRefreshingGame}
                onPress={onRefreshPress}
              />

              {overviewOptions.showSearchAction ? (
                <SpinnerIconButton
                  className={styles.actions}
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
            </div>
          </div>

          <div className={styles.details}>
            <div className={styles.overviewContainer}>
              <Link className={styles.overview} to={link}>
                <TextTruncate
                  line={Math.floor(
                    overviewHeight / (defaultFontSize * lineHeight)
                  )}
                  text={overview}
                />
              </Link>

              {overviewOptions.showTags ? (
                <div className={styles.tags}>
                  <GameTagList tags={tags} />
                </div>
              ) : null}
            </div>
            <GameIndexOverviewInfo
              height={overviewHeight}
              monitored={summary.monitored}
              qualityProfile={{ name: qualityProfileNames.join(', ') }}
              platform={summary.versions
                .map((v) => v.game.platform?.name)
                .filter((name) => !!name)
                .join(', ')}
              sizeOnDisk={summary.sizeOnDisk}
              added={added}
              path={path}
              sortKey={sortKey}
              {...overviewOptions}
            />
          </div>
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
