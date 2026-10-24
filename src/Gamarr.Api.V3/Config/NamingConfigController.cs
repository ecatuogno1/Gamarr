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
using NzbDrone.Core.Organizer;

namespace Gamarr.Api.V3.Config
{
    [V3ApiController("config/naming")]
    public class NamingConfigController : RestController<NamingConfigResource>
    {
        private readonly INamingConfigService _namingConfigService;
        private readonly IFilenameSampleService _filenameSampleService;
        private readonly IFilenameValidationService _filenameValidationService;

        public NamingConfigController(INamingConfigService namingConfigService,
                                  IFilenameSampleService filenameSampleService,
                                  IFilenameValidationService filenameValidationService)
        {
            _namingConfigService = namingConfigService;
            _filenameSampleService = filenameSampleService;
            _filenameValidationService = filenameValidationService;

            SharedValidator.RuleFor(c => c.StandardGameFormat).ValidGameFormat();
            SharedValidator.RuleFor(c => c.GameFolderFormat).ValidGameFolderFormat();
        }

        protected override NamingConfigResource GetResourceById(int id)
        {
            return _namingConfigService.GetConfig().ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<NamingConfigResource> GetNamingConfig()
        {
            return TypedResults.Ok(GetResourceById(1));
        }

        [RestPutById]
        [Consumes("application/json")]
        public Results<Accepted<NamingConfigResource>, NotFound> UpdateNamingConfig([FromBody] NamingConfigResource resource)
        {
            var nameSpec = resource.ToModel();
            ValidateFormatResult(nameSpec);

            _namingConfigService.Save(nameSpec);

            return Accepted(resource.Id);
        }

        [HttpGet("examples")]
        [Produces("application/json")]
        public Ok<NamingExampleResource> GetExamples([FromQuery] NamingConfigResource config)
        {
            if (config.Id == 0)
            {
                config = GetResourceById(1);
            }

            var nameSpec = config.ToModel();
            var sampleResource = new NamingExampleResource();

            var gameSampleResult = _filenameSampleService.GetGameSample(nameSpec);

            sampleResource.GameExample = nameSpec.StandardGameFormat.IsNullOrWhiteSpace()
                ? null
                : gameSampleResult.FileName;

            sampleResource.GameFolderExample = nameSpec.GameFolderFormat.IsNullOrWhiteSpace()
                ? null
                : _filenameSampleService.GetGameFolderSample(nameSpec);

            return TypedResults.Ok(sampleResource);
        }

        private void ValidateFormatResult(NamingConfig nameSpec)
        {
            var gameSampleResult = _filenameSampleService.GetGameSample(nameSpec);

            var standardGameValidationResult = _filenameValidationService.ValidateGameFilename(gameSampleResult);

            var validationFailures = new List<ValidationFailure>();

            validationFailures.AddIfNotNull(standardGameValidationResult);

            if (validationFailures.Any())
            {
                throw new ValidationException(validationFailures.DistinctBy(v => v.PropertyName).ToArray());
            }
        }
    }
}
