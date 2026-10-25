import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageJumpBar from 'Components/Page/PageJumpBar';
import { OverflowDivider } from 'Components/Page/Toolbar/Overflow';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import PageToolbarSpacer from 'Components/Page/Toolbar/PageToolbarSpacer';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import styles from 'Game/Index/GameIndex.css';
import { icons, kinds, sortDirections } from 'Helpers/Props';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import CollectionFooter from './CollectionFooter';
import GameCollectionFilterMenu from './Menus/GameCollectionFilterMenu';
import GameCollectionSortMenu from './Menus/GameCollectionSortMenu';
import NoGameCollections from './NoGameCollections';
import CollectionOverviewsConnector from './Overview/CollectionOverviewsConnector';
import CollectionOverviewOptionsModal from './Overview/Options/CollectionOverviewOptionsModal';

function getViewComponent(view) {
  return CollectionOverviewsConnector;
}

class Collection extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.scrollerRef = React.createRef();

    this.state = {
      jumpBarItems: { order: [] },
      jumpToCharacter: null,
      isPosterOptionsModalOpen: false,
      isOverviewOptionsModalOpen: false,
      isConfirmSearchModalOpen: false,
      searchType: null,
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {}
    };
  }

  componentDidMount() {
    this.setJumpBarItems();
    this.setSelectedState();
  }

  componentDidUpdate(prevProps) {
    const {
      items,
      sortKey,
      sortDirection
    } = this.props;

    if (sortKey !== prevProps.sortKey ||
        sortDirection !== prevProps.sortDirection ||
        hasDifferentItemsOrOrder(prevProps.items, items)
    ) {
      this.setJumpBarItems();
      this.setSelectedState();
    }

    if (this.state.jumpToCharacter != null) {
      this.setState({ jumpToCharacter: null });
    }
  }

  //
  // Control

  getSelectedIds = () => {
    if (this.state.allUnselected) {
      return [];
    }
    return getSelectedIds(this.state.selectedState);
  };

  setSelectedState() {
    const {
      items
    } = this.props;

    const {
      selectedState
    } = this.state;

    const newSelectedState = {};

    items.forEach((collection) => {
      const isItemSelected = selectedState[collection.id];

      if (isItemSelected) {
        newSelectedState[collection.id] = isItemSelected;
      } else {
        newSelectedState[collection.id] = false;
      }
    });

    const selectedCount = getSelectedIds(newSelectedState).length;
    const newStateCount = Object.keys(newSelectedState).length;
    let isAllSelected = false;
    let isAllUnselected = false;

    if (selectedCount === 0) {
      isAllUnselected = true;
    } else if (selectedCount === newStateCount) {
      isAllSelected = true;
    }

    this.setState({ selectedState: newSelectedState, allSelected: isAllSelected, allUnselected: isAllUnselected });
  }

  setJumpBarItems() {
    const {
      items,
      sortKey,
      sortDirection
    } = this.props;

    // Reset if not sorting by sortTitle
    if (sortKey !== 'sortTitle') {
      this.setState({ jumpBarItems: { order: [] } });
      return;
    }

    const characters = _.reduce(items, (acc, item) => {
      let char = item.sortTitle.charAt(0);

      if (!isNaN(char)) {
        char = '#';
      }

      if (char in acc) {
        acc[char] = acc[char] + 1;
      } else {
        acc[char] = 1;
      }

      return acc;
    }, {});

    const order = Object.keys(characters).sort();

    // Reverse if sorting descending
    if (sortDirection === sortDirections.DESCENDING) {
      order.reverse();
    }

    const jumpBarItems = {
      characters,
      order
    };

    this.setState({ jumpBarItems });
  }

  //
  // Listeners

  onOverviewOptionsPress = () => {
    this.setState({ isOverviewOptionsModalOpen: true });
  };

  onOverviewOptionsModalClose = () => {
    this.setState({ isOverviewOptionsModalOpen: false });
  };

  onJumpBarItemPress = (jumpToCharacter) => {
    this.setState({ jumpToCharacter });
  };

  onSelectAllChange = ({ value }) => {
    this.setState(selectAll(this.state.selectedState, value));
  };

  onSelectAllPress = () => {
    this.onSelectAllChange({ value: !this.state.allSelected });
  };

  onRefreshGameCollectionsPress = () => {
    this.props.onRefreshGameCollectionsPress();
  };

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey, 'id');
    });
  };

  onUpdateSelectedPress = (changes) => {
    this.props.onUpdateSelectedPress({
      collectionIds: this.getSelectedIds(),
      ...changes
    });
  };

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      totalItems,
      items,
      selectedFilterKey,
      filters,
      customFilters,
      sortKey,
      sortDirection,
      view,
      onSortSelect,
      onFilterSelect,
      initialScrollTop,
      onScroll,
      isRefreshingCollections,
      isSaving,
      isAdding,
      ...otherProps
    } = this.props;

    const {
      jumpBarItems,
      jumpToCharacter,
      isOverviewOptionsModalOpen,
      selectedState,
      allSelected,
      allUnselected
    } = this.state;

    const selectedGameIds = this.getSelectedIds();

    const ViewComponent = getViewComponent(view);
    const isLoaded = !!(!error && isPopulated && items.length && this.scrollerRef.current);
    const hasNoCollection = !totalItems;

    return (
      <PageContent title={translate('Collections')}>
        <PageToolbar>
          <ToolbarItem
            id="refreshCollections"
            priority={1}
            groupId="left"
            label={translate('RefreshCollections')}
            iconName={icons.REFRESH}
            isSpinning={isRefreshingCollections}
            isDisabled={hasNoCollection}
            onPress={this.onRefreshGameCollectionsPress}
          />

          <ToolbarItem
            id="selectAll"
            priority={1}
            groupId="left"
            label={allSelected ? translate('UnselectAll') : translate('SelectAll')}
            iconName={icons.CHECK_SQUARE}
            isDisabled={hasNoCollection}
            onPress={this.onSelectAllPress}
          />

          <PageToolbarSpacer />

          {
            view === 'overview' ?
              <ToolbarItem
                id="options"
                priority={2}
                groupId="right"
                label={translate('Options')}
                iconName={icons.OVERVIEW}
                onPress={this.onOverviewOptionsPress}
              /> :
              null
          }

          {
            view === 'overview' ?
              <OverflowDivider groupId="right">
                <PageToolbarSeparator />
              </OverflowDivider> :
              null
          }

          <ToolbarItem id="sort" pinned={true}>
            <GameCollectionSortMenu
              sortKey={sortKey}
              sortDirection={sortDirection}
              isDisabled={hasNoCollection}
              onSortSelect={onSortSelect}
            />
          </ToolbarItem>

          <ToolbarItem id="filter" pinned={true}>
            <GameCollectionFilterMenu
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              isDisabled={hasNoCollection}
              onFilterSelect={onFilterSelect}
            />
          </ToolbarItem>
        </PageToolbar>

        <div className={styles.pageContentBodyWrapper}>
          <PageContentBody
            ref={this.scrollerRef}
            className={styles.contentBody}
            innerClassName={styles[`${view}InnerContentBody`]}
            onScroll={onScroll}
          >
            {
              isFetching && !isPopulated &&
                <LoadingIndicator />
            }

            {
              !isFetching && !!error &&
                <Alert kind={kinds.DANGER}>
                  {translate('UnableToLoadCollections')}
                </Alert>
            }

            {
              isLoaded &&
                <div className={styles.contentBodyContainer}>
                  <ViewComponent
                    scroller={this.scrollerRef.current}
                    items={items}
                    filters={filters}
                    sortKey={sortKey}
                    sortDirection={sortDirection}
                    jumpToCharacter={jumpToCharacter}
                    allSelected={allSelected}
                    allUnselected={allUnselected}
                    onSelectedChange={this.onSelectedChange}
                    onSelectAllChange={this.onSelectAllChange}
                    selectedState={selectedState}
                    scrollTop={initialScrollTop}
                    {...otherProps}
                  />
                </div>
            }

            {
              !error && isPopulated && !items.length &&
                <NoGameCollections totalItems={totalItems} />
            }
          </PageContentBody>

          {
            isLoaded && !!jumpBarItems.order.length &&
              <PageJumpBar
                items={jumpBarItems}
                onItemPress={this.onJumpBarItemPress}
              />
          }
        </div>

        {
          isLoaded &&
            <CollectionFooter
              selectedIds={selectedGameIds}
              isSaving={isSaving}
              isAdding={isAdding}
              onUpdateSelectedPress={this.onUpdateSelectedPress}
            />
        }

        <CollectionOverviewOptionsModal
          isOpen={isOverviewOptionsModalOpen}
          onModalClose={this.onOverviewOptionsModalClose}
        />
      </PageContent>
    );
  }
}

Collection.propTypes = {
  initialScrollTop: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  isAdding: PropTypes.bool.isRequired,
  error: PropTypes.object,
  totalItems: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  view: PropTypes.string.isRequired,
  isRefreshingCollections: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onSortSelect: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired,
  onUpdateSelectedPress: PropTypes.func.isRequired,
  onRefreshGameCollectionsPress: PropTypes.func.isRequired
};

export default Collection;
