using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.IGDB.Popular
{
    public class IGDBPopularSettingsValidator : IGDBSettingsBaseValidator<IGDBPopularSettings>
    {
        public IGDBPopularSettingsValidator()
        {
            RuleFor(c => c.IGDBListType).NotNull();

            RuleFor(c => c.FilterCriteria).SetValidator(_ => new IGDBFilterSettingsValidator());
        }
    }

    public class IGDBPopularSettings : IGDBSettingsBase<IGDBPopularSettings>
    {
        private static readonly IGDBPopularSettingsValidator Validator = new();

        public IGDBPopularSettings()
        {
            IGDBListType = (int)IGDBPopularListType.Popular;
        }

        [FieldDefinition(1, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(IGDBPopularListType), HelpText = "Type of list your seeking to import from")]
        public int IGDBListType { get; set; }

        [FieldDefinition(2)]
        public IGDBFilterSettings FilterCriteria { get; set; } = new();

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
