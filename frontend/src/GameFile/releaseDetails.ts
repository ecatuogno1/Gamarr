// Display names of the release details of game files (the API sends camelCase enum values)
const regionNames: Record<string, string> = {
  unknown: 'Unknown',
  world: 'World',
  usa: 'USA',
  europe: 'Europe',
  japan: 'Japan',
  asia: 'Asia',
  australia: 'Australia',
  brazil: 'Brazil',
  canada: 'Canada',
  china: 'China',
  france: 'France',
  germany: 'Germany',
  hongKong: 'Hong Kong',
  italy: 'Italy',
  korea: 'Korea',
  netherlands: 'Netherlands',
  russia: 'Russia',
  spain: 'Spain',
  sweden: 'Sweden',
  taiwan: 'Taiwan',
  unitedKingdom: 'UK',
  scandinavia: 'Scandinavia',
  latinAmerica: 'Latin America',
  mexico: 'Mexico',
  poland: 'Poland',
  portugal: 'Portugal',
  denmark: 'Denmark',
  finland: 'Finland',
  norway: 'Norway',
  greece: 'Greece',
  india: 'India',
  belgium: 'Belgium',
  switzerland: 'Switzerland',
  austria: 'Austria',
  newZealand: 'New Zealand',
  southAfrica: 'South Africa',
};

const releaseTypeNames: Record<string, string> = {
  base: 'Base Game',
  update: 'Update',
  dlc: 'DLC',
  bundle: 'Bundle',
  hack: 'Hack',
  translation: 'Translation',
  homebrew: 'Homebrew',
  demo: 'Demo',
  prototype: 'Prototype',
};

export function getRegionName(region: string) {
  return regionNames[region] ?? region;
}

export function getRegionNames(regions?: string[]) {
  return (regions ?? []).map(getRegionName).join(', ');
}

export function getReleaseTypeName(releaseType?: string) {
  if (!releaseType) {
    return '';
  }

  return releaseTypeNames[releaseType] ?? releaseType;
}

export function formatVersion(version?: string) {
  if (!version) {
    return '';
  }

  return /^\d/.test(version) ? `v${version}` : version;
}
