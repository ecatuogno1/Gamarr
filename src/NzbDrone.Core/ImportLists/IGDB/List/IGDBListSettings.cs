using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.IGDB.List
{
    public class IGDBListSettingsValidator : IGDBSettingsBaseValidator<IGDBListSettings>
    {
        public IGDBListSettingsValidator()
        {
            RuleFor(c => c.ListId).Matches("^[1-9][0-9]*$").NotEmpty();
        }
    }

    public class IGDBListSettings : IGDBSettingsBase<IGDBListSettings>
    {
        private static readonly IGDBListSettingsValidator Validator = new();

        public IGDBListSettings()
        {
            ListId = "";
        }

        [FieldDefinition(1, Label = "ListId", Type = FieldType.Textbox, HelpText = "IGDB Id of List to Follow")]
        public string ListId { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
