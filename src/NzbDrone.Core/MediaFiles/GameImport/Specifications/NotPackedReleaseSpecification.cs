using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Platforms;

namespace NzbDrone.Core.MediaFiles.GameImport.Specifications
{
    // ROMs in a zip/7z/rar are fine (emulators load them), but a scene release packed in a RAR set has to be
    // extracted (usually by the download client) before it can be imported.
    public class NotPackedReleaseSpecification : IImportDecisionEngineSpecification
    {
        private static readonly Regex RarVolumeRegex = new(@"\.(?:r\d{2,3}|s\d{2}|part0*[2-9]\d*\.rar|\d{3})$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public NotPackedReleaseSpecification(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalGame localGame, DownloadClientItem downloadClientItem)
        {
            if (localGame.ExistingFile)
            {
                return ImportSpecDecision.Accept();
            }

            var extension = Path.GetExtension(localGame.Path);

            if (!".rar".Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                return ImportSpecDecision.Accept();
            }

            var folder = Path.GetDirectoryName(localGame.Path);
            var files = folder != null && _diskProvider.FolderExists(folder) ? _diskProvider.GetFiles(folder, false).ToList() : localGame.CompanionFiles;

            if (files.Any(f => RarVolumeRegex.IsMatch(f)))
            {
                _logger.Debug("{0} is part of a RAR set", localGame.Path);
                return ImportSpecDecision.Reject(ImportRejectionReason.PackedRelease, "Release is packed in a RAR set, it needs to be extracted before it can be imported");
            }

            if (localGame.Game?.Platform?.Family == PlatformFamily.PC)
            {
                _logger.Debug("{0} is a packed PC release", localGame.Path);
                return ImportSpecDecision.Reject(ImportRejectionReason.PackedRelease, "PC release is packed in a RAR archive, it needs to be extracted before it can be imported");
            }

            return ImportSpecDecision.Accept();
        }
    }
}
