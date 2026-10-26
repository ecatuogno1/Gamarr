using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Extras.Companions
{
    // One or more companion files of a release weren't imported. The game is incomplete without them, so the import
    // isn't reported as done and its source files are kept.
    public class CompanionImportException : NzbDroneException
    {
        public CompanionImportException(List<string> failedFiles)
            : base("{0} companion file(s) couldn't be imported: {1}", failedFiles.Count, string.Join(", ", failedFiles.Take(5)))
        {
            FailedFiles = failedFiles;
        }

        public List<string> FailedFiles { get; }
    }
}
