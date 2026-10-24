using NzbDrone.Core.Games;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Notifications
{
    public class DeleteMessage
    {
        public string Message { get; set; }
        public Game Game { get; set; }
        public GameFile GameFile { get; set; }

        public DeleteMediaFileReason Reason { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
