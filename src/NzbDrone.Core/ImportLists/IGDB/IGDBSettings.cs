using FluentValidation;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.IGDB
{
    public class IGDBSettingsBaseValidator<TSettings> : AbstractValidator<TSettings>
        where TSettings : IGDBSettingsBase<TSettings>
    {
    }

    public class IGDBSettingsBase<TSettings> : ImportListSettingsBase<TSettings>
        where TSettings : IGDBSettingsBase<TSettings>
    {
        private static readonly IGDBSettingsBaseValidator<TSettings> Validator = new();

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
