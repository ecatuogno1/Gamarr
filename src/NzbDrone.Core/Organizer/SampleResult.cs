using NzbDrone.Core.Games;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Organizer
{
    public class SampleResult
    {
        public string FileName { get; set; }
        public Game Game { get; set; }
        public GameFile GameFile { get; set; }
    }
}
