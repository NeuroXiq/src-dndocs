using DNDocs.Job.Web.ValueTypes;

namespace DNDocs.Job.Web.Model
{
    public class BgJob
    {
        public int Id { get; set; }
        public BgJobState State { get; set; }
        public string BuildData { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime StartOn { get; set; }
        public DateTime CompletedOn { get; set; }
        public string Exception { get; set; }
        public string Lock { get; set; }
    }
}
