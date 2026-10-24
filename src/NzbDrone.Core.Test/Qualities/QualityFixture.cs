using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityFixture : CoreTest
    {
        public static object[] FromIntCases =
                {
                        new object[] { 0, Quality.Unknown },
                        new object[] { 100, Quality.RomUnverified },
                        new object[] { 101, Quality.RomVerified },
                        new object[] { 110, Quality.DiscCompressedLossy },
                        new object[] { 111, Quality.DiscIso },
                        new object[] { 112, Quality.DiscChdRvz },
                        new object[] { 113, Quality.DiscRedump },
                        new object[] { 120, Quality.NspXci },
                        new object[] { 121, Quality.Pkg },
                        new object[] { 122, Quality.WuaWux },
                        new object[] { 123, Quality.Cia3ds },
                        new object[] { 124, Quality.XexGod },
                        new object[] { 130, Quality.PcPortable },
                        new object[] { 131, Quality.PcSteamRip },
                        new object[] { 132, Quality.PcRepack },
                        new object[] { 133, Quality.PcScene },
                        new object[] { 134, Quality.PcGog },
                };

        public static object[] ToIntCases =
                {
                        new object[] { Quality.Unknown, 0 },
                        new object[] { Quality.RomUnverified, 100 },
                        new object[] { Quality.RomVerified, 101 },
                        new object[] { Quality.DiscCompressedLossy, 110 },
                        new object[] { Quality.DiscIso, 111 },
                        new object[] { Quality.DiscChdRvz, 112 },
                        new object[] { Quality.DiscRedump, 113 },
                        new object[] { Quality.NspXci, 120 },
                        new object[] { Quality.Pkg, 121 },
                        new object[] { Quality.WuaWux, 122 },
                        new object[] { Quality.Cia3ds, 123 },
                        new object[] { Quality.XexGod, 124 },
                        new object[] { Quality.PcPortable, 130 },
                        new object[] { Quality.PcSteamRip, 131 },
                        new object[] { Quality.PcRepack, 132 },
                        new object[] { Quality.PcScene, 133 },
                        new object[] { Quality.PcGog, 134 },
                };

        [TestCase(1)]
        [TestCase(7)]
        [TestCase(31)]
        [TestCase(999)]
        public void should_throw_for_old_video_quality_ids(int id)
        {
            Assert.Throws<System.ArgumentException>(() => Quality.FindById(id));
        }

        [Test]
        [TestCaseSource("FromIntCases")]
        public void should_be_able_to_convert_int_to_qualityTypes(int source, Quality expected)
        {
            var quality = (Quality)source;
            quality.Should().Be(expected);
        }

        [Test]
        [TestCaseSource("ToIntCases")]
        public void should_be_able_to_convert_qualityTypes_to_int(Quality source, int expected)
        {
            var i = (int)source;
            i.Should().Be(expected);
        }

        public static List<QualityProfileQualityItem> GetDefaultQualities(params Quality[] allowed)
        {
            var qualities = Quality.DefaultQualityDefinitions
                .OrderBy(d => d.Weight)
                .Select(d => d.Quality)
                .ToList();

            if (allowed.Length == 0)
            {
                allowed = qualities.ToArray();
            }

            var items = qualities
                .Except(allowed)
                .Concat(allowed)
                .Select(v => new QualityProfileQualityItem
                {
                    Quality = v,
                    Allowed = allowed.Contains(v)
                }).ToList();

            return items;
        }
    }
}
