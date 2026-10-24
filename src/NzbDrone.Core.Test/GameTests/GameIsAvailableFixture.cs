using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Games;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.GameTests
{
    [TestFixture]
    public class GameIsAvailableFixture : CoreTest
    {
        private Game _game;

        [SetUp]
        public void Setup()
        {
            _game = Builder<Game>.CreateNew()
                                 .With(g => g.Platform = null)
                                 .Build();
        }

        private void SetGameProperties(DateTime? releaseDate, GameStatusType minimumAvailability, GameStatusType status = GameStatusType.Announced)
        {
            _game.GameMetadata.Value.ReleaseDate = releaseDate;
            _game.GameMetadata.Value.Status = status;
            _game.MinimumAvailability = minimumAvailability;
        }

        [TestCase(null, GameStatusType.TBA, true)]
        [TestCase("2000/01/01 21:10:42", GameStatusType.TBA, true)]
        [TestCase("2100/01/01 21:10:42", GameStatusType.TBA, true)]
        [TestCase(null, GameStatusType.Announced, true)]
        [TestCase("2000/01/01 21:10:42", GameStatusType.Announced, true)]
        [TestCase("2100/01/01 21:10:42", GameStatusType.Announced, true)]
        [TestCase(null, GameStatusType.Released, false)]
        [TestCase("2000/01/01 21:10:42", GameStatusType.Released, true)]
        [TestCase("2100/01/01 21:10:42", GameStatusType.Released, false)]
        public void should_have_correct_availability(DateTime? releaseDate, GameStatusType minimumAvailability, bool result)
        {
            SetGameProperties(releaseDate, minimumAvailability);
            _game.IsAvailable().Should().Be(result);
        }

        [Test]
        public void released_without_date_should_be_available_when_igdb_says_released()
        {
            SetGameProperties(null, GameStatusType.Released, GameStatusType.Released);
            _game.IsAvailable().Should().BeTrue();
        }

        [Test]
        public void positive_delay_should_effect_availability()
        {
            SetGameProperties(DateTime.UtcNow.AddDays(-5), GameStatusType.Released);
            _game.IsAvailable().Should().BeTrue();
            _game.IsAvailable(6).Should().BeFalse();
        }

        [Test]
        public void negative_delay_should_effect_availability()
        {
            SetGameProperties(DateTime.UtcNow.AddDays(5), GameStatusType.Released);
            _game.IsAvailable().Should().BeFalse();
            _game.IsAvailable(-6).Should().BeTrue();
        }

        [Test]
        public void should_use_release_date_of_the_games_platform()
        {
            _game.Platform = new Platform { Id = 2, IgdbPlatformId = 19 };
            _game.GameMetadata.Value.PlatformReleaseDates = new List<PlatformReleaseDate>
            {
                new PlatformReleaseDate { IgdbPlatformId = 6, Date = DateTime.UtcNow.AddYears(-2) },
                new PlatformReleaseDate { IgdbPlatformId = 19, Date = DateTime.UtcNow.AddDays(10) }
            };

            SetGameProperties(DateTime.UtcNow.AddYears(-2), GameStatusType.Released);

            _game.GetReleaseDate().Should().BeAfter(DateTime.UtcNow);
            _game.IsAvailable().Should().BeFalse();
        }

        [Test]
        public void should_fall_back_to_first_release_date_when_platform_has_no_date()
        {
            _game.Platform = new Platform { Id = 2, IgdbPlatformId = 19 };
            _game.GameMetadata.Value.PlatformReleaseDates = new List<PlatformReleaseDate>
            {
                new PlatformReleaseDate { IgdbPlatformId = 6, Date = DateTime.UtcNow.AddYears(-2) }
            };

            SetGameProperties(DateTime.UtcNow.AddYears(-2), GameStatusType.Released);

            _game.IsAvailable().Should().BeTrue();
        }
    }
}
