using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Platforms;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class PlatformSpecificationValidator : AbstractValidator<PlatformSpecification>
    {
        public PlatformSpecificationValidator()
        {
            RuleFor(c => c.Value).Custom((value, context) =>
            {
                if (PlatformDefinitions.All.All(p => p.IgdbPlatformId != value))
                {
                    context.AddFailure($"Invalid platform condition value: {value}");
                }
            });
        }
    }

    public class PlatformFieldConverter : ISelectOptionsConverter
    {
        public List<SelectOption> GetSelectOptions()
        {
            return PlatformDefinitions.All
                .OrderBy(p => p.Name)
                .Select(p => new SelectOption { Value = p.IgdbPlatformId, Name = p.Name })
                .ToList();
        }
    }

    // Matches the platform of the release (or of the game when the release doesn't say). The value is the
    // IGDB platform id so custom formats can be shared between installs.
    public class PlatformSpecification : CustomFormatSpecificationBase
    {
        private static readonly PlatformSpecificationValidator Validator = new();

        public override int Order => 6;
        public override string ImplementationName => "Platform";

        [FieldDefinition(1, Label = "Platform", Type = FieldType.Select, SelectOptions = typeof(PlatformFieldConverter))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            var platform = input.GameInfo?.PlatformIgdbId ?? 0;

            if (platform == 0)
            {
                platform = input.Game?.Platform?.IgdbPlatformId ?? 0;
            }

            return platform > 0 && Parser.PlatformParser.AreCompatible(platform, Value);
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
