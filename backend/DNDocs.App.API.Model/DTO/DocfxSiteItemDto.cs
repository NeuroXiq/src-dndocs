namespace DNDocs.Api.DTO
{
    public class DocfxSiteItemDto
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public byte[] Content { get; set; }

        public DocfxSiteItemDto() { }

        public DocfxSiteItemDto(int id, string path, byte[] content)
        {
            Id = id;
            Path = path;
            Content = content;
        }
    }
}
