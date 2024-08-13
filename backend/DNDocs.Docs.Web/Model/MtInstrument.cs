using DNDocs.Docs.Web.ValueTypes;

namespace DNDocs.Docs.Web.Model
{
    public class MtInstrument
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MeterName { get; set; }
        public string InstanceId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Tags { get; set; }
        public MtInstrumentType Type { get; set; }

        public MtInstrument() { }

        public MtInstrument(string name, string meterName, string instanceId, MtInstrumentType type, string tags)
        {
            Name = name;
            MeterName = meterName;
            InstanceId = instanceId;
            Tags = tags;
            Type = type;
            CreatedOn = DateTime.UtcNow;
        }
    }
}
