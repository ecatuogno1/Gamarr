using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class UpgradeSpecificationFixture : CoreTest<UpgradableSpecification>
    {
        private static readonly CustomFormat CustomFormat1 = new("My Format 1", new ReleaseTitleSpecification { Value = "R1080" }) { Id = 1 };
        private static readonly CustomFormat CustomFormat2 = new("My Format 2", new ReleaseTitleSpecification { Value = "R480" }) { Id = 2 };

        public static object[] IsUpgradeTestCases =
        {
            // Quality upgrade trumps custom format
            new object[] { Quality.RomUnverified, 1, new List<CustomFormat>(), Quality.RomUnverified, 2, new List<CustomFormat>(), UpgradeableRejectReason.None },
            new object[] { Quality.RomUnverified, 1, new List<CustomFormat> { CustomFormat1 }, Quality.RomUnverified, 2, new List<CustomFormat> { CustomFormat1 }, UpgradeableRejectReason.None },
            new object[] { Quality.RomUnverified, 1, new List<CustomFormat> { CustomFormat1 }, Quality.RomUnverified, 2, new List<CustomFormat> { CustomFormat2 }, UpgradeableRejectReason.None },
            new object[] { Quality.RomUnverified, 1, new List<CustomFormat> { CustomFormat2 }, Quality.RomUnverified, 2, new List<CustomFormat> { CustomFormat1 }, UpgradeableRejectReason.None },

            // Revision upgrade trumps custom format
            new object[] { Quality.NspXci, 1, new List<CustomFormat>(), Quality.NspXci, 2, new List<CustomFormat>(), UpgradeableRejectReason.None },
            new object[] { Quality.NspXci, 1, new List<CustomFormat> { CustomFormat1 }, Quality.NspXci, 2, new List<CustomFormat> { CustomFormat1 }, UpgradeableRejectReason.None },
            new object[] { Quality.NspXci, 1, new List<CustomFormat> { CustomFormat1 }, Quality.NspXci, 2, new List<CustomFormat> { CustomFormat2 }, UpgradeableRejectReason.None },
            new object[] { Quality.NspXci, 1, new List<CustomFormat> { CustomFormat2 }, Quality.NspXci, 2, new List<CustomFormat> { CustomFormat1 }, UpgradeableRejectReason.None },

            // Custom formats apply if quality same
            new object[] { Quality.RomUnverified, 1, new List<CustomFormat>(), Quality.RomUnverified, 1, new List<CustomFormat>(), UpgradeableRejectReason.CustomFormatScore },
            new object[] { Quality.RomUnverified, 1, new List<CustomFormat> { CustomFormat1 }, Quality.RomUnverified, 1, new List<CustomFormat> { CustomFormat1 }, UpgradeableRejectReason.CustomFormatScore },
            new object[] { Quality.RomUnverified, 1, new List<CustomFormat> { CustomFormat1 }, Quality.RomUnverified, 1, new List<CustomFormat> { CustomFormat2 }, UpgradeableRejectReason.CustomFormatCutoff },
            new object[] { Quality.RomUnverified, 1, new List<CustomFormat> { CustomFormat2 }, Quality.RomUnverified, 1, new List<CustomFormat> { CustomFormat1 }, UpgradeableRejectReason.CustomFormatScore },

            new object[] { Quality.NspXci, 1, new List<CustomFormat>(), Quality.DiscRedump, 2, new List<CustomFormat>(), UpgradeableRejectReason.BetterQuality },
            new object[] { Quality.NspXci, 1, new List<CustomFormat>(), Quality.DiscRedump, 2, new List<CustomFormat>(), UpgradeableRejectReason.BetterQuality },
            new object[] { Quality.NspXci, 1, new List<CustomFormat>(), Quality.NspXci, 1, new List<CustomFormat>(), UpgradeableRejectReason.CustomFormatScore },
            new object[] { Quality.Cia3ds, 1, new List<CustomFormat>(), Quality.Cia3ds, 1, new List<CustomFormat>(), UpgradeableRejectReason.CustomFormatScore }
        };

        [SetUp]
        public void Setup()
        {
            CustomFormatsTestHelpers.GivenCustomFormats(CustomFormat1, CustomFormat2);
        }

        private void GivenAutoDownloadPropers(ProperDownloadTypes type)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.DownloadPropersAndRepacks)
                  .Returns(type);
        }

        [Test]
        [TestCaseSource(nameof(IsUpgradeTestCases))]
        public void IsUpgradeTest(Quality current,
                                  int currentVersion,
                                  List<CustomFormat> currentFormats,
                                  Quality newQuality,
                                  int newVersion,
                                  List<CustomFormat> newFormats,
                                  UpgradeableRejectReason expected)
        {
            GivenAutoDownloadPropers(ProperDownloadTypes.PreferAndUpgrade);

            var profile = new QualityProfile
            {
                UpgradeAllowed = true,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems(CustomFormat1.Name, CustomFormat2.Name)
            };

            Subject.IsUpgradable(
                        profile,
                        new QualityModel(current, new Revision(version: currentVersion)),
                        currentFormats,
                        new QualityModel(newQuality, new Revision(version: newVersion)),
                        newFormats)
                   .Should().Be(expected);
        }

        [Test]
        public void should_return_true_if_proper_and_download_propers_is_do_not_download()
        {
            GivenAutoDownloadPropers(ProperDownloadTypes.DoNotUpgrade);

            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(Quality.RomVerified, new Revision(version: 1)),
                       new List<CustomFormat>(),
                       new QualityModel(Quality.RomVerified, new Revision(version: 2)),
                       new List<CustomFormat>())
                    .Should().Be(UpgradeableRejectReason.None);
        }

        [Test]
        public void should_return_false_if_proper_and_autoDownloadPropers_is_do_not_prefer()
        {
            GivenAutoDownloadPropers(ProperDownloadTypes.DoNotPrefer);

            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(Quality.RomVerified, new Revision(version: 1)),
                       new List<CustomFormat>(),
                       new QualityModel(Quality.RomVerified, new Revision(version: 2)),
                       new List<CustomFormat>())
                   .Should().Be(UpgradeableRejectReason.UpgradesNotAllowed);
        }

        [Test]
        public void should_return_false_if_release_and_existing_file_are_the_same()
        {
            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(Quality.DiscRedump, new Revision(version: 1)),
                       new List<CustomFormat>(),
                       new QualityModel(Quality.DiscRedump, new Revision(version: 1)),
                       new List<CustomFormat>())
                   .Should().Be(UpgradeableRejectReason.UpgradesNotAllowed);
        }

        [Test]
        public void should_return_true_if_release_has_higher_quality_and_cutoff_is_not_already_met()
        {
            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true,
                Cutoff = Quality.WuaWux.Id
            };

            Subject.IsUpgradable(
                    profile,
                    new QualityModel(Quality.DiscRedump, new Revision(version: 1)),
                    new List<CustomFormat>(),
                    new QualityModel(Quality.WuaWux, new Revision(version: 1)),
                    new List<CustomFormat>())
                .Should().Be(UpgradeableRejectReason.None);
        }

        [Test]
        public void should_return_false_if_release_has_higher_quality_and_cutoff_is_already_met()
        {
            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true,
                Cutoff = Quality.DiscRedump.Id
            };

            Subject.IsUpgradable(
                    profile,
                    new QualityModel(Quality.DiscRedump, new Revision(version: 1)),
                    new List<CustomFormat>(),
                    new QualityModel(Quality.WuaWux, new Revision(version: 1)),
                    new List<CustomFormat>())
                .Should().Be(UpgradeableRejectReason.QualityCutoff);
        }

        [Test]
        public void should_return_false_if_minimum_custom_score_is_not_met()
        {
            var customFormatOne = new CustomFormat
            {
                Id = 1,
                Name = "One"
            };

            var customFormatTwo = new CustomFormat
            {
                Id = 2,
                Name = "Two"
            };

            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true,
                MinUpgradeFormatScore = 11,
                CutoffFormatScore = 100,
                FormatItems = new List<ProfileFormatItem>
                {
                    new ProfileFormatItem
                    {
                        Format = customFormatOne,
                        Score = 10
                    },
                    new ProfileFormatItem
                    {
                        Format = customFormatTwo,
                        Score = 20
                    }
                }
            };

            Subject.IsUpgradable(
                    profile,
                    new QualityModel(Quality.RomVerified),
                    new List<CustomFormat> { customFormatOne },
                    new QualityModel(Quality.RomVerified),
                    new List<CustomFormat> { customFormatTwo })
                .Should().Be(UpgradeableRejectReason.MinCustomFormatScore);
        }

        [Test]
        public void should_return_true_if_minimum_custom_score_is_met()
        {
            var customFormatOne = new CustomFormat
            {
                Id = 1,
                Name = "One"
            };

            var customFormatTwo = new CustomFormat
            {
                Id = 2,
                Name = "Two"
            };

            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true,
                MinUpgradeFormatScore = 10,
                CutoffFormatScore = 100,
                FormatItems = new List<ProfileFormatItem>
                {
                    new ProfileFormatItem
                    {
                        Format = customFormatOne,
                        Score = 10
                    },
                    new ProfileFormatItem
                    {
                        Format = customFormatTwo,
                        Score = 20
                    }
                }
            };

            Subject.IsUpgradable(
                    profile,
                    new QualityModel(Quality.RomVerified),
                    new List<CustomFormat> { customFormatOne },
                    new QualityModel(Quality.RomVerified),
                    new List<CustomFormat> { customFormatTwo })
                .Should().Be(UpgradeableRejectReason.None);
        }
    }
}
