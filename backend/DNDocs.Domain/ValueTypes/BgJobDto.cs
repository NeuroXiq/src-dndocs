using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Enums;

namespace DNDocs.Domain.ValueTypes
{
    public class BgJobDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime QueuedDateTime { get; set; }
        public DateTime? StartedDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public BgJobStatus Status { get; set; }
        public string DescriptionText { get; set; }
        public string ResultText { get; set; }

        public BgJobDto() { }
    }
}
