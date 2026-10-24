import Game, { GameAvailability, GameMonitor } from 'Game/Game';

interface NewGamePayload {
  rootFolderPath: string;
  monitor: GameMonitor;
  qualityProfileId: number;
  minimumAvailability: GameAvailability;
  tags: number[];
  searchForGame?: boolean;
  platformId?: number;
}

function getNewGame(game: Game, payload: NewGamePayload) {
  const {
    rootFolderPath,
    monitor,
    qualityProfileId,
    minimumAvailability,
    tags,
    searchForGame = false,
    platformId,
  } = payload;

  const addOptions = {
    monitor,
    searchForGame,
  };

  game.addOptions = addOptions;
  game.monitored = monitor !== 'none';
  game.qualityProfileId = qualityProfileId;
  game.minimumAvailability = minimumAvailability;
  game.rootFolderPath = rootFolderPath;
  game.tags = tags;

  // Lists and collections may not know the platform, the API then picks the first enabled IGDB platform
  if (platformId) {
    game.platformId = platformId;
  }

  return game;
}

export default getNewGame;
