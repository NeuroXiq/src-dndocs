using DNDocs.Api.DTO.Enum;

namespace DNDocs.Api.DTO.Shared
{
    public class BgJobDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime QueuedDateTime { get; set; }
        public DateTime? StartedDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public BgJobStatus Status { get; set; }
        public string DoWorkCommandType { get; set; }
        public string DoWorkCommandData { get; set; }
        public int? CreateByUserId { get; set; }

    }
}
