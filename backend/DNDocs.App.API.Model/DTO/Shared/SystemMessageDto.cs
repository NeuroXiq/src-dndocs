using DNDocs.Api.DTO.Enum;

namespace DNDocs.Api.DTO.Shared
{
    public class SystemMessageDto
    {
        public int Id { get; set; }
        public SystemMessageType Type { get; set; }
        public SystemMessageLevel Level { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
        public int? UserId { get; set; }
        public int? ProjectId { get; set; }
    }
}
