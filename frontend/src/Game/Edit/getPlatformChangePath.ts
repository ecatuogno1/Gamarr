// The game's path after a platform change, mirroring the backend: in the standard layout
// ({Root}/{Platform Folder}/{Game Folder}) the platform folder is swapped for the new one.
// Returns undefined when the game isn't inside its platform's folder of the root folder.
function getPlatformChangePath(
  path: string,
  rootFolderPath: string | undefined,
  oldFolderName: string | undefined,
  newFolderName: string | undefined
) {
  if (!path || !rootFolderPath || !oldFolderName || !newFolderName) {
    return undefined;
  }

  const separator = path.includes('\\') && !path.includes('/') ? '\\' : '/';
  const root = rootFolderPath.replace(/[\\/]+$/, '');

  if (!path.toLowerCase().startsWith(`${root.toLowerCase()}${separator}`)) {
    return undefined;
  }

  const segments = path
    .substring(root.length + 1)
    .split(/[\\/]/)
    .filter((segment) => segment.length);

  if (
    segments.length < 2 ||
    segments[0].toLowerCase() !== oldFolderName.toLowerCase()
  ) {
    return undefined;
  }

  return [root, newFolderName, ...segments.slice(1)].join(separator);
}

export default getPlatformChangePath;
