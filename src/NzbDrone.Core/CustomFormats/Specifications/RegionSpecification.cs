using System;
using System.Linq;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class RegionSpecificationValidator : AbstractValidator<RegionSpecification>
    {
        public RegionSpecificationValidator()
        {
            RuleFor(c => c.Value).Custom((value, context) =>
            {
                if (!Enum.IsDefined(typeof(Region), value) || value == (int)Region.Unknown)
                {
                    context.AddFailure($"Invalid region condition value: {value}");
                }
            });
        }
    }

    // Matches releases for a region ("(USA)", "(Europe)", "[JP]", "PAL"...). World releases match every region.
    public class RegionSpecification : CustomFormatSpecificationBase
    {
        private static readonly RegionSpecificationValidator Validator = new();

        public override int Order => 4;
        public override string ImplementationName => "Region";

        [FieldDefinition(1, Label = "CustomFormatsSpecificationRegion", Type = FieldType.Select, SelectOptions = typeof(RegionFieldConverter))]
        public int Value { get; set; }

        [FieldDefinition(2, Label = "CustomFormatsSpecificationRegionIncludeWorld", HelpText = "CustomFormatsSpecificationRegionIncludeWorldHelpText", Type = FieldType.Checkbox)]
        public bool IncludeWorld { get; set; } = true;

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            var regions = input.GameInfo?.Regions;

            if (regions == null || !regions.Any())
            {
                return false;
            }

            return regions.Contains((Region)Value) || (IncludeWorld && regions.Contains(Region.World));
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
