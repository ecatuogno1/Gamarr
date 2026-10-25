import PropTypes from 'prop-types';
import React, { useEffect, useMemo, useRef } from 'react';
import VirtualTable from 'Components/Table/VirtualTable';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import DiscoverGameItemConnector from 'DiscoverGame/DiscoverGameItemConnector';
import { sortDirections } from 'Helpers/Props';
import getIndexOfFirstCharacter from 'Utilities/Array/getIndexOfFirstCharacter';
import DiscoverGameHeaderConnector from './DiscoverGameHeaderConnector';
import DiscoverGameRowConnector from './DiscoverGameRowConnector';

const ROW_HEIGHT = 38;

// Ported to the react-window VirtualTable the v5 shared layer ships (the
// react-virtualized `items` / `scroller` / `rowRenderer` API is gone).
function Row({ index, style, data }) {
  const {
    items,
    columns,
    selectedState,
    onSelectedChange
  } = data;

  const game = items[index];

  if (!game) {
    return null;
  }

  return (
    <VirtualTableRow style={style}>
      <DiscoverGameItemConnector
        key={game.igdbId}
        component={DiscoverGameRowConnector}
        columns={columns}
        gameId={game.igdbId}
        isSelected={selectedState[game.igdbId]}
        onSelectedChange={onSelectedChange}
      />
    </VirtualTableRow>
  );
}

Row.propTypes = {
  index: PropTypes.number.isRequired,
  style: PropTypes.object.isRequired,
  data: PropTypes.object.isRequired
};

function DiscoverGameTable(props) {
  const {
    items,
    columns,
    sortKey,
    sortDirection,
    jumpToCharacter,
    isSmallScreen,
    onSortPress,
    scroller,
    allSelected,
    allUnselected,
    onSelectAllChange,
    selectedState,
    onSelectedChange
  } = props;

  const listRef = useRef(null);
  const scrollerRef = useMemo(() => ({ current: scroller }), [scroller]);

  useEffect(() => {
    if (jumpToCharacter == null) {
      return;
    }

    const index = getIndexOfFirstCharacter(items, jumpToCharacter);

    if (index != null) {
      let scrollTop = index * ROW_HEIGHT;

      // Offset by the approximate size of the header + padding.
      if (scrollTop > 0) {
        scrollTop += 57;
      }

      listRef.current?.scrollTo(scrollTop);
      scroller?.scrollTo(0, scrollTop);
    }
  }, [jumpToCharacter, items, scroller]);

  const itemData = useMemo(() => {
    return {
      items,
      columns,
      selectedState,
      onSelectedChange
    };
  }, [items, columns, selectedState, onSelectedChange]);

  return (
    <VirtualTable
      Header={
        <DiscoverGameHeaderConnector
          columns={columns}
          sortKey={sortKey}
          sortDirection={sortDirection}
          onSortPress={onSortPress}
          allSelected={allSelected}
          allUnselected={allUnselected}
          onSelectAllChange={onSelectAllChange}
        />
      }
      itemCount={items.length}
      itemData={itemData}
      isSmallScreen={isSmallScreen}
      listRef={listRef}
      rowHeight={ROW_HEIGHT}
      Row={Row}
      scrollerRef={scrollerRef}
    />
  );
}

DiscoverGameTable.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  jumpToCharacter: PropTypes.string,
  isSmallScreen: PropTypes.bool.isRequired,
  scroller: PropTypes.instanceOf(Element).isRequired,
  onSortPress: PropTypes.func.isRequired,
  allSelected: PropTypes.bool.isRequired,
  allUnselected: PropTypes.bool.isRequired,
  selectedState: PropTypes.object.isRequired,
  onSelectedChange: PropTypes.func.isRequired,
  onSelectAllChange: PropTypes.func.isRequired
};

export default DiscoverGameTable;
