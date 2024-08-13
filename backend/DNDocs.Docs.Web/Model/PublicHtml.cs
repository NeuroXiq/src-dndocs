
namespace DNDocs.Docs.Web.Model
{
    public class PublicHtml
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public byte[] ByteData { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        public PublicHtml() { }

        public PublicHtml(string path, byte[] byteData)
        {
            Path = path;
            ByteData = byteData;
            CreatedOn = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}
