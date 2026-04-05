namespace DNDocs.IntegrationTests
{
    public class IntegrationTestsBase
    {
        protected HttpClient DNDocsClient { get; set; }
        protected HttpClient DNDocsDocsClient { get; set; }
        protected HttpClient DNDocsJobClient { get; set; }
    }
}
