using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.NewznabTests
{
    [TestFixture]
    public class NewznabStandardCategoryFixture : CoreTest
    {
        [TestCase(new[] { 1000, 1080, 4050 }, new[] { 1000, 1080 }, new[] { 1000, 1080 })]
        [TestCase(new[] { 1000, 1080, 4050 }, new[] { 4050 }, new[] { 4050 })]
        [TestCase(new[] { 1000, 4000 }, new[] { 4050 }, new[] { 4000 })]
        [TestCase(new[] { 1080, 1180 }, new[] { 1000 }, new[] { 1080, 1180 })]
        [TestCase(new[] { 1030, 4050 }, new[] { 1000, 1030, 1060 }, new[] { 1030 })]
        [TestCase(new[] { 1000, 4050 }, new int[0], new[] { 1000, 4050 })]
        public void should_intersect_configured_and_platform_categories(int[] configured, int[] platform, int[] expected)
        {
            NewznabStandardCategory.GetSearchCategories(configured, platform).Should().Equal(expected);
        }

        [Test]
        public void should_use_configured_categories_if_platform_has_none()
        {
            NewznabStandardCategory.GetSearchCategories(new[] { 1000, 4050 }, null).Should().Equal(1000, 4050);
        }

        [Test]
        public void default_category_tree_should_contain_default_categories()
        {
            var tree = NewznabStandardCategory.GetDefaultCategoryTree();
            var ids = tree.Select(c => c.Id).Concat(tree.SelectMany(c => c.Subcategories).Select(c => c.Id)).ToList();

            ids.Should().Contain(NewznabStandardCategory.DefaultGameCategories);
        }

        [Test]
        public void fallback_field_options_should_be_game_categories()
        {
            var options = NewznabCategoryFieldOptionsConverter.GetFieldSelectOptions(null);

            options.Should().Contain(o => o.Value == 1000 && o.Name == "Console");
            options.Should().Contain(o => o.Value == 4050 && o.Name == "Games" && o.ParentValue == 4000);
            options.Should().NotContain(o => o.Value >= 2000 && o.Value < 3000);
        }

        [Test]
        public void field_options_should_hide_non_game_categories()
        {
            var categories = new System.Collections.Generic.List<NewznabCategory>
            {
                new() { Id = 1000, Name = "Console" },
                new() { Id = 2000, Name = "Movies" },
                new() { Id = 4000, Name = "PC" },
                new() { Id = 5000, Name = "TV" },
                new() { Id = 8000, Name = "Other" }
            };

            var options = NewznabCategoryFieldOptionsConverter.GetFieldSelectOptions(categories);

            options.Select(o => o.Value).Should().Equal(1000, 4000, 8000);
        }
    }
}
