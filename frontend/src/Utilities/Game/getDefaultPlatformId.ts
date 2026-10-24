import Platform from 'typings/Platform';

// Picks the Gamarr platform for a game that is added without the user choosing one:
// the preferred platform if set, otherwise the first enabled platform IGDB lists for the game.
function getDefaultPlatformId(
  platforms: Platform[],
  availablePlatformIgdbIds: number[] = [],
  preferredPlatformId = 0
) {
  if (preferredPlatformId > 0) {
    return preferredPlatformId;
  }

  for (const igdbPlatformId of availablePlatformIgdbIds) {
    const platform = platforms.find(
      (p) => p.enabled && p.igdbPlatformId === igdbPlatformId
    );

    if (platform) {
      return platform.id;
    }
  }

  return 0;
}

export default getDefaultPlatformId;
