import PropTypes from 'prop-types';
import React from 'react';
import MenuContent from 'Components/Menu/MenuContent';
import SortMenu from 'Components/Menu/SortMenu';
import SortMenuItem from 'Components/Menu/SortMenuItem';
import { align, sortDirections } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

function DiscoverGameSortMenu(props) {
  const {
    sortKey,
    sortDirection,
    isDisabled,
    onSortSelect
  } = props;

  return (
    <SortMenu
      isDisabled={isDisabled}
      alignMenu={align.RIGHT}
    >
      <MenuContent>
        <SortMenuItem
          name="status"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('Status')}
        </SortMenuItem>

        <SortMenuItem
          name="sortTitle"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('Title')}
        </SortMenuItem>

        <SortMenuItem
          name="developers"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('Developers')}
        </SortMenuItem>

        <SortMenuItem
          name="year"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('Year')}
        </SortMenuItem>

        <SortMenuItem
          name="releaseDate"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('ReleaseDate')}
        </SortMenuItem>

        <SortMenuItem
          name="igdbRating"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('IgdbRating')}
        </SortMenuItem>

        <SortMenuItem
          name="criticRating"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('CriticRating')}
        </SortMenuItem>

      </MenuContent>
    </SortMenu>
  );
}

DiscoverGameSortMenu.propTypes = {
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  isDisabled: PropTypes.bool.isRequired,
  onSortSelect: PropTypes.func.isRequired
};

export default DiscoverGameSortMenu;
