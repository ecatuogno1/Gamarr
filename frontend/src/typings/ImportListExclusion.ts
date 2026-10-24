import ModelBase from 'App/ModelBase';

export default interface ImportListExclusion extends ModelBase {
  igdbId: number;
  gameTitle: string;
  gameYear: number;
  // 0: every platform
  platformId: number;
}
