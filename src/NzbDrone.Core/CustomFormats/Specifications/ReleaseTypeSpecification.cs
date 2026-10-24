using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class ReleaseTypeSpecificationValidator : AbstractValidator<ReleaseTypeSpecification>
    {
        public ReleaseTypeSpecificationValidator()
        {
            RuleFor(c => c.Value).Custom((value, context) =>
            {
                if (!Enum.IsDefined(typeof(ReleaseType), value))
                {
                    context.AddFailure($"Invalid release type condition value: {value}");
                }
            });
        }
    }

    // Matches what a release contains: base game, update, DLC, bundle, hack, translation, homebrew, demo, prototype.
    // A positive score for a custom format with this condition also lets the release type through the
    // release type check (updates, DLC... are rejected by default).
    public class ReleaseTypeSpecification : CustomFormatSpecificationBase
    {
        private static readonly ReleaseTypeSpecificationValidator Validator = new();

        public override int Order => 5;
        public override string ImplementationName => "Release Type";

        [FieldDefinition(1, Label = "CustomFormatsSpecificationReleaseType", Type = FieldType.Select, SelectOptions = typeof(ReleaseTypeFieldConverter))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            var releaseType = input.GameInfo?.ReleaseType ?? ReleaseType.Base;

            return releaseType == (ReleaseType)Value;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
