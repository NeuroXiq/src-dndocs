using DNDocs.Docs.Api.Client;
using DNDocs.Job.Api.Client;

namespace DNDocs.IntegrationTests
{
    public class IntegrationTestsBase
    {
        public DDocsApiClient DNDocsDocsApiClient { get; set; }

        public DNDocsJobApiClient DNDocsJobApiClient { get; set; }

        public HttpClient DNDocsHttpClient { get; set; }
    }
}
