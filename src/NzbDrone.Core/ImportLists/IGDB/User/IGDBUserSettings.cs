using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.IGDB.User
{
    public class IGDBUserSettingsValidator : IGDBSettingsBaseValidator<IGDBUserSettings>
    {
        public IGDBUserSettingsValidator()
        {
            RuleFor(c => c.ListType).NotEmpty();
            RuleFor(c => c.AccessToken).NotEmpty();
            RuleFor(c => c.AccountId).NotEmpty();
        }
    }

    public class IGDBUserSettings : IGDBSettingsBase<IGDBUserSettings>
    {
        private static readonly IGDBUserSettingsValidator Validator = new();

        public IGDBUserSettings()
        {
            ListType = (int)IGDBUserListType.Watchlist;
        }

        public string OAuthUrl => "https://www.thegamedb.org/auth/access";

        [FieldDefinition(0, Label = "Account Id", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccountId { get; set; }

        [FieldDefinition(0, Label = "Access Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(1, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(IGDBUserListType), HelpText = "Type of list your seeking to import from")]
        public int ListType { get; set; }

        [FieldDefinition(99, Label = "Authenticate with IGDB", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
