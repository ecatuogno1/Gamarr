import React, { useCallback } from 'react';
import { useSelector } from 'react-redux';
import { useSelect } from 'App/SelectContext';
import CriticRating from 'Components/CriticRating';
import GameTagList from 'Components/GameTagList';
import Icon from 'Components/Icon';
import IgdbRating from 'Components/IgdbRating';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import Column from 'Components/Table/Column';
import Tooltip from 'Components/Tooltip/Tooltip';
import GameDetailsLinks from 'Game/Details/GameDetailsLinks';
import GamePopularityIndex from 'Game/GamePopularityIndex';
import GameTitleLink from 'Game/GameTitleLink';
import GamePlatformBadges from 'Game/Title/GamePlatformBadges';
import GameTitleModals from 'Game/Title/GameTitleModals';
import GameTitleProgressBar from 'Game/Title/GameTitleProgressBar';
import useGameTitleIndexItem from 'Game/Title/useGameTitleIndexItem';
import { formatVersion, getRegionNames } from 'GameFile/releaseDetails';
import { icons, kinds } from 'Helpers/Props';
import { SelectStateInputProps } from 'typings/props';
import formatBytes from 'Utilities/Number/formatBytes';
import firstCharToUpper from 'Utilities/String/firstCharToUpper';
import translate from 'Utilities/String/translate';
import GameStatusCell from './GameStatusCell';
import selectTableOptions from './selectTableOptions';
import styles from './GameIndexRow.css';

interface GameIndexRowProps {
  gameIds: number[];
  sortKey: string;
  columns: Column[];
  isSelectMode: boolean;
}

function joinUnique(values: (string | undefined)[]) {
  return [...new Set(values.filter((v) => !!v))].join(', ');
}

