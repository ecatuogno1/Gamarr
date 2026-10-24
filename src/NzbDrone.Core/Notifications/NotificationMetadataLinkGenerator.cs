using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Games;

namespace NzbDrone.Core.Notifications;

public static class NotificationMetadataLinkGenerator
{
    public static List<NotificationMetadataLink> GenerateLinks(Game game, IEnumerable<int> metadataLinks)
    {
        var links = new List<NotificationMetadataLink>();

        if (game == null)
        {
            return links;
        }

        foreach (var type in metadataLinks)
        {
            var linkType = (MetadataLinkType)type;

            var igdbUrl = game.GameMetadata?.Value?.IgdbUrl;

            if (linkType == MetadataLinkType.Igdb && igdbUrl.IsNotNullOrWhiteSpace())
            {
                links.Add(new NotificationMetadataLink(MetadataLinkType.Igdb, "IGDB", igdbUrl));
            }
        }

        return links;
    }
}
