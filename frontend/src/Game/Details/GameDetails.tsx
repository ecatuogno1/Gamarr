import classNames from 'classnames';
import React, {
  useCallback,
  useEffect,
  useLayoutEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useHistory } from 'react-router';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import * as commandNames from 'Commands/commandNames';
import Alert from 'Components/Alert';
import CriticRating from 'Components/CriticRating';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import IgdbRating from 'Components/IgdbRating';
import Label from 'Components/Label';
import Button from 'Components/Link/Button';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import MetadataAttribution from 'Components/MetadataAttribution';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { OverflowDivider } from 'Components/Page/Toolbar/Overflow';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import PageToolbarSpacer from 'Components/Page/Toolbar/PageToolbarSpacer';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import AddGamePlatformModal from 'Game/AddPlatform/AddGamePlatformModal';
import DeleteGameModal from 'Game/Delete/DeleteGameModal';
import EditGameModal from 'Game/Edit/EditGameModal';
import { Image, Statistics } from 'Game/Game';
import GameCollectionLabel from 'Game/GameCollectionLabel';
import GameGenres from 'Game/GameGenres';
import GamePoster from 'Game/GamePoster';
import getGameStatusDetails from 'Game/getGameStatusDetails';
import GameHistoryModal from 'Game/History/GameHistoryModal';
import GameInteractiveSearchModal from 'Game/Search/GameInteractiveSearchModal';
import GamePlatformBadges from 'Game/Title/GamePlatformBadges';
import { groupGamesByTitle } from 'Game/Title/gameTitle';
import { getPlatformsPresentText } from 'Game/Title/GameTitleProgressBar';
import useGameTitle from 'Game/Title/useGameTitle';
import useGame from 'Game/useGame';
import GameFileEditorTable from 'GameFile/Editor/GameFileEditorTable';
import ExtraFileTable from 'GameFile/Extras/ExtraFileTable';
import usePrevious from 'Helpers/Hooks/usePrevious';
import {
  icons,
  kinds,
  sizes,
  sortDirections,
  tooltipPositions,
} from 'Helpers/Props';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import OrganizePreviewModal from 'Organize/OrganizePreviewModal';
import QualityProfileName from 'Settings/Profiles/Quality/QualityProfileName';
import { executeCommand } from 'Store/Actions/commandActions';
import {
  clearExtraFiles,
  fetchExtraFiles,
} from 'Store/Actions/extraFileActions';
import { toggleGameMonitored } from 'Store/Actions/gameActions';
import {
  clearGameCredits,
  fetchGameCredits,
} from 'Store/Actions/gameCreditsActions';
import { clearGameFiles, fetchGameFiles } from 'Store/Actions/gameFileActions';
import {
  clearQueueDetails,
  fetchQueueDetails,
} from 'Store/Actions/queueActions';
import {
  cancelFetchReleases,
  clearReleases,
} from 'Store/Actions/releaseActions';
import { fetchImportListSchema } from 'Store/Actions/Settings/importLists';
import createAllGamesSelector from 'Store/Selectors/createAllGamesSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import sortByProp from 'Utilities/Array/sortByProp';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import getPathWithUrlBase from 'Utilities/getPathWithUrlBase';
import formatBytes from 'Utilities/Number/formatBytes';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import GameCompanyCredits from './Credits/GameCompanyCredits';
import GameDetailsInfo from './GameDetailsInfo';
import GameDetailsLinks from './GameDetailsLinks';
import GameDetailsPath from './GameDetailsPath';
import GameReleaseDates from './GameReleaseDates';
import GameScreenshots from './GameScreenshots';
import GameStatusLabel from './GameStatusLabel';
import GameTags from './GameTags';
import GameTitlesTable from './Titles/GameTitlesTable';
import styles from './GameDetails.css';

function getFanartUrl(images: Image[]) {
  const image = images.find((image) => image.coverType === 'fanart');
  return image?.url ?? image?.remoteUrl;
}

