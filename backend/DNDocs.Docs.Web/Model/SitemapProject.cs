namespace DNDocs.Docs.Web.Model
{
    public class SitemapProject
    {
        public long Id { get; set; }
        public long PublicHtmlId { get; set; }
        public long ProjectId { get; set; }

        public SitemapProject() { }
        
        public SitemapProject(long publicHtmlId, long projectId)
        {
            PublicHtmlId = publicHtmlId;
            ProjectId = projectId;
        }
    }
}
