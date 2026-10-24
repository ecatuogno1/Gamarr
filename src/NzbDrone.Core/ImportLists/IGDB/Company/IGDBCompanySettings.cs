using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.IGDB.Company
{
    public class IGDBCompanySettingsValidator : IGDBSettingsBaseValidator<IGDBCompanySettings>
    {
        public IGDBCompanySettingsValidator()
        {
            RuleFor(c => c.CompanyId).Matches(@"^[1-9][0-9]*$", RegexOptions.IgnoreCase);
        }
    }

    public class IGDBCompanySettings : IGDBSettingsBase<IGDBCompanySettings>
    {
        private static readonly IGDBCompanySettingsValidator Validator = new();

        public IGDBCompanySettings()
        {
            CompanyId = "";
        }

        [FieldDefinition(1, Label = "Company Id", Type = FieldType.Textbox, HelpText = "IGDB Id of Company to Follow")]
        public string CompanyId { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
