namespace DNDocs.Docs.Web.Model
{
    public class SiteItem
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string Path { get; set; }
        public long? SharedSiteItemId { get; set; }
        public byte[] ByteData { get; set; }

        public SiteItem() { }
        
        public SiteItem(long projectId, string path, byte[] byteData)
        {
            ProjectId = projectId;
            Path = path;
            ByteData = byteData;
        }
    }
}
