import ModelBase from 'App/ModelBase';
import { Image } from 'Game/Game';

export type GameCreditType =
  | 'developer'
  | 'publisher'
  | 'porting'
  | 'supporting';

// A company credit from IGDB involved companies
interface GameCredit extends ModelBase {
  companyIgdbId: number;
  companyName: string;
  images: Image[];
  type: GameCreditType;
  order: number;
}

export default GameCredit;
