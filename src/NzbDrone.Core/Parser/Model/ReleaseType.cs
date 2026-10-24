using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Parser.Model
{
    // What a release contains. The values are persisted (custom formats, game files), never renumber them.
    public enum ReleaseType
    {
        Base = 0,
        Update = 1,
        Dlc = 2,
        Bundle = 3,
        Hack = 4,
        Translation = 5,
        Homebrew = 6,
        Demo = 7,
        Prototype = 8
    }

    public static class ReleaseTypeExtensions
    {
        private static readonly Dictionary<ReleaseType, string> Names = new()
        {
            { ReleaseType.Base, "Base Game" },
            { ReleaseType.Update, "Update" },
            { ReleaseType.Dlc, "DLC" },
            { ReleaseType.Bundle, "Bundle" },
            { ReleaseType.Hack, "Hack" },
            { ReleaseType.Translation, "Translation" },
            { ReleaseType.Homebrew, "Homebrew" },
            { ReleaseType.Demo, "Demo" },
            { ReleaseType.Prototype, "Prototype" }
        };

        public static string GetName(this ReleaseType releaseType)
        {
            return Names.TryGetValue(releaseType, out var name) ? name : releaseType.ToString();
        }

        // Release types that aren't a playable copy of the game on their own (or are modified copies).
        // They're rejected for a game unless a custom format scores them.
        public static bool IsRejectedByDefault(this ReleaseType releaseType)
        {
            return releaseType is ReleaseType.Update or ReleaseType.Dlc or ReleaseType.Demo or ReleaseType.Hack or ReleaseType.Translation or ReleaseType.Prototype;
        }
    }

    public class ReleaseTypeFieldConverter : ISelectOptionsConverter
    {
        public List<SelectOption> GetSelectOptions()
        {
            return System.Enum.GetValues<ReleaseType>()
                .Select(r => new SelectOption { Value = (int)r, Name = r.GetName(), Order = (int)r })
                .ToList();
        }
    }
}
