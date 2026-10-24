export enum QualitySource {
  Unknown = 'unknown',
  Cartridge = 'cartridge',
  Disc = 'disc',
  Digital = 'digital',
  PC = 'pc',
}

export interface Revision {
  version: number;
  real: number;
  isRepack: boolean;
}

interface Quality {
  id: number;
  name: string;
  source: QualitySource;
}

export interface QualityModel {
  quality: Quality;
  revision: Revision;
}

export default Quality;
