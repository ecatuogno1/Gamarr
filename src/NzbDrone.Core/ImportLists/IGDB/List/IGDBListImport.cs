using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.IGDB.List
{
    public class IGDBListImport : IGDBImportListBase<IGDBListSettings>
    {
        public override string Name => "IGDB List";
        public override bool Enabled => true;
        public override bool EnableAuto => false;
        public override int PageSize => 1;

        public IGDBListImport(IGamarrCloudRequestBuilder requestBuilder,
                                 IHttpClient httpClient,
                                 IImportListStatusService importListStatusService,
                                 IConfigService configService,
                                 IParsingService parsingService,
                                 ISearchForNewGame searchForNewGame,
                                 Logger logger)
        : base(requestBuilder, httpClient, importListStatusService, configService, parsingService, searchForNewGame, logger)
        {
        }

        public override IParseImportListResponse GetParser()
        {
            return new IGDBListParser();
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new IGDBListRequestGenerator
            {
                RequestBuilder = _requestBuilder,
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient
            };
        }
    }
}
