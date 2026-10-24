namespace Gamarr.Api.V3.Games
{
    // Platform summary embedded in game resources, the full platform is available from /api/v3/platform
    public class GamePlatformResource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string FolderName { get; set; }
        public int IgdbPlatformId { get; set; }
        public string Slug { get; set; }
    }
}
