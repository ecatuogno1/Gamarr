namespace NzbDrone.Core.RootFolders
{
    public class UnmappedFolder
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string RelativePath { get; set; }

        // The platform of the platform folder ("snes", "psx"...) the folder is in, 0 when it isn't in one
        public int PlatformId { get; set; }
    }
}
