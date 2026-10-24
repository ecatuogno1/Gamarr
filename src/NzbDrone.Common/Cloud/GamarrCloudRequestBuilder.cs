using NzbDrone.Common.Http;

namespace NzbDrone.Common.Cloud
{
    public interface IGamarrCloudRequestBuilder
    {
        IHttpRequestBuilderFactory Services { get; }
    }

    // Gamarr: there are no Gamarr cloud services. The Radarr/Servarr hosts were removed; the ".invalid"
    // TLD (RFC 6761) never resolves, so anything still using these builders fails fast without leaving the machine.
    // Game metadata comes from IGDB (MetadataSource/Igdb), not from a Gamarr metadata server.
    public class GamarrCloudRequestBuilder : IGamarrCloudRequestBuilder
    {
        public GamarrCloudRequestBuilder()
        {
            Services = new HttpRequestBuilder("https://services.gamarr.invalid/v1/")
                .CreateFactory();
        }

        public IHttpRequestBuilderFactory Services { get; private set; }
    }
}
