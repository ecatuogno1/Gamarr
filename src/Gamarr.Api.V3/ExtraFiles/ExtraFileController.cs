using System.Collections.Generic;
using System.Linq;
using Gamarr.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Extras.Others;

namespace Gamarr.Api.V3.ExtraFiles
{
    [V3ApiController("extrafile")]
    public class ExtraFileController : Controller
    {
        private readonly IExtraFileService<MetadataFile> _metadataFileService;
        private readonly IExtraFileService<OtherExtraFile> _otherFileService;

        public ExtraFileController(IExtraFileService<MetadataFile> metadataFileService, IExtraFileService<OtherExtraFile> otherExtraFileService)
        {
            _metadataFileService = metadataFileService;
            _otherFileService = otherExtraFileService;
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<List<ExtraFileResource>> GetFiles(int gameId)
        {
            var extraFiles = new List<ExtraFileResource>();

            var metadataFiles = _metadataFileService.GetFilesByGame(gameId).OrderBy(f => f.RelativePath).ToList();
            var otherExtraFiles = _otherFileService.GetFilesByGame(gameId).OrderBy(f => f.RelativePath).ToList();

            extraFiles.AddRange(metadataFiles.ToResource());
            extraFiles.AddRange(otherExtraFiles.ToResource());

            return TypedResults.Ok(extraFiles);
        }
    }
}
