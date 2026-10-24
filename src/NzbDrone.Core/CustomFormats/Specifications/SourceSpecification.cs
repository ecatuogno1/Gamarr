using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class SourceSpecificationValidator : AbstractValidator<SourceSpecification>
    {
        public SourceSpecificationValidator()
        {
            RuleFor(c => c.Value).Custom((value, context) =>
            {
                if (!Enum.IsDefined(typeof(QualitySource), value))
                {
                    context.AddFailure($"Invalid source condition value: {value}");
                }
            });
        }
    }

    public class QualitySourceFieldConverter : ISelectOptionsConverter
    {
        public List<SelectOption> GetSelectOptions()
        {
            return new List<SelectOption>
            {
                new() { Value = (int)QualitySource.UNKNOWN, Name = "Unknown" },
                new() { Value = (int)QualitySource.CARTRIDGE, Name = "Cartridge (ROM)" },
                new() { Value = (int)QualitySource.DISC, Name = "Disc image" },
                new() { Value = (int)QualitySource.DIGITAL, Name = "Console digital" },
                new() { Value = (int)QualitySource.PC, Name = "PC" }
            };
        }
    }

    public class SourceSpecification : CustomFormatSpecificationBase
    {
        private static readonly SourceSpecificationValidator Validator = new();

        public override int Order => 5;
        public override string ImplementationName => "Source";

        [FieldDefinition(1, Label = "CustomFormatsSpecificationSource", Type = FieldType.Select, SelectOptions = typeof(QualitySourceFieldConverter))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            // Cartridge (ROM), disc image, console digital package or PC release
            return (input.GameInfo?.Quality?.Quality?.Source ?? QualitySource.UNKNOWN) == (QualitySource)Value;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
