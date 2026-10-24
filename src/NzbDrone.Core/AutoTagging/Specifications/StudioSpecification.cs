using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Games;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class StudioSpecificationValidator : AbstractValidator<StudioSpecification>
    {
        public StudioSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
        }
    }

    public class StudioSpecification : AutoTaggingSpecificationBase
    {
        private static readonly StudioSpecificationValidator Validator = new();

        public override int Order => 1;
        public override string ImplementationName => "Company";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationStudio", Type = FieldType.Tag)]
        public IEnumerable<string> Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Game game)
        {
            // Matches the game's developers and publishers
            var companies = (game.GameMetadata?.Value?.Developers ?? new List<string>())
                .Concat(game.GameMetadata?.Value?.Publishers ?? new List<string>())
                .ToList();

            return Value.Any(studio => companies.Any(c => c.EqualsIgnoreCase(studio)));
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
