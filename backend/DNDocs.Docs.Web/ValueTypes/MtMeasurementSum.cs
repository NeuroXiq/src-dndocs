namespace DNDocs.Docs.Web.ValueTypes
{
    public class MtMeasurementSum
    {
        public int InstrumentId { get; set; }
        public double Sum { get; set; }
        public int MtInstrumentId { get; set; }
        public int? MtHRangeId { get; set; }
        public double? MtHRangeEnd { get; set; }
        public string InstrumentName { get; set; }
        public string InstrumentTags { get; set; }
        public MtInstrumentType InstrumentType { get; set; }
    }
}
