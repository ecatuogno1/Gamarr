using System.Collections.Generic;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Download;
using NzbDrone.Core.Games;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalGame
    {
        public LocalGame()
        {
            CustomFormats = new List<CustomFormat>();
            CompanionFiles = new List<string>();
            Regions = new List<Region>();
        }

        public string Path { get; set; }
        public long Size { get; set; }
        public ParsedGameInfo FileGameInfo { get; set; }
        public ParsedGameInfo DownloadClientGameInfo { get; set; }
        public DownloadClientItem DownloadItem { get; set; }
        public ParsedGameInfo FolderGameInfo { get; set; }
        public Game Game { get; set; }
        public List<DeletedGameFile> OldFiles { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; }
        public IndexerFlags IndexerFlags { get; set; }
        public bool ExistingFile { get; set; }
        public bool SceneSource { get; set; }
        public string ReleaseGroup { get; set; }
        public string Edition { get; set; }
        public string SceneName { get; set; }
        public List<CustomFormat> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public GrabbedReleaseInfo Release { get; set; }
        public bool ScriptImported { get; set; }
        public string FileNameBeforeRename { get; set; }
        public bool ShouldImportExtras { get; set; }
        public List<string> PossibleExtraFiles { get; set; }

        // The other files of a multi-file release (bin tracks, other discs, installer parts, update/DLC files).
        // They're imported next to the primary file, keeping their path relative to ReleaseRoot.
        public List<string> CompanionFiles { get; set; }
        public string ReleaseRoot { get; set; }

        public List<Region> Regions { get; set; }
        public string Version { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public int DiscNumber { get; set; }
        public int DiscCount { get; set; }

        public override string ToString()
        {
            return Path;
        }
    }
}
