import ModelBase from 'App/ModelBase';
import { AppSectionItemState } from 'App/State/AppSectionState';
import Game from 'Game/Game';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from 'typings/CustomFormat';

export interface ParsedGameInfo {
  releaseTitle: string;
  originalTitle: string;
  gameTitle: string;
  gameTitles: string[];
  year: number;
  quality: QualityModel;
  languages: Language[];
  releaseHash: string;
  releaseGroup?: string;
  edition?: string;
  igdbId?: number;
  platformId: number;
  platformIgdbId: number;
  regions: string[];
  version?: string;
  releaseType: string;
  discNumber: number;
  discCount: number;
  titleId?: string;
}

export interface ParseModel extends ModelBase {
  title: string;
  parsedGameInfo: ParsedGameInfo;
  game?: Game;
  languages?: Language[];
  customFormats?: CustomFormat[];
  customFormatScore?: number;
}

type ParseAppState = AppSectionItemState<ParseModel>;

export default ParseAppState;
