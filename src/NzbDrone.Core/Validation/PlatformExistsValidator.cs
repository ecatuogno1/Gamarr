using FluentValidation.Validators;
using NzbDrone.Core.Platforms;

namespace NzbDrone.Core.Validation
{
    public class PlatformExistsValidator : PropertyValidator
    {
        private readonly IPlatformService _platformService;

        public PlatformExistsValidator(IPlatformService platformService)
        {
            _platformService = platformService;
        }

        protected override string GetDefaultMessageTemplate() => "Platform does not exist";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context?.PropertyValue == null || (int)context.PropertyValue == 0)
            {
                return true;
            }

            return _platformService.Find((int)context.PropertyValue) != null;
        }
    }
}
