using System.IO;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Parser;

public static class ReleaseGroupParser
{
    // Release group of a release name or file path: the scene group ("-CODEX") or the repack/store tag
    // ("[FitGirl Repack]", "[GOG]"). For paths the file name is tried first, then the folder name.
    public static string ParseReleaseGroup(string title)
    {
        if (title.IsNullOrWhiteSpace())
        {
            return null;
        }

        title = title.Trim();

        if (title.IndexOfAny(new[] { '/', '\\' }) >= 0)
        {
            var group = Parser.ParseReleaseGroup(Path.GetFileName(title));

            if (group.IsNullOrWhiteSpace())
            {
                var folder = Path.GetFileName(Path.GetDirectoryName(title));

                if (folder.IsNotNullOrWhiteSpace())
                {
                    group = Parser.ParseReleaseGroup(folder);
                }
            }

            return group;
        }

        return Parser.ParseReleaseGroup(title);
    }
}