function GameIndexRow(props: GameIndexRowProps) {
  const { gameIds, columns, isSelectMode } = props;

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

  const { showSearchAction } = useSelector(selectTableOptions);

  const [selectState, selectDispatch] = useSelect();

  const onSelectedChange = useCallback(
    ({ id, value, shiftKey }: SelectStateInputProps) => {
      selectDispatch({
        type: 'toggleSelected',
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [selectDispatch]
  );

  if (!summary) {
    return null;
  }

  const versionGames = summary.versions.map((v) => v.game);
  const files = versionGames
    .map((game) => game.gameFile)
    .filter((file) => !!file);

  const {
    title,
    collection,
    developers = [],
    publishers = [],
    status,
    originalLanguage,
    originalTitle,
    year,
    releaseDate,
    genres = [],
    keywords = [],
    ratings,
    popularity,
    igdbId,
    igdbSlug,
    youTubeTrailerId,
  } = summary.primary;

  const monitored = summary.monitored;
  const isSaving = versionGames.some((game) => game.isSaving);
  const added = versionGames.reduce(
    (acc, game) => (!acc || game.added < acc ? game.added : acc),
    ''
  );
  const minimumAvailability = joinUnique(
    versionGames.map((game) =>
      translate(firstCharToUpper(game.minimumAvailability))
    )
  );
  const paths = versionGames.map((game) => game.path);
  const tags = [...new Set(versionGames.flatMap((game) => game.tags))];
  const releaseGroups = [
    ...new Set(
      versionGames.flatMap((game) => game.statistics?.releaseGroups ?? [])
    ),
  ];
  const gameFileQualities = versionGames
    .flatMap((game) => game.statistics?.gameFileQualities ?? [])
    .filter((q, index, all) => all.findIndex((o) => o.id === q.id) === index);

  return (
    <>
      {isSelectMode ? (
        <VirtualTableSelectCell
          id={igdbId}
          isSelected={selectState.selectedState[igdbId]}
          isDisabled={false}
          onSelectedChange={onSelectedChange}
        />
      ) : null}

      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'status') {
          return (
            <GameStatusCell
              key={name}
              className={styles[name]}
              gameId={gameIds[0]}
              gameIds={gameIds}
              monitored={monitored}
              status={status}
              isSelectMode={isSelectMode}
              isSaving={isSaving}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'sortTitle') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <GameTitleLink titleSlug={`${igdbId}`} title={title} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'collection') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {collection ? collection.title : null}
            </VirtualTableRowCell>
          );
        }

        if (name === 'platform') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <GamePlatformBadges
                className={styles.platformBadges}
                versions={summary.versions}
              />
            </VirtualTableRowCell>
          );
        }

        if (name === 'developers') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {developers.join(', ')}
            </VirtualTableRowCell>
          );
        }

        if (name === 'publishers') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {publishers.join(', ')}
            </VirtualTableRowCell>
          );
        }

        if (name === 'fileRegions') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {joinUnique(files.map((file) => getRegionNames(file?.regions)))}
            </VirtualTableRowCell>
          );
        }

        if (name === 'fileVersion') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {joinUnique(files.map((file) => formatVersion(file?.version)))}
            </VirtualTableRowCell>
          );
        }

        if (name === 'originalLanguage') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {originalLanguage.name}
            </VirtualTableRowCell>
          );
        }

        if (name === 'originalTitle') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {originalTitle}
            </VirtualTableRowCell>
          );
        }

        if (name === 'qualityProfileId') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {qualityProfileNames.join(', ')}
            </VirtualTableRowCell>
          );
        }

        if (name === 'added') {
          return (
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore ts(2739)
            <RelativeDateCell
              key={name}
              className={styles[name]}
              date={added}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'year') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {year > 0 ? year : null}
            </VirtualTableRowCell>
          );
        }

        if (name === 'releaseDate') {
          return (
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore ts(2739)
            <RelativeDateCell
              key={name}
              className={styles[name]}
              date={releaseDate}
              timeForToday={false}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'minimumAvailability') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {minimumAvailability}
            </VirtualTableRowCell>
          );
        }

        if (name === 'path') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span title={paths.join('\n')}>{paths.join(', ')}</span>
            </VirtualTableRowCell>
          );
        }

        if (name === 'sizeOnDisk') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {formatBytes(summary.sizeOnDisk)}
            </VirtualTableRowCell>
          );
        }

        if (name === 'genres') {
          const joinedGenres = genres.join(', ');

          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span title={joinedGenres}>{joinedGenres}</span>
            </VirtualTableRowCell>
          );
        }

        if (name === 'keywords') {
          const joinedKeywords = keywords.join(', ');
          const truncatedKeywords =
            keywords.length > 3
              ? `${keywords.slice(0, 3).join(', ')}...`
              : joinedKeywords;

          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span title={joinedKeywords}>{truncatedKeywords}</span>
            </VirtualTableRowCell>
          );
        }

        if (name === 'gameStatus') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <GameTitleProgressBar
                summary={summary}
                width={125}
                detailedProgressBar={true}
                bottomRadius={false}
                isStandAlone={true}
              />
            </VirtualTableRowCell>
          );
        }

        if (name === 'igdbRating') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {ratings.igdb ? <IgdbRating ratings={ratings} /> : null}
            </VirtualTableRowCell>
          );
        }

        if (name === 'criticRating') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {ratings.critic ? <CriticRating ratings={ratings} /> : null}
            </VirtualTableRowCell>
          );
        }

        if (name === 'popularity') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <GamePopularityIndex popularity={popularity} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'releaseGroups') {
          const joinedReleaseGroups = releaseGroups.join(', ');
          const truncatedReleaseGroups =
            releaseGroups.length > 3
              ? `${releaseGroups.slice(0, 3).join(', ')}...`
              : joinedReleaseGroups;

          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span title={joinedReleaseGroups}>{truncatedReleaseGroups}</span>
            </VirtualTableRowCell>
          );
        }

        if (name === 'gameFileQualities') {
          const joinedQualities = gameFileQualities
            .map((q) => q.name)
            .join(', ');
          const truncatedQualities =
            gameFileQualities.length > 3
              ? `${gameFileQualities
                  .slice(0, 3)
                  .map((q) => q.name)
                  .join(', ')}...`
              : joinedQualities;

          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span title={joinedQualities}>{truncatedQualities}</span>
            </VirtualTableRowCell>
          );
        }

        if (name === 'tags') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <GameTagList tags={tags} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span className={styles.externalLinks}>
                <Tooltip
                  anchor={<Icon name={icons.EXTERNAL_LINK} size={12} />}
                  tooltip={
                    <GameDetailsLinks
                      igdbId={igdbId}
                      igdbSlug={igdbSlug}
                      youTubeTrailerId={youTubeTrailerId}
                    />
                  }
                  canFlip={true}
                  kind={kinds.INVERSE}
                />
              </span>

              <SpinnerIconButton
                name={icons.REFRESH}
                title={translate('RefreshGame')}
                isSpinning={isRefreshingGame}
                onPress={onRefreshPress}
              />

              {showSearchAction ? (
                <SpinnerIconButton
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
            </VirtualTableRowCell>
          );
        }

        return null;
      })}

      <GameTitleModals
        gameIds={gameIds}
        isEditModalOpen={isEditGameModalOpen}
        isDeleteModalOpen={isDeleteGameModalOpen}
        onEditModalClose={onEditGameModalClose}
        onDeletePress={onDeleteGamePress}
        onDeleteModalClose={onDeleteGameModalClose}
      />
    </>
  );
}

export default GameIndexRow;
