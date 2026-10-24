using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.IGDB.Keyword
{
    public class IGDBKeywordSettingsValidator : IGDBSettingsBaseValidator<IGDBKeywordSettings>
    {
        public IGDBKeywordSettingsValidator()
        {
            RuleFor(c => c.KeywordId).Matches(@"^[1-9][0-9]*$", RegexOptions.IgnoreCase);
        }
    }

    public class IGDBKeywordSettings : IGDBSettingsBase<IGDBKeywordSettings>
    {
        private static readonly IGDBKeywordSettingsValidator Validator = new();

        public IGDBKeywordSettings()
        {
            KeywordId = "";
        }

        [FieldDefinition(1, Label = "Keyword Id", Type = FieldType.Textbox, HelpText = "IGDB Id of keyword to Follow")]
        public string KeywordId { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
