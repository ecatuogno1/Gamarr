using System.Collections.Generic;
using System.Linq;
using Gamarr.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Extras.Companions;
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
        private readonly IExtraFileService<CompanionFile> _companionFileService;

        public ExtraFileController(IExtraFileService<MetadataFile> metadataFileService, IExtraFileService<OtherExtraFile> otherExtraFileService, IExtraFileService<CompanionFile> companionFileService)
        {
            _metadataFileService = metadataFileService;
            _otherFileService = otherExtraFileService;
            _companionFileService = companionFileService;
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<List<ExtraFileResource>> GetFiles(int gameId)
        {
            var extraFiles = new List<ExtraFileResource>();

            var metadataFiles = _metadataFileService.GetFilesByGame(gameId).OrderBy(f => f.RelativePath).ToList();
            var otherExtraFiles = _otherFileService.GetFilesByGame(gameId).OrderBy(f => f.RelativePath).ToList();
            var companionFiles = _companionFileService.GetFilesByGame(gameId).OrderBy(f => f.RelativePath).ToList();

            // Companion files (bin tracks, other discs, installer parts) first, they belong to the game file
            extraFiles.AddRange(companionFiles.ToResource());
            extraFiles.AddRange(metadataFiles.ToResource());
            extraFiles.AddRange(otherExtraFiles.ToResource());

            return TypedResults.Ok(extraFiles);
        }
    }
}
