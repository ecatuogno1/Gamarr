using NzbDrone.Core.Extras.Files;

namespace NzbDrone.Core.Extras.Companions
{
    // A file that belongs to a multi-file game release (bin tracks of a cue sheet, other discs, installer
    // data, update/DLC files). It's moved, renamed and deleted together with its game file.
    public class CompanionFile : ExtraFile
    {
    }
}
