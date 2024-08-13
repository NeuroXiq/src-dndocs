namespace DNDocs.Api.DTO
{
    public class DocfxContentItemDto
    {
        public enum ContentType
        {
            Folder = 1,
            Yml = 2,
            Md = 3
        }

        public string Directory { get; set; }
        public string Name { get; set; }
        public ContentType Type { get; set; }
        public List<DocfxContentItemDto> Children { get; set; }

        public DocfxContentItemDto(string directory, string name, ContentType type, List<DocfxContentItemDto> children)
        {
            Directory = directory;
            Name = name;
            Type = type;
            Children = children;
        }
    }
}
