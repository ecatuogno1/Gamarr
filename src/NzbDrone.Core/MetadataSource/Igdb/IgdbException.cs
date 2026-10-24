using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.MetadataSource.Igdb
{
    public class IgdbException : NzbDroneException
    {
        public IgdbException(string message)
            : base(message)
        {
        }

        public IgdbException(string message, params object[] args)
            : base(message, args)
        {
        }

        public IgdbException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }

    public class IgdbAuthenticationException : IgdbException
    {
        public IgdbAuthenticationException(string message, params object[] args)
            : base(message, args)
        {
        }
    }

    public class IgdbCredentialsMissingException : IgdbException
    {
        public IgdbCredentialsMissingException()
            : base("IGDB credentials are missing. Add a Twitch Client ID and Client Secret in Settings > Metadata.")
        {
        }
    }
}
