using System.Collections.Generic;
using System.Linq;
using System.Net;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ConfigSavedEvent))]
    public class ProxyCheck : HealthCheckBase
    {
        private readonly IConfigService _configService;

        public ProxyCheck(IConfigService configService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _configService = configService;
        }

        public override HealthCheck Check()
        {
            if (!_configService.ProxyEnabled)
            {
                return new HealthCheck(GetType());
            }

            var addresses = Dns.GetHostAddresses(_configService.ProxyHostname);

            if (!addresses.Any())
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.ProxyResolveIp,
                    _localizationService.GetLocalizedString("ProxyCheckResolveIpMessage", new Dictionary<string, object>
                    {
                        { "proxyHostName", _configService.ProxyHostname }
                    }),
                    "#proxy-failed-resolve-ip");
            }

            // Gamarr: the proxy connectivity test used to ping the Servarr services; it was removed so Gamarr never contacts them.
            return new HealthCheck(GetType());
        }
    }
}
