namespace DNDocs.Docs.Web.Model
{
    public class MtMeasurement
    {
        public int Id { get; set; }
        public int MtInstrumentId { get; set; }
        public double Value { get; set; }
        public int? MtHRangeId { get; set; }
        public DateTime CreatedOn { get; set; }

        public MtMeasurement() { }

        public MtMeasurement(int mtInstrumentId, double value, int? mthrangeId)
        {
            MtInstrumentId = mtInstrumentId;
            Value = value;
            MtHRangeId = mthrangeId;
        }
    }
}
