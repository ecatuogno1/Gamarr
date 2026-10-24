using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource.Igdb;
using NzbDrone.Core.MetadataSource.Igdb.Resource;

namespace NzbDrone.Core.Platforms
{
    // Keeps the platforms' IGDB ids right: IGDB's /platforms is matched by slug and a stored IgdbPlatformId that differs
    // is corrected, a slug IGDB renamed is updated through the id. Runs on startup and when the IGDB credentials are
    // saved (if there are credentials), and weekly.
    public class PlatformIgdbSyncService : IExecute<SyncPlatformsWithIgdbCommand>, IHandle<ApplicationStartedEvent>, IHandle<ConfigSavedEvent>
    {
        public const string Query = "fields id,slug,name; limit 500;";

        private readonly IIgdbClient _igdbClient;
        private readonly IPlatformRepository _platformRepository;
        private readonly IPlatformService _platformService;
        private readonly IConfigService _configService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly Logger _logger;

        private string _syncedCredentials;

        public PlatformIgdbSyncService(IIgdbClient igdbClient,
                                       IPlatformRepository platformRepository,
                                       IPlatformService platformService,
                                       IConfigService configService,
                                       IManageCommandQueue commandQueueManager,
                                       Logger logger)
        {
            _igdbClient = igdbClient;
            _platformRepository = platformRepository;
            _platformService = platformService;
            _configService = configService;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        public void Sync()
        {
            if (!_igdbClient.HasCredentials)
            {
                _logger.Debug("No IGDB credentials, not checking platform ids");
                return;
            }

            var remote = _igdbClient.Query<IgdbReference>("platforms", Query);

            _syncedCredentials = GetCredentials();

            if (remote.Count == 0)
            {
                _logger.Warn("IGDB returned no platforms, not checking platform ids");
                return;
            }

            var bySlug = remote.Where(r => r.Slug.IsNotNullOrWhiteSpace())
                .GroupBy(r => r.Slug, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            var byId = remote.GroupBy(r => r.Id).ToDictionary(g => g.Key, g => g.First());

            var platforms = _platformRepository.All().ToList();
            var changed = false;

            foreach (var platform in platforms)
            {
                if (platform.Slug.IsNotNullOrWhiteSpace() && bySlug.TryGetValue(platform.Slug, out var match))
                {
                    if (match.Id == platform.IgdbPlatformId)
                    {
                        continue;
                    }

                    var other = platforms.FirstOrDefault(p => p.Id != platform.Id && p.IgdbPlatformId == match.Id);

                    if (other != null)
                    {
                        _logger.Warn("IGDB platform '{0}' has id {1}, but {2} already uses it. Not correcting {3}", match.Slug, match.Id, other.Name, platform.Name);
                        continue;
                    }

                    _logger.Info("Correcting the IGDB id of {0} ({1}) from {2} to {3}", platform.Name, platform.Slug, platform.IgdbPlatformId, match.Id);

                    platform.IgdbPlatformId = match.Id;
                    _platformRepository.SetFields(platform, p => p.IgdbPlatformId);
                    changed = true;
                }
                else if (byId.TryGetValue(platform.IgdbPlatformId, out var byIdMatch) && byIdMatch.Slug.IsNotNullOrWhiteSpace())
                {
                    _logger.Info("IGDB slug of {0} (id {1}) changed from '{2}' to '{3}'", platform.Name, platform.IgdbPlatformId, platform.Slug, byIdMatch.Slug);

                    platform.Slug = byIdMatch.Slug;
                    _platformRepository.SetFields(platform, p => p.Slug);
                    changed = true;
                }
                else
                {
                    _logger.Warn("{0} (IGDB id {1}, slug '{2}') wasn't found on IGDB", platform.Name, platform.IgdbPlatformId, platform.Slug);
                }
            }

            if (changed)
            {
                _platformService.ClearCache();
            }
            else
            {
                _logger.Debug("IGDB ids of all {0} platforms are up to date", platforms.Count);
            }
        }

        public void Execute(SyncPlatformsWithIgdbCommand message)
        {
            try
            {
                Sync();
            }
            catch (IgdbException ex)
            {
                // Missing or rejected credentials have their own health check
                _logger.Warn("Unable to check platform ids with IGDB: {0}", ex.Message);
            }
        }

        public void Handle(ApplicationStartedEvent message)
        {
            if (_igdbClient.HasCredentials)
            {
                _commandQueueManager.Push(new SyncPlatformsWithIgdbCommand());
            }
        }

        public void Handle(ConfigSavedEvent message)
        {
            if (_igdbClient.HasCredentials && GetCredentials() != _syncedCredentials)
            {
                _syncedCredentials = GetCredentials();
                _commandQueueManager.Push(new SyncPlatformsWithIgdbCommand());
            }
        }

        private string GetCredentials()
        {
            return $"{_configService.IgdbClientId}:{_configService.IgdbClientSecret}";
        }
    }
}
