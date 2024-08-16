namespace DNDocs.Docs.Web.Model
{
    public class SitemapProject
    {
        public long Id { get; set; }
        public long SitemapId { get; set; }
        public long ProjectId { get; set; }

        public SitemapProject() { }
        
        public SitemapProject(long sitemapId, long projectId)
        {
            SitemapId = sitemapId;
            ProjectId = projectId;
        }
    }
}
