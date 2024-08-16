namespace DNDocs.Docs.Web.Model
{
    public class Sitemap
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public long DecompressedLength { get; set; }
        public long UrlsCount { get; set; }
        public byte[] ByteData { get; set; }
        public DateTime UpdatedOn { get; set; }

        public Sitemap() { }

        public Sitemap(string path, long decompressedLength, long urlsCount, byte[] byteData)
        {
            Path = path;
            DecompressedLength = decompressedLength;
            UrlsCount = urlsCount;
            ByteData = byteData;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}
