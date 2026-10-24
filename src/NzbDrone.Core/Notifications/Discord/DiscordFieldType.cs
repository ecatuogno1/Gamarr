using System;

namespace NzbDrone.Core.Notifications.Discord
{
    public enum DiscordGrabFieldType
    {
        Overview,
        Rating,
        Genres,
        Quality,
        Group,
        Size,
        Links,
        Release,
        Poster,
        Fanart,
        Indexer,
        CustomFormats,
        CustomFormatScore,
        Tags
    }

    public enum DiscordImportFieldType
    {
        Overview,
        Rating,
        Genres,
        Quality,

        // Games have no media info
        [Obsolete]
        Codecs,
        Group,
        Size,
        Languages,
        [Obsolete]
        Subtitles,
        Links,
        Release,
        Poster,
        Fanart,
        Tags,
        CustomFormats,
        CustomFormatScore
    }

    public enum DiscordManualInteractionFieldType
    {
        Overview,
        Rating,
        Genres,
        Quality,
        Group,
        Size,
        Links,
        DownloadTitle,
        Poster,
        Fanart,
        Tags
    }
}
