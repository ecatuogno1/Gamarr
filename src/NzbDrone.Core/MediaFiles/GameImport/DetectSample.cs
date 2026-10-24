using System;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Games;

namespace NzbDrone.Core.MediaFiles.GameImport
{
    public interface IDetectSample
    {
        DetectSampleResult IsSample(GameMetadata game, string path);
    }

    // Games have no runtime, samples and demos are recognised by their name only
    public class DetectSample : IDetectSample
    {
        private static readonly Regex SampleRegex = new(@"(?:^|[-._ \[(])(?:sample|demo)(?:$|[-._ \])\d])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SampleFolderRegex = new(@"(?:^|[\\/])samples?(?:[\\/]|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly Logger _logger;

        public DetectSample(Logger logger)
        {
            _logger = logger;
        }

        public DetectSampleResult IsSample(GameMetadata game, string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            var directory = Path.GetDirectoryName(path) ?? string.Empty;

            // A game that has "demo" in its title isn't a sample of itself
            var title = game?.Title ?? string.Empty;
            var titleHasSampleWord = SampleRegex.IsMatch(title);

            if (!titleHasSampleWord && SampleRegex.IsMatch(fileName))
            {
                _logger.Debug("[{0}] appears to be a sample or demo", path);
                return DetectSampleResult.Sample;
            }

            if (SampleFolderRegex.IsMatch(Path.GetFileName(directory) ?? string.Empty) &&
                !string.Equals(Path.GetFileName(directory), title, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Debug("[{0}] is in a sample folder", path);
                return DetectSampleResult.Sample;
            }

            return DetectSampleResult.NotSample;
        }
    }
}
