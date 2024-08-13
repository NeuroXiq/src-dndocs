namespace DNDocs.Docs.Web.Model
{
    public class SharedSiteItem
    {
        public long Id { get; set; }
        // path is needed? probably not leaving for debug
        public string Path { get; set; }
        public byte[] ByteData { get; set; }
        public string Sha256 { get; set; }

        public SharedSiteItem() { }

        public SharedSiteItem(string path,
            byte[] byteData,
            string sha256
            )
        {
            Path = path;
            ByteData = byteData;
            Sha256 = sha256;
        }
    }
}