function createGameFilesSelector(gameId: number) {
  return createSelector(
    (state: AppState) => state.gameFiles,
    ({ items, isFetching, isPopulated, error }) => {
      // The files of all platform versions are loaded, these are the selected platform's
      const hasGameFiles = items.some((file) => file.gameId === gameId);

      return {
        isGameFilesFetching: isFetching,
        isGameFilesPopulated: isPopulated,
        gameFilesError: error,
        hasGameFiles,
      };
    }
  );
}

function createExtraFilesSelector() {
  return createSelector(
    (state: AppState) => state.extraFiles,
    ({ isFetching, isPopulated, error }) => {
      return {
        isExtraFilesFetching: isFetching,
        isExtraFilesPopulated: isPopulated,
        extraFilesError: error,
      };
    }
  );
}

function createGameCreditsSelector() {
  return createSelector(
    (state: AppState) => state.gameCredits,
    ({ isFetching, isPopulated, error }) => {
      return {
        isGameCreditsFetching: isFetching,
        isGameCreditsPopulated: isPopulated,
        gameCreditsError: error,
      };
    }
  );
}

interface GameDetailsProps {
  // The selected platform version
  gameId: number;
  // Every platform version of the title
  titleGameIds: number[];
  onPlatformSelect: (gameId: number) => void;
}

