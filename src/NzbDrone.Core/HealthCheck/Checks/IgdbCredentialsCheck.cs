using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MetadataSource.Igdb;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ConfigSavedEvent))]
    public class IgdbCredentialsCheck : HealthCheckBase
    {
        private readonly IIgdbClient _igdbClient;
        private readonly Logger _logger;

        public IgdbCredentialsCheck(IIgdbClient igdbClient, ILocalizationService localizationService, Logger logger)
            : base(localizationService)
        {
            _igdbClient = igdbClient;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            if (!_igdbClient.HasCredentials)
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.IgdbCredentialsMissing,
                    _localizationService.GetLocalizedString("IgdbCredentialsCheckMissingMessage"),
                    "#igdb-credentials-missing");
            }

            try
            {
                _igdbClient.ValidateCredentials();
            }
            catch (IgdbAuthenticationException ex)
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.IgdbCredentialsInvalid,
                    _localizationService.GetLocalizedString("IgdbCredentialsCheckInvalidMessage", new Dictionary<string, object>
                    {
                        { "errorMessage", ex.Message }
                    }),
                    "#igdb-credentials-invalid");
            }
            catch (Exception ex)
            {
                // Network problems aren't a credentials problem, don't flag them here
                _logger.Debug(ex, "Unable to validate IGDB credentials");
            }

            return new HealthCheck(GetType());
        }
    }
}
