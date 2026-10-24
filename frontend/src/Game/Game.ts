import ModelBase from 'App/ModelBase';
import { GameFile } from 'GameFile/GameFile';
import Language from 'Language/Language';
import Quality from 'Quality/Quality';

export type GameMonitor = 'gameOnly' | 'gameAndCollection' | 'none';

export type GameStatus = 'tba' | 'announced' | 'released' | 'deleted';

export type GameAvailability = 'announced' | 'released';

export type CoverType =
  | 'poster'
  | 'fanart'
  | 'screenshot'
  | 'headshot'
  | 'clearlogo';

export interface Image {
  coverType: CoverType;
  url: string;
  remoteUrl: string;
}

export interface Collection {
  igdbId: number;
  title: string;
}

export interface Statistics {
  gameFileCount: number;
  releaseGroups: string[];
  gameFileQualities: Quality[];
  sizeOnDisk: number;
}

export interface RatingValues {
  votes: number;
  value: number;
}

export interface Ratings {
  igdb?: RatingValues;
  critic?: RatingValues;
}

export interface GamePlatform {
  id: number;
  name: string;
  shortName: string;
  folderName: string;
  igdbPlatformId: number;
}

export interface AgeRating {
  organization: string;
  rating: string;
}

export interface GameWebsite {
  url: string;
  type: string;
}

export interface PlatformReleaseDate {
  igdbPlatformId: number;
  date?: string;
}

export interface AlternativeTitle extends ModelBase {
  sourceType: string;
  title: string;
}

export interface GameAddOptions {
  monitor: GameMonitor;
  searchForGame: boolean;
}

interface Game extends ModelBase {
  igdbId: number;
  igdbSlug?: string;
  igdbUrl?: string;
  sortTitle: string;
  overview: string;
  storyline?: string;
  youTubeTrailerId?: string;
  monitored: boolean;
  status: GameStatus;
  title: string;
  titleSlug: string;
  originalTitle: string;
  originalLanguage: Language;
  collection: Collection;
  alternateTitles: AlternativeTitle[];
  developers: string[];
  publishers: string[];
  qualityProfileId: number;
  platformId: number;
  platform?: GamePlatform;
  availablePlatforms?: number[];
  availablePlatformIgdbIds?: number[];
  added: string;
  year: number;
  releaseDate?: string;
  firstReleaseDate?: string;
  platformReleaseDates?: PlatformReleaseDate[];
  rootFolderPath: string;
  minimumAvailability: GameAvailability;
  path: string;
  genres: string[];
  themes: string[];
  gameModes: string[];
  keywords: string[];
  ageRatings: AgeRating[];
  websites: GameWebsite[];
  website?: string;
  ratings: Ratings;
  popularity: number;
  statistics?: Statistics;
  tags: number[];
  images: Image[];
  gameFileId: number;
  gameFile?: GameFile;
  hasFile: boolean;
  grabbed?: boolean;
  lastSearchTime?: string;
  isAvailable: boolean;
  isSaving?: boolean;
  addOptions: GameAddOptions;
}

export default Game;
