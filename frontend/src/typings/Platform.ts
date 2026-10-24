import ModelBase from 'App/ModelBase';

export type PlatformFamily =
  | 'other'
  | 'nintendo'
  | 'sony'
  | 'microsoft'
  | 'sega'
  | 'pc'
  | 'arcade'
  | 'atari'
  | 'nec'
  | 'snk';

interface Platform extends ModelBase {
  name: string;
  shortName: string;
  igdbPlatformId: number;
  family: PlatformFamily;
  generation: number;
  folderName: string;
  fileExtensions: string[];
  newznabCategories: number[];
  enabled: boolean;
}

export default Platform;
