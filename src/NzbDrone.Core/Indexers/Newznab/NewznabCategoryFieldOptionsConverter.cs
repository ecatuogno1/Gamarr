using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Indexers.Newznab
{
    public static class NewznabCategoryFieldOptionsConverter
    {
        public static List<FieldSelectOption<int>> GetFieldSelectOptions(List<NewznabCategory> categories)
        {
            // Categories not relevant for Gamarr (Movies, Audio, TV, XXX, Books)
            var ignoreCategories = new[] { 2000, 3000, 5000, 6000, 7000 };

            // And maybe relevant for specific users
            var unimportantCategories = new[] { 0, 8000 };

            var result = new List<FieldSelectOption<int>>();

            // Fetching categories failed, use the default Newznab game categories
            categories ??= NewznabStandardCategory.GetDefaultCategoryTree();

            foreach (var category in categories.Where(cat => !ignoreCategories.Contains(cat.Id)).OrderBy(cat => unimportantCategories.Contains(cat.Id)).ThenBy(cat => cat.Id))
            {
                result.Add(new FieldSelectOption<int>
                {
                    Value = category.Id,
                    Name = category.Name,
                    Hint = $"({category.Id})"
                });

                if (category.Subcategories != null)
                {
                    foreach (var subcat in category.Subcategories.OrderBy(cat => cat.Id))
                    {
                        result.Add(new FieldSelectOption<int>
                        {
                            Value = subcat.Id,
                            Name = subcat.Name,
                            Hint = $"({subcat.Id})",
                            ParentValue = category.Id
                        });
                    }
                }
            }

            return result;
        }
    }
}
