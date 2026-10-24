namespace NzbDrone.Core.Games
{
    public enum GameStatusType
    {
        Deleted = -1,
        TBA = 0,       // Nothing announced yet (IGDB status rumored, or no release date)
        Announced = 1, // Announced, but the release date is in the future or unknown (also used for cancelled games)
        Released = 3,  // Released on at least one platform
    }
}
