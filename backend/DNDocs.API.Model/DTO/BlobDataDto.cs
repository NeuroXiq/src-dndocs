namespace DNDocs.Api.DTO
{
    public class BlobDataDto
    {
        public int Id { get; set; }
        public string OriginalName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModified { get; set; }

        public BlobDataDto() { }

        public BlobDataDto(int id,
            string originalName,
            DateTime createdon,
            DateTime lastmodified)
        {
            Id = id;
            OriginalName = originalName;
            CreatedOn = createdon;
            LastModified = lastmodified;
        }
    }
}
