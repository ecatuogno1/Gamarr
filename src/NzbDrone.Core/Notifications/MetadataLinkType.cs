using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Notifications
{
    public enum MetadataLinkType
    {
        [FieldOption(Label = "IGDB")]
        Igdb = 0
    }
}
