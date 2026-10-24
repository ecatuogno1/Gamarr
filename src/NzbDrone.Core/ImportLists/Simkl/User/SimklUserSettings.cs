using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Simkl.User
{
    public class SimklUserSettingsValidator : SimklSettingsBaseValidator<SimklUserSettings>
    {
        public SimklUserSettingsValidator()
        {
            RuleFor(c => c.ListType).NotNull();
        }
    }

    public class SimklUserSettings : SimklSettingsBase<SimklUserSettings>
    {
        private static readonly SimklUserSettingsValidator Validator = new();

        public SimklUserSettings()
        {
            GameType = (int)SimklUserGameType.Games;
            ListType = (int)SimklUserListType.Watching;
        }

        [FieldDefinition(1, Label = "ImportListsSimklSettingsGameType", Type = FieldType.Select, SelectOptions = typeof(SimklUserGameType), HelpText = "ImportListsSimklSettingsGameTypeHelpText")]
        public int GameType { get; set; }

        [FieldDefinition(2, Label = "ImportListsSimklSettingsListType", Type = FieldType.Select, SelectOptions = typeof(SimklUserListType), HelpText = "ImportListsSimklSettingsListTypeHelpText")]
        public int ListType { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
