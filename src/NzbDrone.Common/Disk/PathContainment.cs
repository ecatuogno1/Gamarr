using System;
using System.IO;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Disk
{
    // Checks that a path stays inside a folder, after resolving "..", "." and symbolic links (of the file and of
    // every folder on the way), so a path taken from an untrusted file (a cue sheet, an m3u playlist, a game list)
    // can't reach outside the folder it belongs to.
    public static class PathContainment
    {
        private const int MaxLinkHops = 40;

        // The canonical path with every existing symbolic link resolved. Parts that don't exist are kept as they are.
        public static string GetRealPath(string path)
        {
            var hops = 0;

            return Resolve(Path.GetFullPath(path), ref hops);
        }

        // Whether path is strictly below root, both lexically and once symbolic links are resolved
        public static bool IsWithin(string root, string path)
        {
            if (root.IsNullOrWhiteSpace() || path.IsNullOrWhiteSpace())
            {
                return false;
            }

            try
            {
                var fullRoot = Path.GetFullPath(root);
                var fullPath = Path.GetFullPath(path);

                if (!fullRoot.IsParentPath(fullPath))
                {
                    return false;
                }

                return GetRealPath(fullRoot).IsParentPath(GetRealPath(fullPath));
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Whether path is root itself or below it
        public static bool IsWithinOrEqual(string root, string path)
        {
            if (root.IsNullOrWhiteSpace() || path.IsNullOrWhiteSpace())
            {
                return false;
            }

            try
            {
                if (Path.GetFullPath(root).PathEquals(Path.GetFullPath(path)))
                {
                    return GetRealPath(root).PathEquals(GetRealPath(path));
                }

                return IsWithin(root, path);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string Resolve(string fullPath, ref int hops)
        {
            var root = Path.GetPathRoot(fullPath) ?? string.Empty;
            var parts = fullPath.Substring(root.Length).Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var current = root;

            foreach (var part in parts)
            {
                var next = Path.Combine(current, part);
                FileSystemInfo info = Directory.Exists(next) ? new DirectoryInfo(next) : new FileInfo(next);
                var target = info.LinkTarget;

                if (target == null)
                {
                    current = next;
                    continue;
                }

                if (++hops > MaxLinkHops)
                {
                    throw new IOException($"Too many levels of symbolic links: {fullPath}");
                }

                current = Resolve(Path.GetFullPath(Path.IsPathRooted(target) ? target : Path.Combine(current, target)), ref hops);
            }

            return current;
        }
    }
}
