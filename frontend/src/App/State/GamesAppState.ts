import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import Column from 'Components/Table/Column';
import Game from 'Game/Game';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { Filter, FilterBuilderProp } from './AppState';

export interface GameIndexAppState {
  sortKey: string;
  sortDirection: SortDirection;
  secondarySortKey: string;
  secondarySortDirection: SortDirection;
  view: string;

  posterOptions: {
    detailedProgressBar: boolean;
    size: string;
    showPlatform: boolean;
    showTitle: boolean;
    showMonitored: boolean;
    showQualityProfile: boolean;
    showReleaseDate: boolean;
    showIgdbRating: boolean;
    showCriticRating: boolean;
    showTags: boolean;
    showSearchAction: boolean;
  };

  overviewOptions: {
    detailedProgressBar: boolean;
    size: string;
    showMonitored: boolean;
    showPlatform: boolean;
    showQualityProfile: boolean;
    showAdded: boolean;
    showPath: boolean;
    showSizeOnDisk: boolean;
    showTags: boolean;
    showSearchAction: boolean;
  };

  tableOptions: {
    showSearchAction: boolean;
  };

  selectedFilterKey: string;
  filterBuilderProps: FilterBuilderProp<Game>[];
  filters: Filter[];
  columns: Column[];
}

interface GamesAppState
  extends AppSectionState<Game>,
    AppSectionDeleteState,
    AppSectionSaveState {
  itemMap: Record<number, number>;

  deleteOptions: {
    addImportExclusion: boolean;
  };

  pendingChanges: Partial<Game>;
}

export default GamesAppState;
