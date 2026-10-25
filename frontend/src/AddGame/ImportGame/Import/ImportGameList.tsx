import { throttle } from 'lodash';
import React, {
  RefObject,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { ListChildComponentProps, VariableSizeList } from 'react-window';
import useMeasure from 'Helpers/Hooks/useMeasure';
import usePrevious from 'Helpers/Hooks/usePrevious';
import {
  queueLookupGame,
  setImportGameValue,
} from 'Store/Actions/importGameActions';
import createAllGamesSelector from 'Store/Selectors/createAllGamesSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { CheckInputChanged } from 'typings/inputs';
import ImportGameCard from './ImportGameCard';
import {
  getImportGameEligibility,
  ImportGameAppState,
  ImportGameEligibility,
  ImportGameItem,
  UnmappedFolder,
} from './importGameTypes';
import styles from './ImportGameList.css';

const ITEM_HEIGHT = 112;
const ITEM_HEIGHT_STACKED = 140;
const ITEM_HEIGHT_STACKED_UNMATCHED = 80;
const STACKED_WIDTH = 960;

interface Row {
  item: ImportGameItem;
  eligibility: ImportGameEligibility;
}

interface ItemData {
  rows: Row[];
  isStacked: boolean;
  selectedState: Record<string, boolean>;
  onSelectedChange: (change: CheckInputChanged & { id: string }) => void;
}

interface ImportGameListProps {
  unmappedFolders: UnmappedFolder[];
  selectedState: Record<string, boolean>;
  scrollerRef: RefObject<HTMLElement>;
  onSelectedChange: (change: {
    id: string;
    value: boolean;
    shiftKey?: boolean;
  }) => void;
  onRemoveSelectedStateItem: (id: string) => void;
}

function getWindowScrollTopPosition() {
  return document.documentElement.scrollTop || document.body.scrollTop || 0;
}

function Item({ index, style, data }: ListChildComponentProps<ItemData>) {
  const { rows, isStacked, selectedState, onSelectedChange } = data;
  const row = rows[index];

  if (!row) {
    return null;
  }

  return (
    <div style={style} className={styles.item}>
      <ImportGameCard
        item={row.item}
        eligibility={row.eligibility}
        isSelected={!!selectedState[row.item.id]}
        isStacked={isStacked}
        onSelectedChange={onSelectedChange}
      />
    </div>
  );
}

function ImportGameList({
  unmappedFolders,
  selectedState,
  scrollerRef,
  onSelectedChange,
  onRemoveSelectedStateItem,
}: ImportGameListProps) {
  const dispatch = useDispatch();

  const items = useSelector(
    (state: ImportGameAppState) => state.importGame.items
  );
  const defaults = useSelector(
    (state: ImportGameAppState) => state.addGame.defaults
  );
  const allGames = useSelector(createAllGamesSelector());
  const {
    width: windowWidth,
    height: windowHeight,
    isSmallScreen,
  } = useSelector(createDimensionsSelector());

  const listRef = useRef<VariableSizeList<ItemData>>(null);
  const [measureRef, bounds] = useMeasure();
  const [size, setSize] = useState({ width: 0, height: 0 });

  const existingIgdbIds = useMemo(() => {
    return new Set(allGames.map((game) => game.igdbId));
  }, [allGames]);

  // Queue a lookup for every folder and apply the add defaults once, when the
  // page opens (same as Gamarr's previous ImportGameTable).
  useEffect(() => {
    const { monitor, qualityProfileId, minimumAvailability } = defaults;

    unmappedFolders.forEach((unmappedFolder) => {
      // The same game can be in several platform folders, the relative path tells them apart
      const id = unmappedFolder.platformId
        ? unmappedFolder.relativePath
        : unmappedFolder.name;

      dispatch(
        queueLookupGame({
          name: id,
          path: unmappedFolder.path,
          relativePath: unmappedFolder.relativePath,
          platformId: unmappedFolder.platformId,
          term: unmappedFolder.name,
        })
      );

      dispatch(
        // @ts-expect-error actions aren't typed
        setImportGameValue({
          id,
          monitor,
          qualityProfileId,
          minimumAvailability,
        })
      );
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Keep the selection in step with matches. This isn't great, but it's the
  // most reliable way to ensure items are checked off even if they aren't
  // visible, since the rows are virtualized.
  const previousItems = usePrevious(items);

  useEffect(() => {
    if (!previousItems) {
      return;
    }

    previousItems.forEach((prevItem) => {
      const { id } = prevItem;
      const item = items.find((i) => i.id === id);

      if (!item) {
        onRemoveSelectedStateItem(id);
        return;
      }

      const selectedGame = item.selectedGame;
      const isSelected = selectedState[id];
      const isExistingGame =
        !!selectedGame && existingIgdbIds.has(selectedGame.igdbId);

      // No longer has a selected game, or the selected game is an existing game.
      if (
        (!selectedGame && prevItem.selectedGame) ||
        (isExistingGame && !prevItem.selectedGame)
      ) {
        onSelectedChange({ id, value: false });
        return;
      }

      // Selected, but no game is selected or the game is an existing game.
      if (isSelected && (!selectedGame || isExistingGame)) {
        onSelectedChange({ id, value: false });
        return;
      }

      // A game is being selected that wasn't previously selected.
      if (selectedGame && selectedGame !== prevItem.selectedGame) {
        onSelectedChange({ id, value: true });
      }
    });
  }, [
    items,
    previousItems,
    selectedState,
    existingIgdbIds,
    onSelectedChange,
    onRemoveSelectedStateItem,
  ]);

  const rows = useMemo(() => {
    return items
      .filter((item) => !!item.monitor)
      .map((item) => {
        return {
          item,
          eligibility: getImportGameEligibility(item, existingIgdbIds),
        };
      });
  }, [items, existingIgdbIds]);

  const isStacked = size.width > 0 && size.width < STACKED_WIDTH;

  const getItemSize = useCallback(
    (index: number) => {
      if (isStacked) {
        return rows[index]?.eligibility === 'ready'
          ? ITEM_HEIGHT_STACKED
          : ITEM_HEIGHT_STACKED_UNMATCHED;
      }

      return ITEM_HEIGHT;
    },
    [rows, isStacked]
  );

  const getItemKey = useCallback(
    (index: number, data: ItemData) => data.rows[index]?.item.id ?? index,
    []
  );

  useEffect(() => {
    listRef.current?.resetAfterIndex(0, true);
  }, [rows, isStacked]);

  useEffect(() => {
    if (isSmallScreen) {
      setSize({
        width: windowWidth,
        height: windowHeight,
      });

      return;
    }

    if (bounds.width) {
      setSize({
        width: bounds.width,
        height: windowHeight,
      });
    }
  }, [isSmallScreen, windowWidth, windowHeight, bounds]);

  useEffect(() => {
    const currentScrollerRef = scrollerRef.current as HTMLElement;
    const currentScrollListener = isSmallScreen ? window : currentScrollerRef;

    const handleScroll = throttle(() => {
      const { offsetTop = 0 } = currentScrollerRef;
      const scrollTop =
        (isSmallScreen
          ? getWindowScrollTopPosition()
          : currentScrollerRef.scrollTop) - offsetTop;

      listRef.current?.scrollTo(scrollTop);
    }, 10);

    currentScrollListener.addEventListener('scroll', handleScroll);

    return () => {
      handleScroll.cancel();

      if (currentScrollListener) {
        currentScrollListener.removeEventListener('scroll', handleScroll);
      }
    };
  }, [isSmallScreen, listRef, scrollerRef]);

  if (!rows.length) {
    return null;
  }

  return (
    <div ref={measureRef}>
      <VariableSizeList<ItemData>
        ref={listRef}
        style={{
          width: '100%',
          height: '100%',
          overflow: 'none',
        }}
        width={size.width}
        height={size.height}
        itemCount={rows.length}
        itemKey={getItemKey}
        itemSize={getItemSize}
        estimatedItemSize={isStacked ? ITEM_HEIGHT_STACKED : ITEM_HEIGHT}
        itemData={{
          rows,
          isStacked,
          selectedState,
          onSelectedChange,
        }}
        overscanCount={20}
      >
        {Item}
      </VariableSizeList>
    </div>
  );
}

export default ImportGameList;