function GameDetails({
  gameId,
  titleGameIds,
  onPlatformSelect,
}: GameDetailsProps) {
  const dispatch = useDispatch();
  const history = useHistory();

  const game = useGame(gameId);
  const allGames = useSelector(createAllGamesSelector());
  const titleSummary = useGameTitle(titleGameIds);
  const titleGameIdsKey = titleGameIds.join();
  const isMultiPlatform = titleGameIds.length > 1;

  const { isGameFilesFetching, gameFilesError, hasGameFiles } = useSelector(
    createGameFilesSelector(gameId)
  );
  const { isExtraFilesFetching, extraFilesError } = useSelector(
    createExtraFilesSelector()
  );
  const { isGameCreditsFetching, gameCreditsError } = useSelector(
    createGameCreditsSelector()
  );
  const isSidebarVisible = useSelector(
    (state: AppState) => state.app.isSidebarVisible
  );

  const commands = useSelector(createCommandsSelector());

  const { isRefreshing, isRenaming, isSearching } = useMemo(() => {
    const gameRefreshingCommand = findCommand(commands, {
      name: commandNames.REFRESH_GAME,
    });

    const isGameRefreshingCommandExecuting = isCommandExecuting(
      gameRefreshingCommand
    );

    const allGamesRefreshing =
      isGameRefreshingCommandExecuting &&
      !gameRefreshingCommand?.body.gameIds?.length;

    const isGameRefreshing =
      isGameRefreshingCommandExecuting &&
      gameRefreshingCommand?.body.gameIds?.includes(gameId);

    const isSearchingExecuting = isCommandExecuting(
      findCommand(commands, {
        name: commandNames.GAME_SEARCH,
        gameIds: [gameId],
      })
    );

    const isRenamingFiles = isCommandExecuting(
      findCommand(commands, {
        name: commandNames.RENAME_FILES,
        gameId,
      })
    );

    const isRenamingGameCommand = findCommand(commands, {
      name: commandNames.RENAME_GAME,
    });

    const isRenamingGame =
      isCommandExecuting(isRenamingGameCommand) &&
      isRenamingGameCommand?.body?.gameIds?.includes(gameId);

    return {
      isRefreshing: isGameRefreshing || allGamesRefreshing,
      isRenaming: isRenamingFiles || isRenamingGame,
      isSearching: isSearchingExecuting,
    };
  }, [gameId, commands]);

  const { nextGame, previousGame } = useMemo(() => {
    // Navigate between titles, not their platform versions
    const titles = [...groupGamesByTitle(allGames).values()].map(
      (versions) => versions[0]
    );
    const sortedGames = titles.sort(sortByProp('sortTitle'));
    const igdbId = allGames.find((g) => g.id === gameId)?.igdbId;
    const gameIndex = sortedGames.findIndex((game) => game.igdbId === igdbId);

    if (gameIndex === -1) {
      return {
        nextGame: undefined,
        previousGame: undefined,
      };
    }

    const nextGame = sortedGames[gameIndex + 1] ?? sortedGames[0];
    const previousGame =
      sortedGames[gameIndex - 1] ?? sortedGames[sortedGames.length - 1];

    return {
      nextGame: {
        title: nextGame.title,
        titleSlug: `${nextGame.igdbId}`,
      },
      previousGame: {
        title: previousGame.title,
        titleSlug: `${previousGame.igdbId}`,
      },
    };
  }, [gameId, allGames]);

  const touchStart = useRef<number | null>(null);
  const [isOrganizeModalOpen, setIsOrganizeModalOpen] = useState(false);
  const [isManageGamesModalOpen, setIsManageGamesModalOpen] = useState(false);
  const [isInteractiveSearchModalOpen, setIsInteractiveSearchModalOpen] =
    useState(false);
  const [isEditGameModalOpen, setIsEditGameModalOpen] = useState(false);
  const [isDeleteGameModalOpen, setIsDeleteGameModalOpen] = useState(false);
  const [isGameHistoryModalOpen, setIsGameHistoryModalOpen] = useState(false);
  const [isAddPlatformModalOpen, setIsAddPlatformModalOpen] = useState(false);
  const [showAllPlatformFiles, setShowAllPlatformFiles] = useState(false);
  const overviewTextRef = useRef<HTMLParagraphElement>(null);
  const [isOverviewExpanded, setIsOverviewExpanded] = useState(false);
  const [isOverviewClamped, setIsOverviewClamped] = useState(false);

  const handleToggleOverview = useCallback(() => {
    setIsOverviewExpanded((value) => !value);
  }, []);

  useLayoutEffect(() => {
    const element = overviewTextRef.current;

    if (!element) {
      return;
    }

    const compute = () =>
      setIsOverviewClamped(
        !isOverviewExpanded && element.scrollHeight > element.clientHeight
      );

    compute();

    const observer = new ResizeObserver(compute);
    observer.observe(element);

    return () => observer.disconnect();
  }, [game?.overview, isOverviewExpanded]);
  const wasRefreshing = usePrevious(isRefreshing);
  const wasRenaming = usePrevious(isRenaming);

  const handleOrganizePress = useCallback(() => {
    setIsOrganizeModalOpen(true);
  }, []);

  const handleOrganizeModalClose = useCallback(() => {
    setIsOrganizeModalOpen(false);
  }, []);

  const handleManageGamesPress = useCallback(() => {
    setIsManageGamesModalOpen(true);
  }, []);

  const handleManageGamesModalClose = useCallback(() => {
    setIsManageGamesModalOpen(false);
  }, []);

  const handleInteractiveSearchPress = useCallback(() => {
    setIsInteractiveSearchModalOpen(true);
  }, []);

  const handleInteractiveSearchModalClose = useCallback(() => {
    setIsInteractiveSearchModalOpen(false);
  }, []);

  const handleEditGamePress = useCallback(() => {
    setIsEditGameModalOpen(true);
  }, []);

  const handleEditGameModalClose = useCallback(() => {
    setIsEditGameModalOpen(false);
  }, []);

  const handleDeleteGamePress = useCallback(() => {
    setIsEditGameModalOpen(false);
    setIsDeleteGameModalOpen(true);
  }, []);

  const handleDeleteGameModalClose = useCallback(() => {
    setIsDeleteGameModalOpen(false);
  }, []);

  const handleGameHistoryPress = useCallback(() => {
    setIsGameHistoryModalOpen(true);
  }, []);

  const handleGameHistoryModalClose = useCallback(() => {
    setIsGameHistoryModalOpen(false);
  }, []);

  const handleAddPlatformPress = useCallback(() => {
    setIsAddPlatformModalOpen(true);
  }, []);

  const handleAddPlatformModalClose = useCallback(() => {
    setIsAddPlatformModalOpen(false);
  }, []);

  const handleThisPlatformFilesPress = useCallback(() => {
    setShowAllPlatformFiles(false);
  }, []);

  const handleAllPlatformFilesPress = useCallback(() => {
    setShowAllPlatformFiles(true);
  }, []);

  const handleMonitorTogglePress = useCallback(
    (value: boolean) => {
      dispatch(
        toggleGameMonitored({
          gameId,
          monitored: value,
        })
      );
    },
    [gameId, dispatch]
  );

  const handleMonitorToggle = useCallback(() => {
    handleMonitorTogglePress(!game?.monitored);
  }, [handleMonitorTogglePress, game?.monitored]);

  const handleRefreshPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.REFRESH_GAME,
        gameIds: [gameId],
      })
    );
  }, [gameId, dispatch]);

  const handleSearchPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.GAME_SEARCH,
        gameIds: [gameId],
      })
    );
  }, [gameId, dispatch]);

  const handleTouchStart = useCallback(
    (event: TouchEvent) => {
      const touches = event.touches;
      const currentTouch = touches[0].pageX;
      const touchY = touches[0].pageY;

      // Only change when swipe is on header, we need horizontal scroll on tables
      if (touchY > 470) {
        return;
      }

      if (touches.length !== 1) {
        return;
      }

      if (
        currentTouch < 50 ||
        isSidebarVisible ||
        isOrganizeModalOpen ||
        isEditGameModalOpen ||
        isDeleteGameModalOpen ||
        isManageGamesModalOpen ||
        isInteractiveSearchModalOpen ||
        isGameHistoryModalOpen ||
        isAddPlatformModalOpen
      ) {
        return;
      }

      touchStart.current = currentTouch;
    },
    [
      isSidebarVisible,
      isOrganizeModalOpen,
      isEditGameModalOpen,
      isDeleteGameModalOpen,
      isManageGamesModalOpen,
      isInteractiveSearchModalOpen,
      isGameHistoryModalOpen,
      isAddPlatformModalOpen,
    ]
  );

  const handleTouchEnd = useCallback(
    (event: TouchEvent) => {
      const touches = event.changedTouches;
      const currentTouch = touches[0].pageX;

      if (!touchStart.current) {
        return;
      }

      if (
        currentTouch > touchStart.current &&
        currentTouch - touchStart.current > 100 &&
        previousGame !== undefined
      ) {
        history.push(getPathWithUrlBase(`/game/${previousGame.titleSlug}`));
      } else if (
        currentTouch < touchStart.current &&
        touchStart.current - currentTouch > 100 &&
        nextGame !== undefined
      ) {
        history.push(getPathWithUrlBase(`/game/${nextGame.titleSlug}`));
      }

      touchStart.current = null;
    },
    [previousGame, nextGame, history]
  );

  const handleTouchCancel = useCallback(() => {
    touchStart.current = null;
  }, []);

  const handleTouchMove = useCallback(() => {
    if (!touchStart.current) {
      return;
    }
  }, []);

  const handleKeyUp = useCallback(
    (event: KeyboardEvent) => {
      if (
        isOrganizeModalOpen ||
        isManageGamesModalOpen ||
        isInteractiveSearchModalOpen ||
        isEditGameModalOpen ||
        isDeleteGameModalOpen ||
        isGameHistoryModalOpen ||
        isAddPlatformModalOpen
      ) {
        return;
      }

      if (event.composedPath && event.composedPath().length === 4) {
        if (event.key === 'ArrowLeft' && previousGame !== undefined) {
          history.push(getPathWithUrlBase(`/game/${previousGame.titleSlug}`));
        }

        if (event.key === 'ArrowRight' && nextGame !== undefined) {
          history.push(getPathWithUrlBase(`/game/${nextGame.titleSlug}`));
        }
      }
    },
    [
      isOrganizeModalOpen,
      isManageGamesModalOpen,
      isInteractiveSearchModalOpen,
      isEditGameModalOpen,
      isDeleteGameModalOpen,
      isGameHistoryModalOpen,
      isAddPlatformModalOpen,
      previousGame,
      nextGame,
      history,
    ]
  );

  const populate = useCallback(() => {
    // Files of every platform version, for the all platforms view
    dispatch(
      fetchGameFiles({ gameId: titleGameIdsKey.split(',').map(Number) })
    );
    dispatch(fetchExtraFiles({ gameId }));
    dispatch(fetchGameCredits({ gameId }));
    dispatch(fetchQueueDetails({ gameId }));
    dispatch(fetchImportListSchema());
  }, [gameId, titleGameIdsKey, dispatch]);

  useEffect(() => {
    populate();
  }, [populate]);

  useEffect(() => {
    registerPagePopulator(populate, ['gameUpdated']);

    return () => {
      unregisterPagePopulator(populate);
      dispatch(clearGameFiles());
      dispatch(clearExtraFiles());
      dispatch(clearGameCredits());
      dispatch(clearQueueDetails());
      dispatch(cancelFetchReleases());
      dispatch(clearReleases());
    };
  }, [populate, dispatch]);

  useEffect(() => {
    if ((!isRefreshing && wasRefreshing) || (!isRenaming && wasRenaming)) {
      populate();
    }
  }, [isRefreshing, wasRefreshing, isRenaming, wasRenaming, populate]);

  useEffect(() => {
    window.addEventListener('touchstart', handleTouchStart);
    window.addEventListener('touchend', handleTouchEnd);
    window.addEventListener('touchcancel', handleTouchCancel);
    window.addEventListener('touchmove', handleTouchMove);
    window.addEventListener('keyup', handleKeyUp);

    return () => {
      window.removeEventListener('touchstart', handleTouchStart);
      window.removeEventListener('touchend', handleTouchEnd);
      window.removeEventListener('touchcancel', handleTouchCancel);
      window.removeEventListener('touchmove', handleTouchMove);
      window.removeEventListener('keyup', handleKeyUp);
    };
  }, [
    handleTouchStart,
    handleTouchEnd,
    handleTouchCancel,
    handleTouchMove,
    handleKeyUp,
  ]);

  if (!game) {
    return null;
  }

  const {
    id,
    igdbId,
    igdbSlug,
    title,
    originalTitle,
    year,
    releaseDate,
    ratings,
    path,
    statistics = {} as Statistics,
    qualityProfileId,
    monitored,
    platform,
    developers = [],
    publishers = [],
    websites = [],
    genres = [],
    collection,
    overview,
    status,
    youTubeTrailerId,
    isAvailable,
    images,
    tags,
    isSaving = false,
  } = game;

  const { sizeOnDisk = 0 } = statistics;

  const statusDetails = getGameStatusDetails(status);

  const fanartUrl = getFanartUrl(images);
  const screenshots = images.filter(
    (image) => image.coverType === 'screenshot'
  );
  const isFetching =
    isGameFilesFetching || isExtraFilesFetching || isGameCreditsFetching;

  const titleWithYear = `${title}${year > 0 ? ` (${year})` : ''}`;

  return (
    <PageContent title={titleWithYear}>
      <PageToolbar>
        <ToolbarItem
          id="refresh"
          priority={1}
          groupId="left-a"
          label={translate('RefreshAndScan')}
          iconName={icons.REFRESH}
          spinningName={icons.REFRESH}
          title={translate('RefreshInformationAndScanDisk')}
          isSpinning={isRefreshing}
          onPress={handleRefreshPress}
        />

        <ToolbarItem
          id="search"
          priority={1}
          groupId="left-a"
          label={translate('SearchGame')}
          iconName={icons.SEARCH}
          isSpinning={isSearching}
          onPress={handleSearchPress}
        />

        <ToolbarItem
          id="interactive-search"
          priority={1}
          groupId="left-a"
          label={translate('InteractiveSearch')}
          iconName={icons.INTERACTIVE}
          isSpinning={isSearching}
          onPress={handleInteractiveSearchPress}
        />

        <OverflowDivider groupId="left-a">
          <PageToolbarSeparator />
        </OverflowDivider>

        <ToolbarItem
          id="preview-rename"
          priority={1}
          groupId="left-b"
          label={translate('PreviewRename')}
          iconName={icons.ORGANIZE}
          isDisabled={!hasGameFiles}
          onPress={handleOrganizePress}
        />

        <ToolbarItem
          id="manage-files"
          priority={1}
          groupId="left-b"
          label={translate('ManageFiles')}
          iconName={icons.GAME_FILE}
          onPress={handleManageGamesPress}
        />

        <ToolbarItem
          id="history"
          priority={1}
          groupId="left-b"
          label={translate('History')}
          iconName={icons.HISTORY}
          onPress={handleGameHistoryPress}
        />

        <OverflowDivider groupId="left-b">
          <PageToolbarSeparator />
        </OverflowDivider>

        <ToolbarItem
          id="edit"
          priority={1}
          groupId="left-c"
          label={translate('Edit')}
          iconName={icons.EDIT}
          onPress={handleEditGamePress}
        />

        <ToolbarItem
          id="delete"
          priority={1}
          groupId="left-c"
          label={translate('Delete')}
          iconName={icons.DELETE}
          onPress={handleDeleteGamePress}
        />

        <PageToolbarSpacer />
      </PageToolbar>

      <PageContentBody innerClassName={styles.innerContentBody}>
        <div className={styles.header}>
          <div
            className={styles.backdrop}
            style={
              fanartUrl ? { backgroundImage: `url(${fanartUrl})` } : undefined
            }
          >
            <div className={styles.backdropOverlay} />
          </div>

          <div className={styles.headerContent}>
            <GamePoster
              className={styles.poster}
              images={images}
              size={500}
              lazy={false}
            />

            <div className={styles.info}>
              <div className={styles.titleRow}>
                <div className={styles.titleContainer}>
                  <div className={styles.title} title={originalTitle}>
                    {title}
                  </div>
                </div>

                <div className={styles.gameNavigationButtons}>
                  {previousGame ? (
                    <IconButton
                      className={styles.gameNavigationButton}
                      name={icons.ARROW_LEFT}
                      size={24}
                      title={translate('GameDetailsGoTo', {
                        title: previousGame.title,
                      })}
                      aria-label={translate('GameDetailsGoTo', {
                        title: previousGame.title,
                      })}
                      to={`/game/${previousGame.titleSlug}`}
                    />
                  ) : null}

                  {nextGame ? (
                    <IconButton
                      className={styles.gameNavigationButton}
                      name={icons.ARROW_RIGHT}
                      size={24}
                      title={translate('GameDetailsGoTo', {
                        title: nextGame.title,
                      })}
                      aria-label={translate('GameDetailsGoTo', {
                        title: nextGame.title,
                      })}
                      to={`/game/${nextGame.titleSlug}`}
                    />
                  ) : null}
                </div>
              </div>

              <div className={styles.platformRow}>
                {titleSummary ? (
                  <GamePlatformBadges
                    versions={titleSummary.versions}
                    size="large"
                    selectedGameId={id}
                    onSelect={onPlatformSelect}
                  />
                ) : null}

                <Button
                  className={styles.addPlatformButton}
                  size={sizes.SMALL}
                  title={translate('AddPlatformHelpText')}
                  onPress={handleAddPlatformPress}
                >
                  <Icon name={icons.ADD} />
                  <span className={styles.addPlatformLabel}>
                    {translate('AddPlatform')}
                  </span>
                </Button>
              </div>

              <div className={styles.details}>
                <span className={styles.year}>
                  <Popover
                    anchor={
                      year > 0 ? (
                        year
                      ) : (
                        <Icon
                          name={icons.WARNING}
                          kind={kinds.WARNING}
                          size={16}
                        />
                      )
                    }
                    title={translate('ReleaseDates')}
                    body={
                      <GameReleaseDates
                        igdbId={igdbId}
                        igdbSlug={igdbSlug}
                        releaseDate={releaseDate}
                      />
                    }
                    position={tooltipPositions.BOTTOM}
                  />
                </span>

                {genres.length ? (
                  <GameGenres className={styles.genres} genres={genres} />
                ) : null}

                {ratings.igdb ? (
                  <span className={styles.rating}>
                    <IgdbRating ratings={ratings} iconSize={14} />
                  </span>
                ) : null}

                {ratings.critic ? (
                  <span className={styles.rating}>
                    <CriticRating ratings={ratings} iconSize={14} />
                  </span>
                ) : null}
              </div>

              <div className={styles.detailsChips}>
                <div className={styles.chipsRow}>
                  <Link
                    className={styles.monitoredToggle}
                    isDisabled={isSaving}
                    aria-pressed={monitored}
                    onPress={handleMonitorToggle}
                  >
                    <Label
                      className={styles.detailsLabel}
                      size={sizes.LARGE}
                      icon={icons.MONITORED}
                      iconFilled={monitored}
                      interactive={true}
                    >
                      <span className={styles.monitoredText}>
                        {monitored
                          ? translate('Monitored')
                          : translate('Unmonitored')}
                      </span>
                    </Label>
                  </Link>

                  <Tooltip
                    anchor={
                      <Label
                        className={styles.detailsLabel}
                        size={sizes.LARGE}
                        icon={icons.EXTERNAL_LINK}
                        interactive={true}
                      >
                        <span className={styles.links}>
                          {translate('Links')}
                        </span>
                      </Label>
                    }
                    contentRole="dialog"
                    tooltip={
                      <GameDetailsLinks
                        igdbId={igdbId}
                        igdbSlug={igdbSlug}
                        youTubeTrailerId={youTubeTrailerId}
                        websites={websites}
                      />
                    }
                    kind={kinds.DEFAULT}
                    position={tooltipPositions.BOTTOM}
                  />

                  {tags.length ? (
                    <Tooltip
                      anchor={
                        <Label
                          className={styles.detailsLabel}
                          size={sizes.LARGE}
                          icon={icons.TAGS}
                          interactive={true}
                        >
                          <span className={styles.tags}>
                            {translate('Tags')}
                          </span>
                        </Label>
                      }
                      tooltip={<GameTags gameId={id} />}
                      kind={kinds.DEFAULT}
                      position={tooltipPositions.BOTTOM}
                    />
                  ) : null}
                </div>

                <div className={styles.chipsRow}>
                  <span title={statusDetails.message}>
                    <GameStatusLabel
                      className={styles.detailsLabel}
                      gameId={id}
                      monitored={monitored}
                      isAvailable={isAvailable}
                      hasGameFiles={hasGameFiles}
                      status={status}
                      useLabel={true}
                    />
                  </span>

                  <Label
                    className={styles.detailsLabel}
                    title={translate('SizeOnDisk')}
                    size={sizes.LARGE}
                    icon={icons.DRIVE}
                  >
                    <span className={styles.sizeOnDisk}>
                      {formatBytes(sizeOnDisk)}
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    title={translate('QualityProfile')}
                    size={sizes.LARGE}
                    icon={icons.PROFILE}
                  >
                    <span className={styles.qualityProfileName}>
                      <QualityProfileName qualityProfileId={qualityProfileId} />
                    </span>
                  </Label>

                  {isMultiPlatform && titleSummary ? (
                    <Label
                      className={styles.detailsLabel}
                      title={titleSummary.versions
                        .map((v) => v.tooltip)
                        .join('\n')}
                      size={sizes.LARGE}
                      icon={icons.DISC}
                    >
                      <span className={styles.platformsPresent}>
                        {getPlatformsPresentText(titleSummary)}
                      </span>
                    </Label>
                  ) : null}

                  {collection ? (
                    <Label
                      className={styles.detailsLabel}
                      title={translate('Series')}
                      size={sizes.LARGE}
                      dot={false}
                    >
                      <span className={styles.collection}>
                        <GameCollectionLabel
                          igdbId={collection.igdbId}
                          title={collection.title}
                        />
                      </span>
                    </Label>
                  ) : null}

                  {developers.length ? (
                    <Label
                      className={styles.detailsLabel}
                      title={translate('Developers')}
                      size={sizes.LARGE}
                      icon={icons.STUDIO}
                    >
                      <span className={styles.network}>
                        {developers.join(', ')}
                      </span>
                    </Label>
                  ) : null}

                  {publishers.length &&
                  publishers.join() !== developers.join() ? (
                    <Label
                      className={styles.detailsLabel}
                      title={translate('Publishers')}
                      size={sizes.LARGE}
                      icon={icons.STUDIO}
                    >
                      <span className={styles.network}>
                        {publishers.join(', ')}
                      </span>
                    </Label>
                  ) : null}
                </div>
              </div>

              <div className={styles.heroFooter}>
                <GameDetailsPath path={path} />

                <MetadataAttribution />
              </div>
            </div>
          </div>
        </div>

        <div className={styles.contentContainer}>
          {overview ? (
            <section className={styles.overviewSection}>
              <div className={styles.overviewFrame} aria-hidden="true">
                <span className={styles.overviewLabel}>
                  {translate('Overview')}
                </span>
              </div>

              <p
                ref={overviewTextRef}
                className={classNames(
                  styles.overview,
                  !isOverviewExpanded && styles.overviewClamped
                )}
              >
                {overview}
              </p>

              {isOverviewClamped || isOverviewExpanded ? (
                <Link
                  className={styles.overviewToggle}
                  onPress={handleToggleOverview}
                >
                  {isOverviewExpanded ? translate('Less') : translate('More')}
                </Link>
              ) : null}
            </section>
          ) : null}

          {!isFetching && gameFilesError ? (
            <Alert kind={kinds.DANGER}>
              {translate('LoadingGameFilesFailed')}
            </Alert>
          ) : null}

          {!isFetching && extraFilesError ? (
            <Alert kind={kinds.DANGER}>
              {translate('LoadingGameExtraFilesFailed')}
            </Alert>
          ) : null}

          {!isFetching && gameCreditsError ? (
            <Alert kind={kinds.DANGER}>
              {translate('LoadingGameCreditsFailed')}
            </Alert>
          ) : null}

          <FieldSet legend={translate('Details')}>
            <GameDetailsInfo game={game} />
          </FieldSet>

          {screenshots.length ? (
            <FieldSet legend={translate('Screenshots')}>
              <GameScreenshots screenshots={screenshots} />
            </FieldSet>
          ) : null}

          <FieldSet legend={translate('Files')}>
            {isMultiPlatform ? (
              <div className={styles.filesScope}>
                <Button
                  size={sizes.SMALL}
                  kind={showAllPlatformFiles ? kinds.DEFAULT : kinds.PRIMARY}
                  onPress={handleThisPlatformFilesPress}
                >
                  {platform?.name ?? translate('Platform')}
                </Button>

                <Button
                  size={sizes.SMALL}
                  kind={showAllPlatformFiles ? kinds.PRIMARY : kinds.DEFAULT}
                  onPress={handleAllPlatformFilesPress}
                >
                  {translate('AllPlatforms')}
                </Button>
              </div>
            ) : null}

            <GameFileEditorTable
              gameIds={
                isMultiPlatform && showAllPlatformFiles ? titleGameIds : [id]
              }
            />

            {showAllPlatformFiles && isMultiPlatform ? null : (
              <ExtraFileTable gameId={id} />
            )}
          </FieldSet>

          <FieldSet legend={translate('Companies')}>
            <GameCompanyCredits />
          </FieldSet>

          <FieldSet legend={translate('Titles')}>
            <GameTitlesTable gameId={id} />
          </FieldSet>
        </div>

        <OrganizePreviewModal
          isOpen={isOrganizeModalOpen}
          gameId={id}
          onModalClose={handleOrganizeModalClose}
        />

        <EditGameModal
          isOpen={isEditGameModalOpen}
          gameId={id}
          onModalClose={handleEditGameModalClose}
          onDeleteGamePress={handleDeleteGamePress}
        />

        <GameHistoryModal
          isOpen={isGameHistoryModalOpen}
          gameId={id}
          onModalClose={handleGameHistoryModalClose}
        />

        <DeleteGameModal
          isOpen={isDeleteGameModalOpen}
          gameId={id}
          titleGameIds={titleGameIds}
          onModalClose={handleDeleteGameModalClose}
        />

        <AddGamePlatformModal
          isOpen={isAddPlatformModalOpen}
          gameId={id}
          onModalClose={handleAddPlatformModalClose}
        />

        <InteractiveImportModal
          isOpen={isManageGamesModalOpen}
          gameId={id}
          title={title}
          folder={path}
          initialSortKey="relativePath"
          initialSortDirection={sortDirections.ASCENDING}
          showGame={false}
          allowGameChange={false}
          showDelete={true}
          showImportMode={false}
          modalTitle={translate('ManageFiles')}
          onModalClose={handleManageGamesModalClose}
        />

        <GameInteractiveSearchModal
          isOpen={isInteractiveSearchModalOpen}
          gameId={id}
          onModalClose={handleInteractiveSearchModalClose}
        />
      </PageContentBody>
    </PageContent>
  );
}

export default GameDetails;
