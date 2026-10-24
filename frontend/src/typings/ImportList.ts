import { GameMonitor } from 'Game/Game';
import Provider from './Provider';

interface ImportList extends Provider {
  enable: boolean;
  enabled: boolean;
  enableAuto: boolean;
  qualityProfileId: number;
  platformId: number;
  minimumAvailability: string;
  rootFolderPath: string;
  monitor: GameMonitor;
  searchOnAdd: boolean;
  listType: string;
  listOrder: number;
  minRefreshInterval: string;
  name: string;
  tags: number[];
}

export default ImportList;
