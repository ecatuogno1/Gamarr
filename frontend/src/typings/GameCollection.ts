import ModelBase from 'App/ModelBase';
import Game, { GameAvailability, Image } from 'Game/Game';

interface GameCollection extends ModelBase {
  igdbId: number;
  sortTitle: string;
  title: string;
  overview: string;
  monitored: boolean;
  minimumAvailability: GameAvailability;
  qualityProfileId: number;
  // 0: each game is added on the first enabled platform IGDB lists for it
  platformId: number;
  rootFolderPath: string;
  searchOnAdd: boolean;
  images: Image[];
  games: Game[];
  missingGames: number;
  tags: number[];
  isSaving?: boolean;
}

export default GameCollection;
