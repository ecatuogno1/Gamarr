namespace NzbDrone.Core.Qualities
{
    // What kind of media a quality describes. Games have no resolution, the source is the medium the
    // dump or release comes from.
    public enum QualitySource
    {
        UNKNOWN = 0,
        CARTRIDGE = 1,
        DISC = 2,
        DIGITAL = 3,
        PC = 4
    }
}
