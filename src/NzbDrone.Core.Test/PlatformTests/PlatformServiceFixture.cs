using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.PlatformTests
{
    [TestFixture]
    public class PlatformServiceFixture : DbTest<PlatformService, Platform>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IPlatformRepository>(Mocker.Resolve<PlatformRepository>());
            Mocker.SetConstant<NzbDrone.Common.Cache.ICacheManager>(Mocker.Resolve<NzbDrone.Common.Cache.CacheManager>());
        }

        [Test]
        public void migration_should_seed_all_platforms()
        {
            var platforms = Subject.All();

            platforms.Should().HaveCount(PlatformDefinitions.All.Count);
            platforms.Single(p => p.IgdbPlatformId == 6).Id.Should().Be(1);
            platforms.Single(p => p.IgdbPlatformId == 19).FileExtensions.Should().Contain(".sfc");
            platforms.Single(p => p.IgdbPlatformId == 48).NewznabCategories.Should().BeEquivalentTo(new[] { 1000, 1180 });
        }

        [Test]
        public void should_insert_missing_platforms_on_startup()
        {
            var snes = Db.All<Platform>().Single(p => p.IgdbPlatformId == 19);
            Db.Delete(snes);

            Subject.Handle(new ApplicationStartedEvent());

            Subject.All().Should().ContainSingle(p => p.IgdbPlatformId == 19);
            Subject.All().Should().HaveCount(PlatformDefinitions.All.Count);
        }

        [Test]
        public void should_only_update_enabled()
        {
            var snes = Subject.FindByIgdbPlatformId(19);

            Subject.Update(new Platform { Id = snes.Id, Enabled = false, Name = "Changed" });

            var updated = Subject.Get(snes.Id);
            updated.Enabled.Should().BeFalse();
            updated.Name.Should().Be("Super Nintendo Entertainment System");
            Subject.Enabled().Should().NotContain(p => p.Id == snes.Id);
        }

        [Test]
        public void should_find_platforms_by_igdb_ids_in_order()
        {
            var platforms = Subject.FindByIgdbPlatformIds(new List<int> { 130, 999999, 6, 130 });

            platforms.Select(p => p.IgdbPlatformId).Should().Equal(130, 6);
        }

        [Test]
        public void should_find_by_folder_name_case_insensitive()
        {
            Subject.FindByFolderName("SNES").Should().ContainSingle(p => p.IgdbPlatformId == 19);
        }

        [Test]
        public void migration_should_set_igdb_slugs()
        {
            var platforms = Subject.All();

            platforms.Should().OnlyContain(p => p.Slug == PlatformDefinitions.Get(p.IgdbPlatformId).Slug);
            platforms.Single(p => p.IgdbPlatformId == 19).Slug.Should().Be("snes");
        }

        [Test]
        public void should_not_insert_platform_again_after_its_igdb_id_was_corrected()
        {
            var snes = Db.All<Platform>().Single(p => p.IgdbPlatformId == 19);
            snes.IgdbPlatformId = 99919;
            Db.Update(snes);

            Subject.Handle(new ApplicationStartedEvent());

            Subject.All().Should().HaveCount(PlatformDefinitions.All.Count);
            Subject.All().Should().NotContain(p => p.IgdbPlatformId == 19);
        }

        [Test]
        public void should_find_platform_with_corrected_igdb_id_by_definition_id()
        {
            var snes = Db.All<Platform>().Single(p => p.IgdbPlatformId == 19);
            snes.IgdbPlatformId = 99919;
            Db.Update(snes);

            Subject.FindByIgdbPlatformId(19).Id.Should().Be(snes.Id);
            Subject.FindByIgdbPlatformId(99919).Id.Should().Be(snes.Id);
        }

        [Test]
        public void should_fill_in_missing_slugs_on_startup()
        {
            var snes = Db.All<Platform>().Single(p => p.IgdbPlatformId == 19);
            snes.Slug = null;
            Db.Update(snes);

            Subject.Handle(new ApplicationStartedEvent());

            Subject.Get(snes.Id).Slug.Should().Be("snes");
            Subject.All().Should().HaveCount(PlatformDefinitions.All.Count);
        }
    }
}
