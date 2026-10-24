using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Games;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.PlatformTests
{
    [TestFixture]
    public class DefaultPlatformSelectorFixture : CoreTest
    {
        private static readonly Platform Wii = new Platform { Id = 1, IgdbPlatformId = 5, Family = PlatformFamily.Nintendo };
        private static readonly Platform N64 = new Platform { Id = 2, IgdbPlatformId = 4, Family = PlatformFamily.Nintendo };
        private static readonly Platform WiiU = new Platform { Id = 3, IgdbPlatformId = 41, Family = PlatformFamily.Nintendo };
        private static readonly Platform XboxSeries = new Platform { Id = 4, IgdbPlatformId = 169, Family = PlatformFamily.Microsoft };
        private static readonly Platform Ps4 = new Platform { Id = 5, IgdbPlatformId = 48, Family = PlatformFamily.Sony };
        private static readonly Platform Pc = new Platform { Id = 6, IgdbPlatformId = 6, Family = PlatformFamily.PC };

        private static GameMetadata GivenReleases(params (Platform Platform, int Year)[] releases)
        {
            return new GameMetadata
            {
                AvailablePlatformIgdbIds = releases.Select(r => r.Platform.IgdbPlatformId).ToList(),
                PlatformReleaseDates = releases
                    .Where(r => r.Year > 0)
                    .Select(r => new PlatformReleaseDate { IgdbPlatformId = r.Platform.IgdbPlatformId, Date = new DateTime(r.Year, 5, 19, 0, 0, 0, DateTimeKind.Utc) })
                    .ToList()
            };
        }

        [Test]
        public void should_pick_the_original_release_platform()
        {
            var metadata = GivenReleases((Wii, 2007), (N64, 1998), (WiiU, 2015));

            DefaultPlatformSelector.Select(new List<Platform> { Wii, N64, WiiU }, metadata).Should().Be(N64);
        }

        [Test]
        public void should_prefer_pc_when_released_on_the_same_day()
        {
            var metadata = GivenReleases((XboxSeries, 2015), (Ps4, 2015), (Pc, 2015));

            DefaultPlatformSelector.Select(new List<Platform> { XboxSeries, Ps4, Pc }, metadata).Should().Be(Pc);
        }

        [Test]
        public void should_use_the_requested_platform_when_available()
        {
            var metadata = GivenReleases((Wii, 2007), (N64, 1998));

            DefaultPlatformSelector.Select(new List<Platform> { Wii, N64 }, metadata, Wii.Id).Should().Be(Wii);
        }

        [Test]
        public void should_ignore_a_requested_platform_that_is_not_available()
        {
            var metadata = GivenReleases((Wii, 2007), (N64, 1998));

            DefaultPlatformSelector.Select(new List<Platform> { Wii, N64 }, metadata, Pc.Id).Should().Be(N64);
        }

        [Test]
        public void should_fall_back_to_igdb_order_without_release_dates()
        {
            var metadata = GivenReleases((WiiU, 0), (Wii, 0));

            DefaultPlatformSelector.Select(new List<Platform> { Wii, WiiU }, metadata).Should().Be(WiiU);
        }

        [Test]
        public void should_return_null_without_platforms()
        {
            DefaultPlatformSelector.Select(new List<Platform>(), new GameMetadata()).Should().BeNull();
        }
    }
}
