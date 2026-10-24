using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Gamarr.Http;
using Gamarr.Http.REST;
using Gamarr.Http.REST.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Validation;

namespace Gamarr.Api.V3.CustomFormats
{
    [V3ApiController]
    public class CustomFormatController : RestController<CustomFormatResource>
    {
        private readonly ICustomFormatService _formatService;
        private readonly List<ICustomFormatSpecification> _specifications;

        public CustomFormatController(ICustomFormatService formatService,
                                  List<ICustomFormatSpecification> specifications)
        {
            _formatService = formatService;
            _specifications = specifications;

            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Name)
                .Must((v, c) => !_formatService.All().Any(f => f.Name == c && f.Id != v.Id)).WithMessage("Must be unique.");
            SharedValidator.RuleFor(c => c).Custom((customFormat, context) =>
            {
                if (!customFormat.Specifications.Any())
                {
                    context.AddFailure("Must contain at least one Condition");
                }

                if (customFormat.Specifications.Any(s => s.Name.IsNullOrWhiteSpace()))
                {
                    context.AddFailure("Condition name(s) cannot be empty or consist of only spaces");
                }
            });
        }

        protected override CustomFormatResource GetResourceById(int id)
        {
            return _formatService.GetById(id).ToResource(true);
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<List<CustomFormatResource>> GetAll()
        {
            return TypedResults.Ok(_formatService.All().ToResource(true));
        }

        [RestPostById]
        [Consumes("application/json")]
        public Results<Created<CustomFormatResource>, NotFound> Create([FromBody] CustomFormatResource customFormatResource)
        {
            var model = customFormatResource.ToModel(_specifications);

            Validate(model);

            return Created(_formatService.Insert(model).Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        public Results<Accepted<CustomFormatResource>, NotFound> Update([FromBody] CustomFormatResource resource)
        {
            var model = resource.ToModel(_specifications);

            Validate(model);

            _formatService.Update(model);

            return Accepted(model.Id);
        }

        [HttpPut("bulk")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public virtual Results<Ok<List<CustomFormatResource>>, BadRequest> Update([FromBody] CustomFormatBulkResource resource)
        {
            if (!resource.Ids.Any())
            {
                throw new BadRequestException("ids must be provided");
            }

            var customFormats = resource.Ids.Select(id => _formatService.GetById(id)).ToList();

            customFormats.ForEach(existing =>
            {
                existing.IncludeCustomFormatWhenRenaming = resource.IncludeCustomFormatWhenRenaming ?? existing.IncludeCustomFormatWhenRenaming;
            });

            _formatService.Update(customFormats);

            return TypedResults.Ok(customFormats.ConvertAll(cf => cf.ToResource(true)));
        }

        [RestDeleteById]
        public Ok DeleteFormat(int id)
        {
            _formatService.Delete(id);

            return TypedResults.Ok();
        }

        [HttpDelete("bulk")]
        [Consumes("application/json")]
        public virtual Ok<object> DeleteFormats([FromBody] CustomFormatBulkResource resource)
        {
            _formatService.Delete(resource.Ids.ToList());

            return TypedResults.Ok<object>(new { });
        }

        [HttpGet("schema")]
        [Produces("application/json")]
        public Ok<List<CustomFormatSpecificationSchema>> GetTemplates()
        {
            var schema = _specifications.OrderBy(x => x.Order).Select(x => x.ToSchema()).ToList();

            var presets = GetPresets().ToList();

            foreach (var item in schema)
            {
                item.Presets = presets.Where(x => x.GetType().Name == item.Implementation).Select(x => x.ToSchema()).ToList();
            }

            return TypedResults.Ok(schema);
        }

        private void Validate(CustomFormat definition)
        {
            foreach (var spec in definition.Specifications)
            {
                var validationResult = spec.Validate();
                VerifyValidationResult(validationResult);
            }
        }

        protected void VerifyValidationResult(ValidationResult validationResult)
        {
            var result = new NzbDroneValidationResult(validationResult.Errors);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        private IEnumerable<ICustomFormatSpecification> GetPresets()
        {
            yield return new ReleaseTitleSpecification
            {
                Name = "No-Intro / Redump",
                Value = @"\b(No-?Intro|Redump)\b"
            };

            yield return new ReleaseTitleSpecification
            {
                Name = "GoodTools Verified",
                Value = @"\[!\]"
            };

            yield return new ReleaseTitleSpecification
            {
                Name = "Bad Dump",
                Value = @"\[(b|o)\d*\]"
            };

            yield return new ReleaseTitleSpecification
            {
                Name = "Repack Groups",
                Value = @"\b(FitGirl|DODI|ElAmigos|KaOs)\b"
            };

            yield return new RegionSpecification
            {
                Name = "USA",
                Value = (int)NzbDrone.Core.Parser.Model.Region.USA
            };

            yield return new RegionSpecification
            {
                Name = "Europe",
                Value = (int)NzbDrone.Core.Parser.Model.Region.Europe
            };

            yield return new ReleaseTypeSpecification
            {
                Name = "Update",
                Value = (int)NzbDrone.Core.Parser.Model.ReleaseType.Update
            };

            yield return new ReleaseTypeSpecification
            {
                Name = "DLC",
                Value = (int)NzbDrone.Core.Parser.Model.ReleaseType.Dlc
            };

            var formats = _formatService.All();
            foreach (var format in formats)
            {
                foreach (var condition in format.Specifications)
                {
                    var preset = condition.Clone();
                    preset.Name = $"{format.Name}: {preset.Name}";
                    yield return preset;
                }
            }
        }
    }
}
